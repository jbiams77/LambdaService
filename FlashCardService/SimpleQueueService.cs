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
using System.Text.Json;

namespace FlashCardService
{
    class SQS
    {
        private AmazonSQSConfig sqsConfig;
        private AmazonSQSClient sqsClient;

        public string QueueURL { get; set; }
        private S3 PolicyBucket = new S3();

        private readonly string SQS_POLICY_FILE = "sqsPolicyTemplate.json";
        private readonly string DEFAULT_SQS_URL = "https://sqs.us-west-2.amazonaws.com/151435209032/flashCardService-default";

        // Queue settings
        private readonly int MAX_MESSAGE_SIZE = 256 * 1024;
        private readonly TimeSpan MESSAGE_SEND_DELAY = TimeSpan.FromSeconds(0);
        private readonly TimeSpan MESSAGE_RETENTION_PERIOD = TimeSpan.FromMinutes(3);
        private readonly TimeSpan RECEIVED_MESSAGE_WAIT_TIME = TimeSpan.FromSeconds(20);
        private readonly TimeSpan VISIBILITY_TIMEOUT = TimeSpan.FromSeconds(30);

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

            string queueUrl = DEFAULT_SQS_URL;

            var queueExists = await GetExistingQueueUrl(queueName);
            if (queueExists.Item1)
            {
                // Queue already exists for this user, no need to create a new one
                queueUrl = queueExists.Item2;
            }
            else
            {
                // Queue not found. Create a new one
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

            // Always reset the queue attributes so that any changed queue settings take effect
            await SetQueueAttributes(queueUrl);

            return queueUrl;
        }

        /// <summary>
        /// Sends a message on the queue. Message must be serializable to json or an exception will be thrown.
        /// </summary>
        /// <param name="message">message that will be sent</param>
        public async Task Send<MessageType>(MessageType message)
        {
            string messageAsJson = ToJson(message);
            Function.info.Log("Attempting to send on SQS\n: " + messageAsJson);
            if (messageAsJson != "")
            {
                SendMessageRequest smr = new SendMessageRequest(QueueURL, messageAsJson);
                try
                {
                    await sqsClient.SendMessageAsync(smr);
                    Function.info.Log("Sent message to " + QueueURL + ": \n" + messageAsJson);
                }
                catch (Exception e)
                {
                    Function.info.Log("Failed to send message to " + QueueURL + ": " + e);
                }
            }
        }

        // Queries SQS to see if given queueName already exists
        // Returns boolean indicating if the queue exists. If the queue exists, then the queue URL is also returned.
        // Case queue exists: { true, queueURL } is returned
        // Case queue not found: { false, null } is returned
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

        // Queue settings
        private async Task SetQueueAttributes(string queueUrl)
        {
            var queueSettings = new Dictionary<string, string>();
            queueSettings.Add(QueueAttributeName.DelaySeconds,
              MESSAGE_SEND_DELAY.TotalSeconds.ToString());
            queueSettings.Add(QueueAttributeName.MaximumMessageSize, MAX_MESSAGE_SIZE.ToString());
            queueSettings.Add(QueueAttributeName.MessageRetentionPeriod,
              MESSAGE_RETENTION_PERIOD.TotalSeconds.ToString());
            queueSettings.Add(QueueAttributeName.ReceiveMessageWaitTimeSeconds,
              RECEIVED_MESSAGE_WAIT_TIME.TotalSeconds.ToString());
            queueSettings.Add(QueueAttributeName.VisibilityTimeout,
              VISIBILITY_TIMEOUT.TotalSeconds.ToString());

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

        // Gets the Queue policy template from S3 and populates it with the details of this queue
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

        private string ToJson<T>(T rawObject)
        {
            try
            {
                return JsonSerializer.Serialize(rawObject);
            }
            catch (Exception e)
            {
                Function.info.Log("[SQS] Failed to convert object to json: " + e);
                return "";
            }
        }

    }
}
