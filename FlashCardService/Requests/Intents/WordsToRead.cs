using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using AWSInfrastructure.GlobalConstants;
using System.Linq;

namespace FlashCardService.Requests.Intents
{
    public class WordsToRead : Intent
    {
        public WordsToRead(SkillRequest request) : base(request) { }

        public async Task<SkillResponse> HandleIntent()
        {
            Function.log.INFO("Function", "HandleWordsToReadIntent", "Current Schedule: " + this.sessionAttributes.Schedule);

            this.sessionAttributes.SessionState = STATE.Assess;

            var request = (Alexa.NET.Request.Type.IntentRequest)skillRequest.Request;

            string currentWord = this.sessionAttributes.CurrentWord;

            Function.log.INFO("Function", "HandleWordsToReadIntent", "Current Word: " + currentWord);

            string prompt = "Say the word";
            string rePrompt = "Say the word";

            bool wordWasSaid = ReaderSaidTheWord(request);

            Function.log.DEBUG("Function", "HandleWordsToReadIntent", "Reader said the word? " + wordWasSaid);

            if (wordWasSaid)
            {
                prompt = CommonPhrases.ShortAffirmation;

                this.sessionAttributes.RemoveCurrentWord();
                bool sessionFinished = !this.sessionAttributes.WordsToRead.Any();

                Function.log.DEBUG("Function", "HandleWordsToReadIntent", "Session Finished? " + sessionFinished);

                if (sessionFinished)
                {
                    this.sessionAttributes.SessionState = STATE.Off;
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

        private bool ReaderSaidTheWord(Alexa.NET.Request.Type.IntentRequest input)
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