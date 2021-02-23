using System;
using System.Collections.Generic;
using System.Text;
using Alexa.NET.InSkillPricing.Responses;
using Alexa.NET.Response;
using FlashCardService.Interfaces;

namespace FlashCardService.Responses.Purchase
{
    public class Buy : IConnection
    {
        public SkillResponse Handle(string purchaseResult)
        {
            LOGGER.log.INFO("Buy", "Handle");

            switch (purchaseResult)
            {
                case PurchaseResult.Accepted:
                    LOGGER.log.INFO("Buy", "Handle", "Purchase Result Accepted.");
                    // if purchased, begin the user on newly purchased flash card session
                    break;
                case PurchaseResult.Declined:
                    LOGGER.log.INFO("Buy", "Handle", "Purchase Result Declined.");
                    // if declined, remind user they can purchase premium content when ready
                    // and decrement session number by one
                    break;
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
