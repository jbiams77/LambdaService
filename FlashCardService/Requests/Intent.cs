using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.InSkillPricing.Responses;
using AWSInfrastructure.DynamoDB;
using AWSInfrastructure.GlobalConstants;
using AWSInfrastructure.CognitoPool;
using AWSInfrastructure.Logger;
using System.Linq;

namespace FlashCardService.Requests
{
    public class Intent : Request
    {
        public UserProfileDB userProfile;
        public ScopeAndSequenceDB scopeAndSequence;
        public SkillResponse response;
        public string userId;
        private SessionAttributes sessionAttributes;
        private TeachMode teachMode;
        private ProductInventory purchase;
        private SkillRequest skillRequest;


        public Intent(SkillRequest request)
        {
            this.sessionAttributes = new SessionAttributes(Function.log);
            this.teachMode = new TeachMode(this.sessionAttributes);
            this.purchase = new ProductInventory(request);
            this.userProfile = new UserProfileDB(userId, Function.log);
            this.scopeAndSequence = new ScopeAndSequenceDB(Function.log);
        }

        public override async Task<SkillResponse> HandleRequest()
        {
            var request = (IntentRequest)skillRequest.Request;
            this.sessionAttributes.UpdateSessionAttributes(skillRequest.Session.Attributes);

            Function.log.INFO("Function", "HandleIntentRequest", request.Intent.Name);

            // Allow cancel, stop, and help intents to happen in any state
            switch (request.Intent.Name)
            {
                case "AMAZON.CancelIntent":
                    SetStateToOffAndExit();
                    return ResponseBuilder.Tell("Until next time my Moycan!");
                case "AMAZON.StopIntent":
                    SetStateToOffAndExit();
                    return ResponseBuilder.Tell("Goodbye.");
                case "AMAZON.HelpIntent":
                    return await HandleHelpRequest();
            }

            // Only accept yes or no intents when in introduction.
            // This prevents Alexa from going into other intents if something other than "Yes" or "No" was said during intro.
            if (this.sessionAttributes.SessionState == STATE.Introduction)
            {
                switch (request.Intent.Name)
                {
                    case "AMAZON.YesIntent":
                        return await HandleYesIntent();
                    case "AMAZON.NoIntent":
                    default:
                        SetStateToOffAndExit();
                        return ResponseBuilder.Tell("When you are ready to begin say, 'Alexa, open Moyca Readers'. Goodbye.");
                }
            }
            else if (this.sessionAttributes.SessionState == STATE.Assess ||
                     this.sessionAttributes.SessionState == STATE.FirstWord)
            {
                // Treat any word said as a flash card attempt (including yes or no)
                switch (request.Intent.Name)
                {
                    case "WordsToReadIntent":
                    case "AMAZON.FallbackIntent":
                    case "AMAZON.YesIntent":
                    case "AMAZON.NoIntent":
                        return await HandleWordsToReadIntent(skillRequest);
                }
            }

            return ResponseBuilder.Tell("I didn't understand that.");

        }

        private async Task PopulateSessionAttributes(int currentScheduleNumber)
        {
            Function.log.INFO("Function", "PopulateSessionAttributes", "Transferring Data");
            Function.log.DEBUG("Function", "PopulateSessionAttributes", "Current Schedule: " + currentScheduleNumber);

            await scopeAndSequence.GetSessionDataWithNumber(currentScheduleNumber);

            this.sessionAttributes.WordsToRead = scopeAndSequence.WordsToRead;
            this.sessionAttributes.LessonMode = MODE.Teach;
            this.sessionAttributes.LessonSkill = (SKILL)(int.Parse(scopeAndSequence.Skill));
            this.sessionAttributes.Lesson = scopeAndSequence.Lesson;
            this.sessionAttributes.Schedule = currentScheduleNumber;
            this.sessionAttributes.TotalWordsInSession = scopeAndSequence.WordsToRead.Count();
            this.sessionAttributes.FailedAttempts = 0;
        }

        private void SetStateToOffAndExit()
        {
            Function.log.INFO("Function", "SetStateToOffAndExit", "Schedule: " + this.sessionAttributes.Schedule);
            this.sessionAttributes.SessionState = STATE.Off;
        }

        private async Task<SkillResponse> HandleYesIntent()
        {
            Function.log.INFO("Function", "HandleYesIntent", "Current Schedule: " + this.sessionAttributes.Schedule);

            this.sessionAttributes.SessionState = STATE.FirstWord;

            string currentWord = this.sessionAttributes.CurrentWord;

            string prompt = "Say the word ";

            Function.log.DEBUG("Function", "HandleYesIntent", "Teach Mode: " + this.sessionAttributes.LessonMode.ToString());
            Function.log.INFO("Function", "HandleYesIntent", "Current Word: " + this.sessionAttributes.CurrentWord);


            if (this.sessionAttributes.LessonMode == MODE.Teach)
            {
                WordAttributes wordAttributes = await WordAttributes.GetWordAttributes(this.sessionAttributes.CurrentWord, Function.log);
                return this.teachMode.TeachTheWord(" ", wordAttributes);
            }
            else
            {
                return AlexaResponse.PresentFlashCard(currentWord, prompt, prompt);
            }
        }

        private async Task<SkillResponse> HandleWordsToReadIntent(SkillRequest input)
        {
            Function.log.INFO("Function", "HandleWordsToReadIntent", "Current Schedule: " + this.sessionAttributes.Schedule);

            this.sessionAttributes.SessionState = STATE.Assess;

            var request = (IntentRequest)input.Request;

            string currentWord = this.sessionAttributes.CurrentWord;

            Function.log.INFO("Function", "HandleWordsToReadIntent", "Current Word: " + currentWord);

            string prompt = "Say the word";
            string rePrompt = "Say the word";

            bool wordWasSaid = ReaderSaidTheWord(request);

            Function.log.DEBUG("Function", "HandleWordsToReadIntent", "Reader said the word? " + wordWasSaid);

            if (wordWasSaid)
            {
                prompt = CommonPhrases.ShortAffirmation;

                this.sessionAttributes.WordsToRead.Remove(currentWord);
                bool sessionFinished = !this.sessionAttributes.WordsToRead.Any();

                Function.log.DEBUG("Function", "HandleWordsToReadIntent", "Session Finished? " + sessionFinished);

                if (sessionFinished)
                {
                    var percentAccuracy = await GetPercentAccuracy(this.sessionAttributes.FailedAttempts, this.sessionAttributes.TotalWordsInSession);

                    //if (percentAccuracy >= PERCENT_TO_MOVE_FORWARD)
                    //{
                    //    prompt = CommonPhrases.LongAffirmation + "You're ready to move to the next lesson! Just say, Alexa, open Moyca Readers!";
                    //    await this.userProfile.IncrementUserSchedule(this.sessionAttributes.Schedule);
                    //}
                    //else if (percentAccuracy <= PERCENT_TO_MOVE_BACKWARD && this.sessionAttributes.LessonMode == MODE.Assess)
                    //{
                    //    prompt = "Let's review this lesson again! Just say, Alexa, open Moyca Readers!";
                    //    await this.userProfile.DecrementUserSchedule(this.sessionAttributes.Schedule);
                    //}
                    //else
                    //{
                    //    prompt = CommonPhrases.LongAffirmation + "Let's practice that session again! Just say, Alexa, open Moyca Readers!";
                    //}
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

            Function.log.DEBUG("Function", "HandleWordsToReadIntent", "Teach Mode: " + this.sessionAttributes.LessonMode.ToString());
            Function.log.DEBUG("Function", "HandleWordsToReadIntent", "Attempts Made: " + this.sessionAttributes.FailedAttempts.ToString());

            if (this.sessionAttributes.LessonMode == MODE.Teach)
            {
                WordAttributes wordAttributes = await WordAttributes.GetWordAttributes(this.sessionAttributes.CurrentWord, Function.log);
                return this.teachMode.TeachTheWord(prompt, wordAttributes);
            }
            else
            {
                return AlexaResponse.PresentFlashCard(currentWord, prompt, rePrompt);
            }
        }

        private async Task<int> GetPercentAccuracy(int totalFailedAttempts, int totalWordsInSession)
        {
            int currentScheduleNumber = userProfile.GetUserSchedule();

            var percentAccuracy = (int)((1.0 - ((double)totalFailedAttempts / (double)totalWordsInSession)) * 100.0);

            Function.log.DEBUG("Function", "GetPercentAccuracy", "totalFailedAttempts " + totalFailedAttempts);
            Function.log.DEBUG("Function", "GetPercentAccuracy", "totalWordsInSession " + totalWordsInSession);
            Function.log.DEBUG("Function", "GetPercentAccuracy", "percentAccuracy " + percentAccuracy);

            return percentAccuracy;
        }

        private async Task<SkillResponse> HandleHelpRequest()
        {
            Function.log.INFO("Function", "HandleHelpRequest", "Current Schedule: " + this.sessionAttributes.Schedule);

            return AlexaResponse.PresentFlashCard(this.sessionAttributes.CurrentWord, CommonPhrases.Help, "You can say the word now");
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