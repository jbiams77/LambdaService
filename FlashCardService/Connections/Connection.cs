using System;
using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.InSkillPricing.Responses;
using Alexa.NET.InSkillPricing;
using Alexa.NET.Request.Type;
using FlashCardService.Connections.Purchase;
using FlashCardService.Requests.Intents;
using FlashCardService.Interfaces;

namespace FlashCardService.Responses
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
                    return new Cancel(this.skillRequest).HandleIntent();
            }

            return AlexaResponse.Say("Goodbye Moycan!"); 
        }

    }

}
