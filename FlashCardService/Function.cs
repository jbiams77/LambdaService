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
using Moyca.Database.GlobalConstants;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace FlashCardService
{
    using DatabaseItem = Dictionary<string, AttributeValue>;
    public class Function
    {
        // Make logger static to give all classes access to it
        public static ILambdaLogger info;
        public LiveSessionDB liveSession;
        public UserProfileDB userProfile;
        public ScopeAndSequenceDB scopeAndSequence;
        public SkillResponse response;
        public string userId;

        public async Task<SkillResponse> FunctionHandler(SkillRequest input, ILambdaContext context)
        {           
            Type T = input.GetRequestType();
            info = context.Logger;
            
            this.userId = input.Session.User.UserId;

            this.liveSession = new LiveSessionDB(userId);
            this.userProfile = new UserProfileDB(userId);
            this.scopeAndSequence = new ScopeAndSequenceDB();

            info.LogLine("USERID: " + this.userId);

            switch (T.Name)
            {
                case "LaunchRequest":
                    await TransferDataFromUserProfileToLiveSession();
                    await UpdateLiveSessionDatabase();
                    response = AlexaResponse.Introduction();
                    break;

                case "IntentRequest":                    
                    response = await HandleIntentRequest((IntentRequest)input.Request);
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

        private async Task<SkillResponse> HandleIntentRequest(IntentRequest intent)
        {
            SkillResponse intentResponse;

            switch (intent.Intent.Name)
            {
                case "AMAZON.YesIntent":
                    intentResponse = await HandleYesIntent();
                    break;
                case "AMAZON.NoIntent":
                    intentResponse = ResponseBuilder.Tell("No intent.");
                    break;
                case "AMAZON.CancelIntent":
                    intentResponse = ResponseBuilder.Tell("Cancel intent.");
                    break;
                case "AMAZON.FallbackIntent":
                    intentResponse = ResponseBuilder.Tell("Fallback intent");
                    break;
                case "AMAZON.StopIntent":
                    intentResponse = ResponseBuilder.Tell("Stop intent.");
                    break;
                case "AMAZON.HelpIntent":
                    intentResponse = ResponseBuilder.Tell("Help intent.");
                    break;
                case "WordsToReadIntent":
                    intentResponse = await HandleWordsToReadIntent();
                    break;
                default:
                    intentResponse = ResponseBuilder.Tell("Unhandled intent.");
                    break;
            }
            return intentResponse;
        }

        private async Task<SkillResponse> HandleYesIntent()
        {
            await liveSession.GetDataFromLiveSession();

            string currentWord = liveSession.GetCurrentWord();

            string prompt = "Say the word on the flash card";

            return AlexaResponse.GetResponse(currentWord, prompt, prompt);
        }

        private async Task<SkillResponse> HandleWordsToReadIntent()
        {            

            await liveSession.GetDataFromLiveSession();
            string prompt = liveSession.GetCurrentWord();

            return ResponseBuilder.Tell("Say " + prompt);
        }

        private async Task TransferDataFromUserProfileToLiveSession()
        {            
            int currentScheduleNumber = await userProfile.GetFirstScheduleNumber();            
            await scopeAndSequence.GetSessionDataWithNumber(currentScheduleNumber);
            liveSession.wordsToRead = scopeAndSequence.wordsToRead;
            liveSession.TeachMode = (MODE)(int.Parse(scopeAndSequence.teachMode));
            liveSession.Skill = (SKILL)(int.Parse(scopeAndSequence.skill));
            liveSession.State = STATE.Introduction;
        }

        private async Task UpdateLiveSessionDatabase( )
        {
            await liveSession.UpdateLiveSession(this.userId, liveSession.wordsToRead,
                                                             liveSession.TeachMode.ToString(),
                                                             liveSession.Skill.ToString(),
                                                             liveSession.State.ToString());
        }
    }
}