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
        public static bool displaySupported;
        public UserProfileDB userProfile;
        public ScopeAndSequenceDB scopeAndSequence;
        public SkillResponse response;
        public string userId;        
        private CognitoUserPool cognitoUserPool;
        private SessionAttributes sessionAttributes;
        private TeachMode teachMode;

        // Most sessions have 6 - 7 words. Reader must miss one or less to advance sessions.
        private static int PERCENT_TO_MOVE_FORWARD = 83;

        // If the reader misses half of the words or more in assess mode, they will move back to teach mode
        private static int PERCENT_TO_MOVE_BACKWARD = 50;

            
        public async Task<SkillResponse> FunctionHandler(SkillRequest input, ILambdaContext context)
        {

            Type T = input.GetRequestType();
            log = new MoycaLogger(context, LogLevel.TRACE);

            this.sessionAttributes = new SessionAttributes(log);
            this.teachMode = new TeachMode(this.sessionAttributes);

            AlexaResponse.SetLogger(log);
            AlexaResponse.SetSessionAttributeHandler(sessionAttributes);
            log.INFO("BEGIN", "-----------------------------------------------------------------------");
            log.INFO("skill request", JsonConvert.SerializeObject(input)); 
            this.cognitoUserPool = new CognitoUserPool(log);

            // new user requires account linking
            if (input.Session.User.AccessToken == null)
            {
                return HandleNoExistingAccount();
            }

            this.userId = await cognitoUserPool.GetUsername(input.Session.User.AccessToken);
            this.userProfile = new UserProfileDB(userId, log);
            this.scopeAndSequence = new ScopeAndSequenceDB(log);

            Function.displaySupported = input.APLSupported();            
            log.INFO("Function", "USERID: " + this.userId);
            log.INFO("Function", "LaunchRequest: " + T.Name);
            log.INFO("Function", "DisplaySupported: " + input.APLSupported());
            AlexaResponse.SetDisplaySupported(input.APLSupported());

            switch (T.Name)
            {
                case "LaunchRequest":                    
                    response = await HandleLaunchRequest();
                    break;

                case "IntentRequest":
                    response = await HandleIntentRequest(input);
                    break;

                case "SessionEndedRequest":
                    await SetStateToOffAndExit();
                    response = AlexaResponse.Say("Goodbye Moycan!");
                    break;

                default:
                    response = AlexaResponse.Say("Error");
                    break;
            }

            return response;
        }

        private async Task<SkillResponse> HandleIntentRequest(SkillRequest input)
        {
            SkillResponse intentResponse;
            var request = (IntentRequest)input.Request;
            this.sessionAttributes.UpdateSessionAttributes(input.Session.Attributes);

            log.INFO("Function", "HandleIntentRequest", request.Intent.Name);

            switch (request.Intent.Name)
            {
                case "AMAZON.YesIntent":
                    intentResponse = await HandleYesIntent();
                    break;
                case "AMAZON.NoIntent":
                    await SetStateToOffAndExit();
                    intentResponse = ResponseBuilder.Tell("When you are ready to begin say, 'Alexa, open Moyca Readers'. Goodbye.");
                    break;
                case "AMAZON.CancelIntent":
                    await SetStateToOffAndExit();
                    intentResponse = ResponseBuilder.Tell("Until next time my Moycan!");
                    break;
                case "AMAZON.FallbackIntent":
                    intentResponse = await HandleWordsToReadIntent(input);
                    break;
                case "AMAZON.StopIntent":
                    await SetStateToOffAndExit();
                    intentResponse = ResponseBuilder.Tell("Goodbye.");
                    break;
                case "AMAZON.HelpIntent":                    
                    intentResponse = await HandleHelpRequest();
                    break;
                case "WordsToReadIntent":
                    intentResponse = await HandleWordsToReadIntent(input);
                    break;
                default:
                    intentResponse = ResponseBuilder.Tell("I didn't understand that.");
                    break;
            }

            return intentResponse;
        }
        private async Task SetStateToOffAndExit()
        {
            log.INFO("Function", "SetStateToOffAndExit", "Schedule: " + this.sessionAttributes.Schedule);
        }

        private async Task<SkillResponse> HandleLaunchRequest()
        {
            await PopulateSessionAttributes();
            this.sessionAttributes.SessionState = STATE.Introduction;

            log.INFO("Function", "HandleLaunchRequest", "Current Schedule: " + this.sessionAttributes.Schedule);


            log.DEBUG("Function", "HandleLaunchRequest", "Teach Mode: " + this.sessionAttributes.LessonMode.ToString());

            if (this.sessionAttributes.LessonMode == MODE.Teach)
            {           
                WordAttributes wordAttributes = await WordAttributes.GetWordAttributes(this.sessionAttributes.CurrentWord, log);
                return this.teachMode.Introduction(wordAttributes);
            }
            else
            {                
                return AlexaResponse.Introduction("Greetings my fellow Moycan! Lets learn to read. Are you ready to begin ?", "Say yes or no to continue.");
            }
            
        }

        private async Task<SkillResponse> HandleYesIntent()
        {
            log.INFO("Function", "HandleYesIntent", "Current Schedule: " + this.sessionAttributes.Schedule);

            this.sessionAttributes.SessionState = STATE.FirstWord;

            string currentWord = this.sessionAttributes.CurrentWord;

            string prompt = "Say the word ";

            log.DEBUG("Function", "HandleYesIntent", "Teach Mode: " + this.sessionAttributes.LessonMode.ToString());
            log.INFO("Function", "HandleYesIntent", "Current Word: " + this.sessionAttributes.CurrentWord);

            
            if (this.sessionAttributes.LessonMode == MODE.Teach)
            {
                WordAttributes wordAttributes = await WordAttributes.GetWordAttributes(this.sessionAttributes.CurrentWord, log);
                return this.teachMode.TeachTheWord(" ", wordAttributes);
            }
            else
            {
                return AlexaResponse.PresentFlashCard(currentWord, prompt, prompt);
            }            
        }

        private async Task<SkillResponse> HandleHelpRequest()
        {
            log.INFO("Function", "HandleHelpRequest", "Current Schedule: " + this.sessionAttributes.Schedule);
            this.sessionAttributes.SessionState = STATE.Help;

            return AlexaResponse.PresentFlashCard(this.sessionAttributes.CurrentWord, CommonPhrases.Help, "You can say the word now");
        }
        

        private async Task<SkillResponse> HandleWordsToReadIntent(SkillRequest input)
        {
            log.INFO("Function", "HandleWordsToReadIntent", "Current Schedule: " + this.sessionAttributes.Schedule);

            this.sessionAttributes.SessionState = STATE.Assess;

            var request = (IntentRequest)input.Request;

            string currentWord = this.sessionAttributes.CurrentWord;

            log.INFO("Function", "HandleWordsToReadIntent", "Current Word: " + currentWord);

            string prompt = "Say the word";
            string rePrompt = "Say the word";

            bool wordWasSaid = ReaderSaidTheWord(request);

            log.DEBUG("Function", "HandleWordsToReadIntent", "Reader said the word? " + wordWasSaid);

            if (wordWasSaid)
            {
                prompt = CommonPhrases.ShortAffirmation;

                this.sessionAttributes.WordsToRead.Remove(currentWord);
                bool sessionFinished = !this.sessionAttributes.WordsToRead.Any();

                log.DEBUG("Function", "HandleWordsToReadIntent", "Session Finished? " + sessionFinished);

                if (sessionFinished)
                {
                    var percentAccuracy = await GetPercentAccuracy(this.sessionAttributes.FailedAttempts, this.sessionAttributes.TotalWordsInSession);

                    if (percentAccuracy >= PERCENT_TO_MOVE_FORWARD)
                    {
                        prompt = CommonPhrases.LongAffirmation + "You're ready to move to the next lesson! Just say, Alexa, open Moyca Readers!";
                        await this.userProfile.IncrementUserSchedule(this.sessionAttributes.Schedule);
                    }
                    else if (percentAccuracy <= PERCENT_TO_MOVE_BACKWARD && this.sessionAttributes.LessonMode == MODE.Assess)
                    {
                        prompt = "Let's review this lesson again! Just say, Alexa, open Moyca Readers!";
                        await this.userProfile.DecrementUserSchedule(this.sessionAttributes.Schedule);
                    }
                    else
                    {
                        prompt = CommonPhrases.LongAffirmation + "Let's practice that session again! Just say, Alexa, open Moyca Readers!";
                    }
                    return ResponseBuilder.Tell(prompt);
                }
                else
                {
                    currentWord = this.sessionAttributes.CurrentWord;
                    this.sessionAttributes.SessionState = STATE.Assess;
                }
            }
            else
            {
                // Missed a word. Increment the attempts counter
                this.sessionAttributes.FailedAttempts = this.sessionAttributes.FailedAttempts + 1;
            }

            log.DEBUG("Function", "HandleWordsToReadIntent", "Teach Mode: " + this.sessionAttributes.LessonMode.ToString());
            log.DEBUG("Function", "HandleWordsToReadIntent", "Attempts Made: " + this.sessionAttributes.FailedAttempts.ToString());

            if (this.sessionAttributes.LessonMode == MODE.Teach)
            {
                WordAttributes wordAttributes = await WordAttributes.GetWordAttributes(this.sessionAttributes.CurrentWord, log);
                return this.teachMode.TeachTheWord(prompt, wordAttributes);
            }
            else
            {
                return AlexaResponse.PresentFlashCard(currentWord, prompt, rePrompt);
            }
        }

        private SkillResponse HandleNoExistingAccount()
        {
            log.INFO("Function", "HandleNoExistingAccount");

            String prompt = "You must have an account to continue. Please use the Alexa app to link your Amazon " +
                "account with Moyca Readers. This can be done by going to the skills section, clicking your skills, selecting " +
                " Moyca readers and Link Account under settings.";

            SkillResponse response = new SkillResponse { Version = "1.0" };

            ResponseBody body = new ResponseBody
            {
                ShouldEndSession = true,
                OutputSpeech = new PlainTextOutputSpeech { Text = prompt }
            };

            body.Card = new LinkAccountCard();

            response.Response = body;
            log.INFO("Function", "HandleNoExistingAccount", JsonConvert.SerializeObject(response) );

            return response;
        }


        private async Task PopulateSessionAttributes()
        {
            log.INFO("Function", "PopulateSessionAttributes", "Transferring Data");

            int currentScheduleNumber = await userProfile.GetFirstScheduleNumber();

            log.DEBUG("Function", "PopulateSessionAttributes", "Current Schedule: " + currentScheduleNumber);

            await scopeAndSequence.GetSessionDataWithNumber(currentScheduleNumber);
            this.sessionAttributes.WordsToRead = scopeAndSequence.WordsToRead;
            // TODO: incorporate teachmode with grade book lambda
            this.sessionAttributes.LessonMode = MODE.Teach;
            this.sessionAttributes.LessonSkill = (SKILL)(int.Parse(scopeAndSequence.Skill));
            this.sessionAttributes.Lesson = scopeAndSequence.Lesson;
            this.sessionAttributes.Schedule = currentScheduleNumber;
            this.sessionAttributes.TotalWordsInSession = scopeAndSequence.WordsToRead.Count();
            this.sessionAttributes.FailedAttempts = 0;
        }

        private async Task<int> GetPercentAccuracy(int totalFailedAttempts, int totalWordsInSession)
        {
            int currentScheduleNumber = await userProfile.GetFirstScheduleNumber();

            var percentAccuracy = (int)((1.0 - ((double)totalFailedAttempts / (double)totalWordsInSession)) * 100.0);

            log.DEBUG("Function", "GetPercentAccuracy", "totalFailedAttempts " + totalFailedAttempts);
            log.DEBUG("Function", "GetPercentAccuracy", "totalWordsInSession " + totalWordsInSession);
            log.DEBUG("Function", "GetPercentAccuracy", "percentAccuracy " + percentAccuracy);

            return percentAccuracy;
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