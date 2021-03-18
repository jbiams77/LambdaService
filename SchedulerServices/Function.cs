using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Infrastructure.DynamoDB;
using Infrastructure.GlobalConstants;
using Infrastructure.Logger;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SchedulerService
{
    /// <summary>
    /// Outputs logs to labmda function
    /// </summary>
    public static class LOGGER
    {
        public static MoycaLogger log;
    }

    public class Function
    {
        // Make logger static to give all classes access to it
        public static ILambdaLogger info;
        ScopeAndSequenceDB scopeAndSequence;
        DictionaryDB dictionary;
        int totalWords = 0;
        public async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
        {
            LOGGER.log = MoycaLogger.GetLogger(context, LogLevel.TRACE);
            scopeAndSequence = new ScopeAndSequenceDB(LOGGER.log);
            dictionary = new DictionaryDB(LOGGER.log);
            int i;
            for(i=1057; i<1058; i++)
            {                
                await GetAndSetWords(i);
            }
            LOGGER.log.INFO("Function", "Words in range: " + totalWords);
        }

        public async Task GetAndSetWords(int orderNumber)
        {            
            await dictionary.GetWordsToReadWithOrder(await scopeAndSequence.GetOrder(orderNumber));
            totalWords += dictionary.GetSizeOfWordsToRead();
            await scopeAndSequence.PutItemBackWithWordsToRead(orderNumber, dictionary.GetWordsToRead());            
        }

    }
}