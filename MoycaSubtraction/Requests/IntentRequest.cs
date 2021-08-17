using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using MoycaSubtraction.Requests.Intents;
using MoycaSubtraction;
using Infrastructure.Alexa;
using Infrastructure.Interfaces;

namespace MoycaSubtraction.Requests
{

    public class IntentRequest : IRequest
    {
        private SkillRequest skillRequest;

        public IntentRequest(SkillRequest skillRequest) 
        {
            this.skillRequest = skillRequest;
        }


        public Task<SkillResponse> HandleRequest()
        {
            string intentName = ((Alexa.NET.Request.Type.IntentRequest)skillRequest.Request).Intent.Name;

            LOGGER.log.INFO("IntentRequest", "HandleRequest", intentName);

            return Task.FromResult(ResponseToIntent(intentName));

        }

        private SkillResponse ResponseToIntent(string intentName)
        {
            switch (intentName)
            {
                case "AMAZON.CancelIntent":
                    return new Cancel(this.skillRequest).HandleIntent();

                case "AMAZON.FallbackIntent":
                    return new Fallback(this.skillRequest).HandleIntent();

                case "AMAZON.HelpIntent":
                    return new Help(this.skillRequest).HandleIntent();

                case "AMAZON.StopIntent":
                    return new Stop(this.skillRequest).HandleIntent();

                case "MathIntent":
                    return new Math(this.skillRequest).HandleIntent();

                default:
                    return ResponseBuilder.Tell("When you are ready to begin say, 'Alexa, open Moyca Readers'. Goodbye.");
            }
        }
    }
}