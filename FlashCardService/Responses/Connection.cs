using System;
using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Request;
using AWSInfrastructure.DynamoDB;
using AWSInfrastructure.GlobalConstants;
using Alexa.NET.InSkillPricing.Responses;
using Alexa.NET.InSkillPricing;
using Alexa.NET;
using Alexa.NET.Request.Type;
using FlashCardService.Responses.Purchase;
using FlashCardService.Requests.Intents;
using AWSInfrastructure.DynamoDB;

namespace FlashCardService.Responses
{
    public class Connection 
    {
        private SkillRequest skillRequest;
        private SessionAttributes sessionAttributes;
        private UserProfileDB userProfile;
        private ProductInventory productInventory;
        private ScopeAndSequenceDB scopeAndSequence;

        public Connection(SkillRequest skillRequest)
        {
            this.skillRequest = skillRequest;
            this.userProfile = new UserProfileDB(skillRequest.Session.User.UserId, Function.log);
            this.sessionAttributes = new SessionAttributes(Function.log);            
            this.productInventory = new ProductInventory(skillRequest);
            this.scopeAndSequence = new ScopeAndSequenceDB(Function.log);
            AlexaResponse.SetSessionAttributeHandler(sessionAttributes);
        }

        public async Task<SkillResponse> HandleRequest()
        {
            await this.userProfile.GetUserProfileData();
            await scopeAndSequence.GetSessionDataWithNumber(userProfile.GetUserSchedule());
            string paymentType = ((ConnectionResponseRequest<ConnectionResponsePayload>)skillRequest.Request).Name;
            string purchaseResult = ((ConnectionResponseRequest<ConnectionResponsePayload>)skillRequest.Request).Payload.PurchaseResult;
            await productInventory.GetAvailableProducts();
            string productName = scopeAndSequence.InSkillPurchase.Replace('_', ' ');

            switch (paymentType)
            {
                case PaymentType.Buy:
                    return new Buy().Handle(purchaseResult, productName);

                case PaymentType.Upsell:
                    return new Upsell().Handle(purchaseResult, productName);

                case PaymentType.Cancel:
                    return new Cancel(this.skillRequest).HandleIntent();
            }

            return AlexaResponse.Say("Goodbye Moycan!"); ;
        }

    }

}
