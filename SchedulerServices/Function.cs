using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Moyca.Database;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SchedulerService
{

    public class Function
    {
        // Make logger static to give all classes access to it
        public static ILambdaLogger info;
        ScopeAndSequenceDB scopeAndSequence;
        DictionaryDB dictionary;

        public async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
        {
            scopeAndSequence = new ScopeAndSequenceDB();
            dictionary = new DictionaryDB();
            int i;
            for(i=1000; i<1005; i++)
            {
                await GetAndSetWords(i);
            }

            
        }

        public async Task GetAndSetWords(int orderNumber)
        {            
            await dictionary.GetWordsToReadWithOrder(await scopeAndSequence.GetOrder(orderNumber));
            await scopeAndSequence.PutItemBackWithWordsToRead(orderNumber, dictionary.GetWordsToRead());
        }

    }
}