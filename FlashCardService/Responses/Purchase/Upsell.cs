using Alexa.NET.InSkillPricing.Responses;
using Alexa.NET.Response;
using FlashCardService.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlashCardService.Responses.Purchase
{
    public class Upsell : IConnection
    {
        public SkillResponse Handle(string purchaseResult, string productName)
        {
            switch (purchaseResult)
            {
                case PurchaseResult.Accepted:
                    return AlexaResponse.Say(CommonPhrases.UpSellAccepted(productName));
                    
                case PurchaseResult.Declined:
                    return AlexaResponse.Say(CommonPhrases.UpSellDeclined(productName));
                    
                case PurchaseResult.AlreadyPurchased:
                    break;
                case PurchaseResult.Error:
                    break;
            }
            return AlexaResponse.Say(" You want to buy Something.");
        }
    }
}
