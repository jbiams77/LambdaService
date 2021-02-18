using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using AWSInfrastructure.DynamoDB;
using AWSInfrastructure.GlobalConstants;
using AWSInfrastructure.Logger;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SchedulerService
{

    public class Function
    {
        public static MoycaLogger log;
        // Make logger static to give all classes access to it
        public static ILambdaLogger info;
        ScopeAndSequenceDB scopeAndSequence;
        DictionaryDB dictionary;
        int totalWords = 0;
        public async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
        {
            log = new MoycaLogger(context, LogLevel.TRACE);
            scopeAndSequence = new ScopeAndSequenceDB(log);
            dictionary = new DictionaryDB(log);
            int i;
            for(i=1036; i<1096; i++)
            {                
                await GetAndSetWords(i);
            }
            log.INFO("Function", "Words in range: " + totalWords);
        }

        public async Task GetAndSetWords(int orderNumber)
        {            
            await dictionary.GetWordsToReadWithOrder(await scopeAndSequence.GetOrder(orderNumber));
            totalWords += dictionary.GetSizeOfWordsToRead();
            await scopeAndSequence.PutItemBackWithWordsToRead(orderNumber, dictionary.GetWordsToRead());            
        }

    }
}