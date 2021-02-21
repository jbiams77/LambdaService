using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using AWSInfrastructure.GlobalConstants;

namespace FlashCardService.Requests.Intents
{
    public class Help : Intent
    {
        public Help() { }

        public async Task<SkillResponse> HandleIntent()
        {
            Function.log.DEBUG("Help", "HandleIntent", "CURRENT STATE: " + this.sessionAttributes.Schedule);

            if (this.sessionAttributes.SessionState != STATE.Introduction)
            {
                WordsToRead wordsToRead = new WordsToRead();
                return await wordsToRead.HandleIntent();
            }

            this.sessionAttributes.SessionState = STATE.Help;

            return ResponseBuilder.Tell("The Help feature has not yet been configured, goodbye.");
        }

    }
}
