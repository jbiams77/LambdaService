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
        SQS sqs;

        public async Task<SkillResponse> FunctionHandler(SkillRequest input, ILambdaContext context)
        {

            Type T = input.GetRequestType();
            info = context.Logger;
            CognitoUserPool cognitoUserPool = new CognitoUserPool();
            this.userId = await cognitoUserPool.GetUsername(input.Session.User.AccessToken);

            this.liveSession = new LiveSessionDB(userId);
            this.userProfile = new UserProfileDB(userId);
            this.scopeAndSequence = new ScopeAndSequenceDB();
            this.sqs = new SQS();
            await InitializeUserQueue();

            switch (T.Name)
            {
                case "LaunchRequest":
                    response = await HandleLaunchRequest(input.APLSupported());
                    break;

                case "IntentRequest":
                    response = await HandleIntentRequest((IntentRequest)input.Request, input.APLSupported());
                    break;

                case "SessionEndedRequest":
                    await SetStateToOffAndExit();
                    response = AlexaResponse.Say("Session End");
                    break;

                default:
                    response = AlexaResponse.Say("Error");
                    break;
            }

            return response;
        }

        private async Task<SkillResponse> HandleIntentRequest(IntentRequest intent, bool displaySupported)
        {
            SkillResponse intentResponse;

            switch (intent.Intent.Name)
            {
                case "AMAZON.YesIntent":
                    await TransferDataFromUserProfileToLiveSession();
                    liveSession.CurrentState = STATE.FirstWord;
                    await UpdateLiveSessionDatabase();
                    await sqs.Send(GetSessionUpdate());
                    intentResponse = await HandleYesIntent(displaySupported);
                    break;
                case "AMAZON.NoIntent":
                    await SetStateToOffAndExit();
                    intentResponse = ResponseBuilder.Tell("No intent.");
                    break;
                case "AMAZON.CancelIntent":
                    await SetStateToOffAndExit();
                    intentResponse = ResponseBuilder.Tell("Cancel intent.");
                    break;
                case "AMAZON.FallbackIntent":
                    await SetStateToOffAndExit();
                    intentResponse = ResponseBuilder.Tell("Fallback intent");
                    break;
                case "AMAZON.StopIntent":
                    await SetStateToOffAndExit();
                    intentResponse = ResponseBuilder.Tell("Goodbye.");
                    break;
                case "AMAZON.HelpIntent":
                    intentResponse = ResponseBuilder.Tell("Help intent.");
                    break;
                case "WordsToReadIntent":
                    intentResponse = await HandleWordsToReadIntent(intent, displaySupported);
                    await sqs.Send(GetSessionUpdate());
                    break;
                default:
                    intentResponse = ResponseBuilder.Tell("Unhandled intent.");
                    break;
            }
            return intentResponse;
        }
        private async Task SetStateToOffAndExit()
        {
            await TransferDataFromUserProfileToLiveSession();
            liveSession.CurrentState = STATE.Off;
            await UpdateLiveSessionDatabase();
            await sqs.Send(GetSessionUpdate());
        }

        private async Task<SkillResponse> HandleLaunchRequest(bool displaySupported)
        {   
            await TransferDataFromUserProfileToLiveSession();
            await UpdateLiveSessionDatabase();
            await sqs.Send(GetSessionUpdate());

            if (liveSession.TeachMode == MODE.Teach)
            {
                WordAttributes wordAttributes = await WordAttributes.GetWordAttributes(liveSession.GetCurrentWord());
                return TeachMode.Introduction(liveSession, wordAttributes, displaySupported);
            }
            else
            {
                return AlexaResponse.Introduction(displaySupported);
            }
            
        }

        private async Task<SkillResponse> HandleYesIntent(bool displaySupported)
        {
            await liveSession.GetDataFromLiveSession();

            string currentWord = liveSession.GetCurrentWord();

            string prompt = "Say the word on the flash card";

            return AlexaResponse.GetResponse(currentWord, prompt, prompt, displaySupported);
        }

        private async Task<SkillResponse> HandleWordsToReadIntent(IntentRequest intent, bool displaySupported)
        {

            await liveSession.GetDataFromLiveSession();

            string currentWord = liveSession.GetCurrentWord();

            string prompt = "Say the word";

            if (ReaderSaidTheWord(intent))
            {
                prompt = "Great!";  

                if (liveSession.Remove(currentWord))
                {
                    prompt = "Congratulations! You finished this session! Another reading assession awaits you. Just say, Alexa, open Moycan Readers!";                    
                    await this.userProfile.RemoveCompletedScheduleFromUserProfile(liveSession.CurrentSchedule);
                    liveSession.CurrentState = STATE.Off;
                    info.LogLine("SESSION COMPLETED.");
                    return ResponseBuilder.Tell(prompt);
                }
                else
                {
                    currentWord = liveSession.GetCurrentWord();
                    liveSession.CurrentState = STATE.Assess;
                    LogSessionInfo(liveSession, info);
                }

                await UpdateLiveSessionDatabase();
            }

            return AlexaResponse.GetResponse(currentWord, prompt, prompt, displaySupported);
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
            liveSession.wordsToRead = scopeAndSequence.WordsToRead;
            liveSession.TeachMode = (MODE)(int.Parse(scopeAndSequence.TeachMode));
            liveSession.Skill = (SKILL)(int.Parse(scopeAndSequence.Skill));
            liveSession.Lesson = scopeAndSequence.Lesson;
            liveSession.CurrentSchedule = currentScheduleNumber;
            liveSession.CurrentState = STATE.Introduction;
            
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
            info.LogLine("Current State: " + session.CurrentState);
            info.LogLine("CLive Session Schedule: " + session.CurrentSchedule);

        }

        private MessageSchema.SessionUpdate GetSessionUpdate()
        {
            return new MessageSchema.SessionUpdate
            {
                CurrentWord = this.liveSession.GetCurrentWord(),
                CurrentSchedule = this.liveSession.CurrentSchedule,
                CurrentState = this.liveSession.CurrentState.ToString(),
                WordsRemaining = this.liveSession.GetWordsRemaining()
            };
        }
    }
}