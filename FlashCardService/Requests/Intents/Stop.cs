using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using AWSInfrastructure.GlobalConstants;

namespace FlashCardService.Requests.Intents
{
    public class Stop : Intent
    {
        public Stop(SkillRequest request) : base(request) { }

        public async Task<SkillResponse> HandleIntent()
        {
            Function.log.INFO("Stop", "HandleIntent", "State before stopped: " + this.sessionAttributes.SessionState);

            this.sessionAttributes.SessionState = STATE.Off;

            return ResponseBuilder.Tell("If you would like to play again, 'Alexa, open Moyca Readers'. Goodbye.");
        }

    }
}
