﻿using System;
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
        SQS sqs;

        public async Task<SkillResponse> FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            
            Type T = input.GetRequestType();
            info = context.Logger;
            CognitoUserPool cognitoUserPool = new CognitoUserPool();
            this.userId = await GetUsername(cognitoUserPool, input.Session.User.AccessToken);            

            this.liveSession = new LiveSessionDB(userId);
            this.userProfile = new UserProfileDB(userId);
            this.scopeAndSequence = new ScopeAndSequenceDB();
            this.sqs = new SQS();

            await InitializeUserQueue();                        

            switch (T.Name)
            {
                case "LaunchRequest":
                    await TransferDataFromUserProfileToLiveSession();
                    await UpdateLiveSessionDatabase();                    
                    response = AlexaResponse.Introduction();
                    LogSessionInfo(liveSession, info);
                    await sqs.SendMessageToSQS(FormatSessionDataAsJSON());
                    break;

                case "IntentRequest":                    
                    response = await HandleIntentRequest((IntentRequest)input.Request);
                    break;

                case "SessionEndedRequest":
                    await TransferDataFromUserProfileToLiveSession();
                    liveSession.State = STATE.Off;
                    await UpdateLiveSessionDatabase();
                    await sqs.SendMessageToSQS(FormatSessionDataAsJSON());
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
                    await TransferDataFromUserProfileToLiveSession();
                    liveSession.State = STATE.FirstWord;
                    await UpdateLiveSessionDatabase();
                    await sqs.SendMessageToSQS(FormatSessionDataAsJSON());
                    intentResponse = await HandleYesIntent();
                    break;
                case "AMAZON.NoIntent":
                    await TransferDataFromUserProfileToLiveSession();
                    liveSession.State = STATE.Off;
                    await UpdateLiveSessionDatabase();
                    await sqs.SendMessageToSQS(FormatSessionDataAsJSON());
                    intentResponse = ResponseBuilder.Tell("No intent.");
                    break;
                case "AMAZON.CancelIntent":
                    await TransferDataFromUserProfileToLiveSession();
                    liveSession.State = STATE.Off;
                    await UpdateLiveSessionDatabase();
                    await sqs.SendMessageToSQS(FormatSessionDataAsJSON());
                    intentResponse = ResponseBuilder.Tell("Cancel intent.");
                    break;
                case "AMAZON.FallbackIntent":
                    await TransferDataFromUserProfileToLiveSession();
                    liveSession.State = STATE.Off;
                    await UpdateLiveSessionDatabase();
                    await sqs.SendMessageToSQS(FormatSessionDataAsJSON());
                    intentResponse = ResponseBuilder.Tell("Fallback intent");
                    break;
                case "AMAZON.StopIntent":
                    await TransferDataFromUserProfileToLiveSession();
                    liveSession.State = STATE.Off;
                    await UpdateLiveSessionDatabase();
                    await sqs.SendMessageToSQS(FormatSessionDataAsJSON());
                    intentResponse = ResponseBuilder.Tell("Goodbye.");
                    break;
                case "AMAZON.HelpIntent":
                    intentResponse = ResponseBuilder.Tell("Help intent.");
                    break;
                case "WordsToReadIntent":
                    intentResponse = await HandleWordsToReadIntent(intent);
                    LogSessionInfo(liveSession, info);
                    await sqs.SendMessageToSQS(FormatSessionDataAsJSON());
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

        private async Task<SkillResponse> HandleWordsToReadIntent(IntentRequest intent)
        {            

            await liveSession.GetDataFromLiveSession();

            string currentWord = liveSession.GetCurrentWord();

            string prompt = "Say the word";

            if (ReaderSaidTheWord(intent))
            {
                prompt = "Great!";
                if (liveSession.Remove(currentWord))
                {   
                    prompt = "Congratulations! You can move on to the next session.";
                    await this.userProfile.RemoveCompletedScheduleFromUserProfile(liveSession.CurrentSchedule);
                    await TransferDataFromUserProfileToLiveSession();
                }
                
                currentWord = liveSession.GetCurrentWord();
                liveSession.State = STATE.Assess;
                await UpdateLiveSessionDatabase();
                LogSessionInfo(liveSession, info);
            }

            return AlexaResponse.GetResponse(currentWord, prompt, prompt);
        }
        // Creates or updates the queue and sets the queue URL in the user database
        public async Task InitializeUserQueue()
        {
            this.sqs.QueueURL = await sqs.CreateQueue(this.userId);
            await this.userProfile.SetQueueUrl(this.sqs.QueueURL);
        }

        private async Task TransferDataFromUserProfileToLiveSession()
        {            
            int currentScheduleNumber = await userProfile.GetFirstScheduleNumber();            
            await scopeAndSequence.GetSessionDataWithNumber(currentScheduleNumber);
            liveSession.wordsToRead = scopeAndSequence.wordsToRead;
            liveSession.TeachMode = (MODE)(int.Parse(scopeAndSequence.teachMode));
            liveSession.Skill = (SKILL)(int.Parse(scopeAndSequence.skill));
            liveSession.State = STATE.Introduction;
            liveSession.CurrentSchedule = currentScheduleNumber;
        }

        private async Task UpdateLiveSessionDatabase()
        {
            await liveSession.UpdateLiveSession();
        }

        private bool ReaderSaidTheWord(IntentRequest input)
        {   

            foreach (ResolutionAuthority auth in input.Intent.Slots.Last().Value.Resolution.Authorities)
            {
                if (auth.Status.Code == ResolutionStatusCode.SuccessfulMatch)
                {
                    return true;
                }
            }

            return false;
        }

        private void LogSessionInfo(LiveSessionDB session, ILambdaLogger info)
        {            
            info.LogLine("Teach Mode: " + session.TeachMode);
            info.LogLine("Current State: " + session.State);            
            info.LogLine("CLive Session Schedule: " + session.CurrentSchedule);

        }

        private string FormatSessionDataAsJSON()
        {
            return "{CurrentWord:" + this.liveSession.GetCurrentWord() +
                   ", CurrentSchedule:" + this.liveSession.CurrentSchedule +
                   ", WordsRemaining:" + this.liveSession.GetWordsRemaining() + 
                    ", CurrentState: " + this.liveSession.State.ToString() + "}";
        }

        private async Task<string> GetUsername(CognitoUserPool userPool, string accessToken)
        {
            if (accessToken != null )
            {
                var userData = await userPool.GetUserData(accessToken);

                if (userData != null)
                {
                    return userData.Username;
                }
            }

            return "default";
        }

    }
}