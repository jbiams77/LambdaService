using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Amazon.DynamoDBv2.Model;
using Alexa.NET.Response.Converters;
using Alexa.NET.Response.Directive;
using Newtonsoft.Json;
using Alexa.NET;
using Moyca.Database;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace FlashCardService
{
    using DatabaseItem = Dictionary<string, AttributeValue>;
    public class Function
    {
        // Make logger static to give all classes access to it
        public static ILambdaLogger info;

        public async Task<SkillResponse> FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            Type T = input.GetRequestType();
            info = context.Logger;
            SkillResponse response;
            string userId = input.Session.User.UserId;


            switch (T.Name)
            {
                case "LaunchRequest":
                    List<string> wordsToRead = await GetWordsToRead(userId);
                    await UpdateLiveSession(userId);
                    response = AlexaResponse.Say("Launch");
                    break;

                case "IntentRequest":
                    response = AlexaResponse.Say("Intent");
                    break;

                case "SessionEndedRequest":
                    response = AlexaResponse.Say("Session End"); 
                    break;

                default:
                    response = AlexaResponse.Say("Error");
                    break;
            }

            return response;
        }

        private async Task<List<string>> GetWordsToRead(string userId)
        {
            UserProfileDB userProfile = new UserProfileDB(userId);
            int currentScheduleNumber = await userProfile.GetFirstSchedule();
            ScopeAndSequenceDB scopeAndSequence = new ScopeAndSequenceDB();
            return await scopeAndSequence.GetWordsToReadWithNumber(currentScheduleNumber);
        }

        private async Task UpdateLiveSession(string userId)
        {

        }
    }
}