using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Amazon.DynamoDBv2.Model;
using Infrastructure.Logger;
using Newtonsoft.Json;
using Alexa.NET.InSkillPricing.Responses;
using FlashCardService.Responses;
using FlashCardService.Requests;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: Amazon.Lambda.Core.LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace FlashCardService
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
        public SkillResponse response;
        
        // REQUIRED FOR IN-SKILL PURHCASES TO WORK
        public Function()
        {            
            ConnectionRequestHandler.AddToRequestConverter();
        }

        public async Task<SkillResponse> FunctionHandler(SkillRequest request, ILambdaContext context)
        {
            
            string requestType = request.Request.Type;
            LOGGER.log = MoycaLogger.GetLogger(context, LogLevel.TRACE);                       
            AlexaResponse.SetDisplaySupported(request.APLSupported());            
            LogSessionStart(request);                     
            

            switch (requestType)
            {
                case "LaunchRequest":
                    LOGGER.log.DEBUG("Function", "Launch Request");
                    response = await new LaunchRequest(request).HandleRequest();
                    break;

                case "IntentRequest":
                    LOGGER.log.DEBUG("Function", "Intent Request");
                    response = await new IntentRequest(request).HandleRequest();
                    break;

                case "SessionEndedRequest":
                    LOGGER.log.DEBUG("Function", "Session Ended Request");
                    response = await new SessionEnded().HandleRequest();
                    break;

                case "Connections.Response":
                    LOGGER.log.DEBUG("Function", "Connection Response ");
                    response = await new Connection(request).HandleRequest();
                    break;

                default:
                    LOGGER.log.DEBUG("Function", "Default Error Request");
                    response = AlexaResponse.Say("Error");
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
                return TeachMode.TeachTheWord(" ", 0, liveSession, wordAttributes);
            }
            else
            {
                return AlexaResponse.PresentFlashCard(currentWord, 0, prompt, prompt);
            }            
        }

        private async Task<SkillResponse> HandleHelpRequest()
        {
            await TransferDataFromUserProfileToLiveSession();
            await liveSession.GetDataFromLiveSession();

            log.INFO("Function", "HandleHelpRequest", "Current Schedule: " + liveSession.CurrentSchedule);
            
            return AlexaResponse.PresentFlashCard(liveSession.GetCurrentWord(), 0, CommonPhrases.Help, "You can say the word now");
        }
        

        private async Task<SkillResponse> HandleWordsToReadIntent(SkillRequest input)
        {
            log.INFO("Function", "HandleWordsToReadIntent", "Current Schedule: " + liveSession.CurrentSchedule);
            
            var request = (IntentRequest)input.Request;

            await liveSession.GetDataFromLiveSession();

            string currentWord = liveSession.GetCurrentWord();

            log.INFO("Function", "HandleWordsToReadIntent", "Current Word: " + currentWord);

            string prompt = "Say the word";
            string rePrompt = "Say the word";

            int totalFailedAttempts = 0;

            if (input.Session.Attributes != null &&
                input.Session.Attributes.ContainsKey("TotalFailedAttempts"))
            {
                totalFailedAttempts = Int32.Parse((string)input.Session.Attributes["TotalFailedAttempts"]);
            }

            bool wordWasSaid = ReaderSaidTheWord(request);

            log.DEBUG("Function", "HandleWordsToReadIntent", "Reader said the word? " + wordWasSaid);

            if (wordWasSaid)
            {
                prompt = CommonPhrases.ShortAffirmation;

                bool sessionFinished = liveSession.Remove(currentWord);

                log.DEBUG("Function", "HandleWordsToReadIntent", "Session Finished? " + sessionFinished);

                if (sessionFinished)
                {
                    var percentAccuracy = await GetPercentAccuracy(totalFailedAttempts);

                    if (percentAccuracy >= PERCENT_TO_MOVE_FORWARD)
                    {
                        prompt = CommonPhrases.LongAffirmation + "You're ready to move to the next lesson! Just say, Alexa, open Moyca Readers!";
                        await this.userProfile.IncrementUserSchedule(liveSession.CurrentSchedule);
                    }
                    else if (percentAccuracy <= PERCENT_TO_MOVE_BACKWARD && liveSession.TeachMode == MODE.Assess)
                    {
                        prompt = "Let's review this lesson again! Just say, Alexa, open Moyca Readers!";
                        await this.userProfile.DecrementUserSchedule(liveSession.CurrentSchedule);
                    }
                    else
                    {
                        prompt = CommonPhrases.LongAffirmation + "Let's practice that session again! Just say, Alexa, open Moyca Readers!";
                    }
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
            else
            {
                // Missed a word. Increment the attempts counter
                totalFailedAttempts++;
            }

            log.DEBUG("Function", "HandleWordsToReadIntent", "Teach Mode: " + liveSession.TeachMode.ToString());
            log.DEBUG("Function", "HandleWordsToReadIntent", "Attempts Made: " + totalFailedAttempts.ToString());

            if (liveSession.TeachMode == MODE.Teach)
            {
                WordAttributes wordAttributes = await WordAttributes.GetWordAttributes(liveSession.GetCurrentWord(), log);
                return TeachMode.TeachTheWord(prompt, totalFailedAttempts, liveSession, wordAttributes);
            }
            else
            {
                return AlexaResponse.PresentFlashCard(currentWord, totalFailedAttempts, prompt, rePrompt);
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

        private void LogSessionStart(SkillRequest request)
        {
            LOGGER.log.INFO("BEGIN", "-----------------------------------------------------------------------");
            string skillRequest = JsonConvert.SerializeObject(request, Formatting.Indented);
            LOGGER.log.DEBUG("INPUT RECEIVED: ", skillRequest);
            LOGGER.log.INFO("Function", "USERID: " + request.Session.User.UserId);
            LOGGER.log.DEBUG("Function", "REQUEST TYPE: " + request.Request.Type);
            LOGGER.log.DEBUG("Function", "DisplaySupported: " + request.APLSupported());
        }

    }

}