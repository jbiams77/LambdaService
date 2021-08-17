using Alexa.NET.InSkillPricing.Responses;
using Alexa.NET.Response;
using Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using MoycaWordFamilies.Connections.Purchase.Response;
using Alexa.NET.Request;
using System.Threading.Tasks;
using Alexa.NET;
using Infrastructure.Alexa;
using MoycaWordFamilies.Requests;

namespace MoycaWordFamilies.Connections.Purchase
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
                    return await new Accepted(skillRequest).Handle();
                    
                case PurchaseResult.Declined:
                    LOGGER.log.INFO("Upsell", "Handle", "Purchase Result Declined.");
                    
                    return await new Declined(skillRequest).Handle();

                case PurchaseResult.AlreadyPurchased:
                    LOGGER.log.INFO("Upsell", "Handle", "Purchase Result Already Purchased."); 
                    return await new Error(skillRequest).Handle();

                case "PENDING_PURCHASE":
                    LOGGER.log.INFO("Upsell", "Handle", "Purchase Result Pending Purchase.");
                    MoycaResponse.Prompt = " While we wait to confirm the purchase, we can continue with the free content. ";
                    return await new Launch(skillRequest).HandleRequest();

                case PurchaseResult.Error:
                    LOGGER.log.INFO("Upsell", "Handle", "Purchase Result Error.");
                    return await new Error(skillRequest).Handle();                    
                
            }
            LOGGER.log.INFO("Upsell", "Handle", "None selected.");
            return await new Error(skillRequest).Handle();
        }
    }
}
