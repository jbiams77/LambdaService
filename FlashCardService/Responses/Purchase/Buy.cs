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
        public SkillResponse Handle(string purchaseResult, string productName)
        {
            switch (purchaseResult)
            {
                case PurchaseResult.Accepted:
                    break;
                case PurchaseResult.Declined:
                    break;
                case PurchaseResult.AlreadyPurchased:
                    break;
                case PurchaseResult.Error:
                    break;
            }
            return AlexaResponse.Say(" You want to buy Something.");
        }
    }
}
