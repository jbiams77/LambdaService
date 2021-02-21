using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.InSkillPricing.Responses;
using AWSInfrastructure.DynamoDB;
using AWSInfrastructure.GlobalConstants;
using AWSInfrastructure.CognitoPool;
using AWSInfrastructure.Logger;

namespace FlashCardService.Requests
{
    public class Launch : Request
    {
        private UserProfileDB userProfile;
        private ScopeAndSequenceDB scopeAndSequence;
        private SkillResponse response;
        private SessionAttributes sessionAttributes;
        private TeachMode teachMode;
        private ProductInventory productInventory;
        private SkillRequest skillRequest;


        public Launch(SkillRequest request)
        {
            this.skillRequest = request;
            this.userProfile = new UserProfileDB(request.Session.User.UserId, Function.log);
            this.sessionAttributes = new SessionAttributes(Function.log);                     
            this.productInventory = new ProductInventory(request);            
            this.scopeAndSequence = new ScopeAndSequenceDB(Function.log);
            AlexaResponse.SetSessionAttributeHandler(sessionAttributes);
        }

        public override async Task<SkillResponse> HandleRequest()
        {
            await this.userProfile.GetUserProfileData();
            await scopeAndSequence.GetSessionDataWithNumber(userProfile.GetUserSchedule());

            if (userProfile.RequiresPurchase())
            {
                await productInventory.UpdateProductInformation();
                string productName = scopeAndSequence.InSkillPurchase;

                if (productInventory.IsUnpaid(productName))
                {
                    string properName = productName.Replace('_', ' ');
                    return AlexaResponse.PurchaseContentUpsell(productInventory.GetProductId(productName), 
                        CommonPhrases.Upsell(properName), properName);
                }
            }
            
            this.sessionAttributes.UpdateSessionAttributes(scopeAndSequence, userProfile.GetUserSchedule(), userProfile.GetMode());
            this.sessionAttributes.SessionState = STATE.Introduction;

            Function.log.INFO("Launch", "HandleRequest", "Current Schedule: " + userProfile.GetUserSchedule());
            Function.log.DEBUG("Function", "HandleLaunchRequest", "Teach Mode: " + userProfile.GetMode());

            if (this.sessionAttributes.LessonMode == MODE.Teach)
            {
                this.teachMode = new TeachMode(this.sessionAttributes);
                WordAttributes wordAttributes = await WordAttributes.GetWordAttributes(this.sessionAttributes.CurrentWord, Function.log);
                return this.teachMode.Introduction(wordAttributes);
            }
            else
            {
                return AlexaResponse.Introduction("Greetings my fellow Moycan! Lets learn to read. Are you ready to begin ?", "Say yes or no to continue.");
            }

        }


    }
}
