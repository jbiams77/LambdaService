using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Alexa.NET.InSkillPricing.Responses;
using Alexa.NET.Request;
using Alexa.NET.Response;
using FlashCardService.Connections.Purchase.Response;
using FlashCardService.Interfaces;

namespace FlashCardService.Connections.Purchase
{
    public class Buy : IConnection
    {
        public SkillRequest skillRequest;
        public Buy(SkillRequest skillRequest)
        {
            this.skillRequest = skillRequest;
        }

        public async Task<SkillResponse> Handle(string purchaseResult)
        {
            LOGGER.log.INFO("Buy", "Handle");

            switch (purchaseResult)
            {
                case PurchaseResult.Accepted:
                    LOGGER.log.INFO("Buy", "Handle", "Purchase Result Accepted.");
                    return new Accepted().Handle();
                case PurchaseResult.Declined:
                    LOGGER.log.INFO("Buy", "Handle", "Purchase Result Declined.");
                    return await new Declined(skillRequest).Handle();
                case PurchaseResult.AlreadyPurchased:
                    LOGGER.log.INFO("Buy", "Handle", "Purchase Result Already Purchased.");
                    // purchase is detected on launch, so should not get here, respond appropriately
                    break;
                case PurchaseResult.Error:
                    LOGGER.log.INFO("Buy", "Handle", "Purchase Result Error.");
                    // unsure of how this case is reached
                    break;
            }
            return AlexaResponse.Say(" You want to buy Something.");
        }
    }
}
