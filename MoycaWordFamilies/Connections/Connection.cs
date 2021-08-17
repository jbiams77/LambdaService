using System;
using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.InSkillPricing.Responses;
using Alexa.NET.InSkillPricing;
using MoycaWordFamilies.Connections.Purchase;
using MoycaWordFamilies.Requests.Intents;
using Infrastructure.Interfaces;
using Infrastructure.Alexa;
using Alexa.NET;
using MoycaWordFamilies.Requests;
using Alexa.NET.Request.Type;

namespace MoycaWordFamilies.Responses
{
    public class Connection : IRequest
    {
        SkillRequest skillRequest;
        public Connection(SkillRequest skillRequest)
        {
            this.skillRequest = skillRequest;
        }

        public async Task<SkillResponse> HandleRequest()
        {
            LOGGER.log.INFO("Connection", "HandleRequest");
            string paymentType = ((ConnectionResponseRequest<ConnectionResponsePayload>)skillRequest.Request).Name;
            string purchaseResult = ((ConnectionResponseRequest<ConnectionResponsePayload>)skillRequest.Request).Payload.PurchaseResult;

            switch (paymentType)
            {
                case PaymentType.Buy:
                    return await new Buy(skillRequest).Handle(purchaseResult);

                case PaymentType.Upsell:
                    return await new Upsell(skillRequest).Handle(purchaseResult);

                case PaymentType.Cancel:
                    MoycaResponse.Prompt = " You can still use the free flash cards. ";
                    return await new Launch(skillRequest).HandleRequest();
            }

            return ResponseBuilder.Tell("Goodbye Moycan!"); 
        }

    }

}
