using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using System;
using System.Collections.Generic;
using System.Text;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace FlashCardService
{
    class SQS
    {
        private AmazonSQSConfig sqsConfig;
        private AmazonSQSClient sqsClient;

        public string QueueURL { get; set; }
        private S3 PolicyBucket = new S3();

        // Config files are relative to the runtime executable
        private readonly string SQS_POLICY_FILE = "sqsPolicyTemplate.json";

        public SQS()
        {
            sqsConfig = new AmazonSQSConfig();
            sqsClient = new AmazonSQSClient(sqsConfig);
            sqsConfig.ServiceURL = "http://sqs.us-west-2.amazonaws.com";
        }


        // Creates the Queue if it doesnt already exist and updates the queue settings
        // Returns the URL of the queue
        public async Task<string> CreateQueue(string username)
        {
            // Queue Name can only contain alphanumeric charachters, dashes, and underscores
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            string usernameAlpha = rgx.Replace(username, "");
            var queueName = "flashCardService-" + usernameAlpha;

            string queueUrl = "https://sqs.us-west-2.amazonaws.com/151435209032/flashCardService-default";

            var queueExists = await GetExistingQueueUrl(queueName);
            if (queueExists.Item1)
            {
                queueUrl = queueExists.Item2;
            }
            else
            {
                CreateQueueRequest createQueueRequest = new CreateQueueRequest();
                createQueueRequest.QueueName = queueName;

                try
                {
                    var response = await sqsClient.CreateQueueAsync(createQueueRequest);
                    queueUrl = response.QueueUrl;
                }
                catch (Exception e)
                {
                    Function.info.Log("Failed to create queue " + createQueueRequest.QueueName + ": " + e);
                }
            }

            // Set the queue attributes after attempting to create it so that if it already existed, we still update the attributes.
            await SetQueueAttributes(queueUrl);

            return queueUrl;
        }

        private async Task<Tuple<bool, string>> GetExistingQueueUrl(string queueName)
        {
            try
            {
                var response = await sqsClient.GetQueueUrlAsync(queueName);
                var queueUrl = response.QueueUrl;
                return Tuple.Create(true, queueUrl);
            }
            catch (Exception e)
            {
                Function.info.Log("Failed to get existing queue url for " + queueName + ": " + e);
            }

            return Tuple.Create(false, "");
        }

        private async Task SetQueueAttributes(string queueUrl)
        {
            // Maximum message size of 256 KiB (1,024 bytes * 256 KiB = 262,144 bytes).
            int maxMessage = 256 * 1024;

            var queueSettings = new Dictionary<string, string>();
            queueSettings.Add(QueueAttributeName.DelaySeconds,
              TimeSpan.FromSeconds(0).TotalSeconds.ToString());
            queueSettings.Add(QueueAttributeName.MaximumMessageSize, maxMessage.ToString());
            queueSettings.Add(QueueAttributeName.MessageRetentionPeriod,
              TimeSpan.FromMinutes(3).TotalSeconds.ToString());
            queueSettings.Add(QueueAttributeName.ReceiveMessageWaitTimeSeconds,
              TimeSpan.FromSeconds(20).TotalSeconds.ToString());
            queueSettings.Add(QueueAttributeName.VisibilityTimeout,
              TimeSpan.FromSeconds(30).TotalSeconds.ToString());

            var queuePolicy = await GetQueuePolicy(queueUrl);
            if (queuePolicy != "")
            {
                queueSettings.Add(QueueAttributeName.Policy, queuePolicy);
            }

            var request = new SetQueueAttributesRequest
            {
                QueueUrl = queueUrl,
                Attributes = queueSettings,
            };

            try
            {
                await sqsClient.SetQueueAttributesAsync(request);
            }
            catch (Exception e)
            {
                Function.info.Log("Failed to set queue attributes: " + e);
            }
        }

        private async Task<string> GetQueuePolicy(string queueUrl)
        {
            var policyTemplate = await PolicyBucket.GetFile(SQS_POLICY_FILE);

            try
            {
                // This is bad code. Dont put everything in a single try catch. But it works..
                // Parse the URL to get account information.
                var accountId = queueUrl.Split("/")[3];
                var queueName = queueUrl.Split("/")[4];
                var region = queueUrl.Split("/")[2].Split(".")[1];

                return policyTemplate.Replace("${AccountId}", accountId).Replace("${QueueName}", queueName).Replace("${Region}", region);
            }
            catch (IndexOutOfRangeException e)
            {
                Function.info.Log("Unable to create sqs policy: " + e);
            }

            return "";
        }

        public async Task SendMessageToSQS(string jsonMessage)
        {
            SendMessageRequest smr = new SendMessageRequest(QueueURL, jsonMessage);
            try
            {
                await sqsClient.SendMessageAsync(smr);
            }
            catch (Exception e)
            {
                Function.info.Log("Unable to send message to " + QueueURL + ": " + e);
            }
        }



    }
}
