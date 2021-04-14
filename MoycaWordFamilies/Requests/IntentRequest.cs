using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using MoycaWordFamilies.Requests.Intents;
using Infrastructure.Alexa;
using MoycaWordFamilies.Requests.Intents;

namespace MoycaWordFamilies.Requests
{

    public class IntentRequest : IRequest
    {
        private SkillRequest skillRequest;

        public IntentRequest(SkillRequest skillRequest) 
        {
            this.skillRequest = skillRequest;
        }


        public SkillResponse HandleRequest()
        {
            string intentName = ((Alexa.NET.Request.Type.IntentRequest)skillRequest.Request).Intent.Name;            

            LOGGER.log.INFO("IntentRequest", "HandleRequest", intentName);
            
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

                case "ValidateUserResponseIntent":
                    return new ValidateUserResponse(this.skillRequest).HandleIntent();
                    
                default:                    
                    return ResponseBuilder.Tell("When you are ready to begin say, 'Alexa, open Moyca Readers'. Goodbye.");
            }

        }

    }
}