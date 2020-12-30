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
        public List<string> wordsToRead;
        public MODE teachMode;
        public STATE state;
        public SKILL skill;
        public string userId;

        public async Task<SkillResponse> FunctionHandler(SkillRequest input, ILambdaContext context)
        {           
            Type T = input.GetRequestType();
            info = context.Logger;
            
            this.userId = input.Session.User.UserId;

            this.liveSession = new LiveSessionDB(userId);
            this.userProfile = new UserProfileDB(userId);
            this.scopeAndSequence = new ScopeAndSequenceDB();

            switch (T.Name)
            {
                case "LaunchRequest":
                    await GetSessionDataFromSchedule();
                    await UpdateLiveSessionDatabase();
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

        private async Task GetSessionDataFromSchedule()
        {            
            int currentScheduleNumber = await userProfile.GetFirstScheduleNumber();            
            await scopeAndSequence.GetSessionDataWithNumber(currentScheduleNumber);
            this.wordsToRead = scopeAndSequence.wordsToRead;
            this.teachMode = (MODE)(int.Parse(scopeAndSequence.teachMode));
            this.skill = (SKILL)(int.Parse(scopeAndSequence.skill));
            this.state = STATE.Introduction;
        }

        private async Task UpdateLiveSessionDatabase( )
        {
            await liveSession.UpdateLiveSession(this.userId, scopeAndSequence.wordsToRead,
                                                             scopeAndSequence.teachMode, 
                                                             scopeAndSequence.skill, 
                                                             nameof(this.state));
        }
    }
}