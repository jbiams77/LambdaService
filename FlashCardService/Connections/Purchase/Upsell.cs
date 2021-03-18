using Alexa.NET.InSkillPricing.Responses;
using Alexa.NET.Response;
using FlashCardService.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using FlashCardService.Connections.Purchase.Response;
using Alexa.NET.Request;
using System.Threading.Tasks;

namespace FlashCardService.Connections.Purchase
{
    public class Upsell : IConnection
    {
        public SkillRequest skillRequest;

        public Upsell(SkillRequest skillRequest)
        {
            this.skillRequest = skillRequest;
        }

        public async Task<SkillResponse> Handle(string purchaseResult)
        {
            LOGGER.log.INFO("Upsell", "Handle");

            switch (purchaseResult)
            {
                case PurchaseResult.Accepted:
                    LOGGER.log.INFO("Upsell", "Handle", "Purchase Result Accepted.");
                    return new Accepted().Handle();
                    
                case PurchaseResult.Declined:
                    LOGGER.log.INFO("Upsell", "Handle", "Purchase Result Declined.");
                    
                    return await new Declined(skillRequest).Handle();

                case PurchaseResult.AlreadyPurchased:
                    LOGGER.log.INFO("Upsell", "Handle", "Purchase Result Already Purchased.");
                    // purchase is detected on launch, so should not get here, respond appropriately
                    break;

                case PurchaseResult.Error:
                    LOGGER.log.INFO("Upsell", "Handle", "Purchase Result Error.");
                    // purchase is detected on launch, so should not get here, respond appropriately
                    break;
                
            }
            return AlexaResponse.Say(" You want to buy Something.");
        }
    }
}
