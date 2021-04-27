using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using MoycaWordFamilies.Requests.Intents;
using Infrastructure.Interfaces;

namespace MoycaWordFamilies.Requests
{

    public class Intent : IRequest
    {
        private SkillRequest skillRequest;

        public Intent(SkillRequest skillRequest) 
        {
            this.skillRequest = skillRequest;
        }

        public async Task<SkillResponse> HandleRequest()
        {
            string intentName = ((Alexa.NET.Request.Type.IntentRequest)skillRequest.Request).Intent.Name;            

            LOGGER.log.INFO("IntentRequest", "HandleRequest", intentName);
            
            switch (intentName)
            {   
                case "AMAZON.CancelIntent":
                    return new Cancel(this.skillRequest).HandleIntent();

                case "AMAZON.FallbackIntent":
                    return await new Fallback(this.skillRequest).HandleIntent();

                case "AMAZON.HelpIntent":
                    return new Help(this.skillRequest).HandleIntent();

                case "AMAZON.StopIntent":
                    return new Stop(this.skillRequest).HandleIntent();

                case "ValidateUserResponseIntent":
                    return await new ValidateUserResponse(this.skillRequest).HandleIntent();

                case "MakePurchaseIntent":
                    return await new MakePurchase(this.skillRequest).HandleIntent();

                case "RefundPurchaseIntent":
                    return new RefundPurchase(this.skillRequest).HandleIntent();


                default:                    
                    return ResponseBuilder.Tell("When you are ready to begin say, 'Alexa, open Moyca Readers'. Goodbye.");
            }

        }

    }
}