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
using AWSInfrastructure.DynamoDB;
using AWSInfrastructure.GlobalConstants;
using AWSInfrastructure.CognitoPool;
using AWSInfrastructure.Logger;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace FlashCardService
{
    using DatabaseItem = Dictionary<string, AttributeValue>;
    
    public class Function
    {
        public static MoycaLogger log;
        public LiveSessionDB liveSession;
        public UserProfileDB userProfile;
        public ScopeAndSequenceDB scopeAndSequence;
        public SkillResponse response;
        public string userId;        
        private CognitoUserPool cognitoUserPool;

        public async Task<SkillResponse> FunctionHandler(SkillRequest input, ILambdaContext context)
        {

            Type T = input.GetRequestType();
            log = new MoycaLogger(context, LogLevel.TRACE);
            AlexaResponse.SetLogger(log);

            this.cognitoUserPool = new CognitoUserPool(log);
            this.userId = await cognitoUserPool.GetUsername(input.Session.User.AccessToken);
            this.liveSession = new LiveSessionDB(userId, log);
            this.userProfile = new UserProfileDB(userId, log);
            this.scopeAndSequence = new ScopeAndSequenceDB(log);
            log.INFO("BEGIN", "-----------------------------------------------------------------------");
            log.INFO("Function", "USERID: " + this.userId);
            log.INFO("Function", "LaunchRequest: " + T.Name);

            AlexaResponse.SetDisplaySupported(input.APLSupported());

            switch (T.Name)
            {
                case "LaunchRequest":                    
                    response = await HandleLaunchRequest();
                    break;

                case "IntentRequest":                    
                    response = await HandleIntentRequest((IntentRequest)input.Request);
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

        private async Task<SkillResponse> HandleIntentRequest(IntentRequest intent)
        {
            SkillResponse intentResponse;

            log.INFO("Function", "HandleIntentRequest", intent.Intent.Name);

            switch (intent.Intent.Name)
            {
                case "AMAZON.YesIntent":
                    intentResponse = await HandleYesIntent();
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
                    intentResponse = await HandleWordsToReadIntent(intent);
                    break;
                case "AMAZON.StopIntent":
                    await SetStateToOffAndExit();
                    intentResponse = ResponseBuilder.Tell("Goodbye.");
                    break;
                case "AMAZON.HelpIntent":
                    intentResponse = ResponseBuilder.Tell("Help intent.");
                    break;
                case "WordsToReadIntent":
                    intentResponse = await HandleWordsToReadIntent(intent);
                    break;
                default:
                    intentResponse = ResponseBuilder.Tell("Unhandled intent.");
                    break;
            }

            return intentResponse;
        }
        private async Task SetStateToOffAndExit()
        {
            log.INFO("Function", "SetStateToOffAndExit", "Schedule: " + liveSession.CurrentSchedule);

            await TransferDataFromUserProfileToLiveSession();
            liveSession.CurrentState = STATE.Off;
            await UpdateLiveSessionDatabase();
        }

        private async Task<SkillResponse> HandleLaunchRequest()
        {
            log.INFO("Function", "HandleLaunchRequest", "Current Schedule: " + liveSession.CurrentSchedule);

            await TransferDataFromUserProfileToLiveSession();
            await UpdateLiveSessionDatabase();

            log.DEBUG("Function", "HandleLaunchRequest", "Teach Mode: " + liveSession.TeachMode.ToString());

            if (liveSession.TeachMode == MODE.Teach)
            {           
                WordAttributes wordAttributes = await WordAttributes.GetWordAttributes(liveSession.GetCurrentWord(), log);
                return TeachMode.Introduction(liveSession, wordAttributes);
            }
            else
            {                
                return AlexaResponse.Introduction("Greetings my fellow Moycan! Lets learn to read. Are you ready to begin ?", "Say yes or no to continue.");
            }
            
        }

        private async Task<SkillResponse> HandleYesIntent()
        {
            log.INFO("Function", "HandleYesIntent", "Current Schedule: " + liveSession.CurrentSchedule);

            await TransferDataFromUserProfileToLiveSession();
            
            liveSession.CurrentState = STATE.FirstWord;
            
            await UpdateLiveSessionDatabase();
            
            await liveSession.GetDataFromLiveSession();

            string currentWord = liveSession.GetCurrentWord();

            string prompt = "Say the word ";

            log.DEBUG("Function", "HandleYesIntent", "Teach Mode: " + liveSession.TeachMode.ToString());
            log.INFO("Function", "HandleYesIntent", "Current Word: " + liveSession.GetCurrentWord());

            if (liveSession.TeachMode == MODE.Teach)
            {
                WordAttributes wordAttributes = await WordAttributes.GetWordAttributes(liveSession.GetCurrentWord(), log);
                return TeachMode.TeachTheWord(" ", liveSession, wordAttributes);
            }
            else
            {
                return AlexaResponse.PresentFlashCard(currentWord, prompt, prompt);
            }            
        }
           

        private async Task<SkillResponse> HandleWordsToReadIntent(IntentRequest intent)
        {
            log.INFO("Function", "HandleWordsToReadIntent", "Current Schedule: " + liveSession.CurrentSchedule);
            

            await liveSession.GetDataFromLiveSession();

            string currentWord = liveSession.GetCurrentWord();

            log.INFO("Function", "HandleWordsToReadIntent", "Current Word: " + currentWord);

            string prompt = "";
            string rePrompt = "Say the word";

            bool wordWasSaid = ReaderSaidTheWord(intent);

            log.DEBUG("Function", "HandleWordsToReadIntent", "Reader said the word? " + wordWasSaid);

            if (wordWasSaid)
            {
                prompt = CommonPhrases.ShortAffirmation;

                bool sessionFinished = liveSession.Remove(currentWord);

                log.DEBUG("Function", "HandleWordsToReadIntent", "Session Finished? " + sessionFinished);

                if (sessionFinished)
                {
                    prompt = CommonPhrases.LongAffirmation + "You finished this session! Another reading session awaits you. Just say, Alexa, open Moycan Readers!";                    
                    await this.userProfile.RemoveCompletedScheduleFromUserProfile(liveSession.CurrentSchedule);
                    liveSession.CurrentState = STATE.Off;
                    return ResponseBuilder.Tell(prompt);
                }
                else
                {
                    currentWord = liveSession.GetCurrentWord();
                    liveSession.CurrentState = STATE.Assess;
                }

                await UpdateLiveSessionDatabase();
            }

            log.DEBUG("Function", "HandleWordsToReadIntent", "Teach Mode: " + liveSession.TeachMode.ToString());

            if (liveSession.TeachMode == MODE.Teach)
            {
                WordAttributes wordAttributes = await WordAttributes.GetWordAttributes(liveSession.GetCurrentWord(), log);
                return TeachMode.TeachTheWord(prompt, liveSession, wordAttributes);
            }
            else
            {
                return AlexaResponse.PresentFlashCard(currentWord, prompt, rePrompt);
            }
            
        }

        private async Task TransferDataFromUserProfileToLiveSession()
        {
            log.INFO("Function", "TransferDataFromUserProfileToLiveSession", "Transferring Data");

            int currentScheduleNumber = await userProfile.GetFirstScheduleNumber();

            log.DEBUG("Function", "TransferDataFromUserProfileToLiveSession", "Current Schedule: " + currentScheduleNumber);

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
            log.INFO("Function", "UpdateLiveSessionDatabase", "Updating Live Session");

            await liveSession.UpdateLiveSession();
        }

        private bool ReaderSaidTheWord(IntentRequest input)
        {
            if (input.Intent.Slots?.Any() ?? false)
            {
                foreach (ResolutionAuthority auth in input.Intent.Slots.Last().Value.Resolution.Authorities)
                {
                    if (auth.Status.Code == ResolutionStatusCode.SuccessfulMatch)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

    }
}