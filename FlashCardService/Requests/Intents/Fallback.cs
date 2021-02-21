using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using AWSInfrastructure.GlobalConstants;

namespace FlashCardService.Requests.Intents
{
    public class Fallback : Intent
    {
        public Fallback() { }

        public async Task<SkillResponse> HandleIntent()
        {
            Function.log.INFO("Fallback", "HandleIntent", "CURRENT STATE: " + this.sessionAttributes.Schedule);

            if (this.sessionAttributes.SessionState != STATE.Introduction)
            {
                WordsToRead wordsToRead = new WordsToRead();
                return await wordsToRead.HandleIntent();
            }

            this.sessionAttributes.SessionState = STATE.Off;

            return ResponseBuilder.Tell("When you are ready to begin say, 'Alexa, open Moyca Readers'. Goodbye.");
        }

    }
}
