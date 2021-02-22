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

        public SkillResponse HandleIntent()
        {
            Function.log.INFO("Cancel", "HandleIntent", "State before stopped: " + this.sessionAttributes.SessionState);

            this.sessionAttributes.SessionState = STATE.Off;

            return ResponseBuilder.Tell("If you would like to play again, 'Alexa, open Moyca Readers'. Goodbye.");
        }

    }
}
