using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using MoycaAddition.Requests.Intents;
using FlashCardService.Interfaces;
using MoycaAddition;
using Infrastructure.Alexa;

namespace MoycaAddition.Requests
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

                case "MathIntent":
                    return new Math(this.skillRequest).HandleIntent();
                    
                default:                    
                    return ResponseBuilder.Tell("When you are ready to begin say, 'Alexa, open Moyca Readers'. Goodbye.");
            }

        }

    }
}