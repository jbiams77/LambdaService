using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using AWSInfrastructure.GlobalConstants;

namespace FlashCardService.Requests.Intents
{
    public class Cancel : Intent
    {
        public Cancel(SkillRequest request) : base(request) { }

        public async Task<SkillResponse> HandleIntent()
        {
            Function.log.INFO("Function", "HandleYesIntent", "Current Schedule: " + this.sessionAttributes.Schedule);

            if (this.sessionAttributes.SessionState != STATE.Introduction)
            {
                WordsToRead wordsToRead = new WordsToRead(base.skillRequest);
                return await wordsToRead.HandleIntent();
            }

            this.sessionAttributes.SessionState = STATE.Off;

            return ResponseBuilder.Tell("When you are ready to begin say, 'Alexa, open Moyca Readers'. Goodbye.");
        }

    }
}
