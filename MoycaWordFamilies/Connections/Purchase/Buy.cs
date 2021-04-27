using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.InSkillPricing.Responses;
using Alexa.NET.Request;
using Alexa.NET.Response;
using MoycaWordFamilies.Connections.Purchase.Response;
using Infrastructure.Interfaces;

namespace MoycaWordFamilies.Connections.Purchase
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
                    return await new Accepted(skillRequest).Handle();
                case PurchaseResult.Declined:
                    LOGGER.log.INFO("Buy", "Handle", "Purchase Result Declined.");
                    return await new Declined(skillRequest).Handle();
                case PurchaseResult.AlreadyPurchased:
                    LOGGER.log.INFO("Buy", "Handle", "Purchase Result Already Purchased.");
                    return await new Error(skillRequest).Handle();
                case PurchaseResult.Error:
                    LOGGER.log.INFO("Buy", "Handle", "Purchase Result Error.");
                    return await new Error(skillRequest).Handle();
                // add default to handle purchase pending
            }
            return await new Error(skillRequest).Handle();
        }
    }
}
