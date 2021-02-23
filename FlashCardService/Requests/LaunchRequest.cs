using System;
using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Request;
using AWSInfrastructure.DynamoDB;
using AWSInfrastructure.GlobalConstants;
using FlashCardService.Interfaces;

namespace FlashCardService.Requests
{
    public class LaunchRequest : IRequest
    {
        private UserProfileDB userProfile;
        private ScopeAndSequenceDB scopeAndSequence;
        private SessionAttributes sessionAttributes;
        private TeachMode teachMode;
        private ProductInventory productInventory;


        public LaunchRequest(SkillRequest skillRequest)
        {
            this.userProfile = new UserProfileDB(skillRequest.Session.User.UserId, LOGGER.log);
            this.sessionAttributes = new SessionAttributes(LOGGER.log);                     
            this.productInventory = new ProductInventory(skillRequest);            
            this.scopeAndSequence = new ScopeAndSequenceDB(LOGGER.log);
            AlexaResponse.SetSessionAttributeHandler(sessionAttributes);
        }

        public async Task<SkillResponse> HandleRequest()
        {
            LOGGER.log.INFO("LaunchRequest", "HandleRequest");

            await this.userProfile.GetUserProfileData();
            await scopeAndSequence.GetSessionDataWithNumber(userProfile.GetUserSchedule());

            if (userProfile.RequiresPurchase())
            {
                LOGGER.log.DEBUG("LaunchRequest", "HandleRequest", "Schedule is premium content");

                await productInventory.GetAvailableProducts();
                string productName = scopeAndSequence.InSkillPurchase;

                if (productInventory.IsUnpaid(productName))
                {
                    LOGGER.log.DEBUG("LaunchRequest", "HandleRequest", "Premium content requires purchase");

                    string properName = productName.Replace('_', ' ');
                    this.sessionAttributes.UpdateProductName(properName);
                    return AlexaResponse.PurchaseContentUpsell(productInventory.GetProductId(productName),
                        CommonPhrases.Upsell(), properName);
                }
            }

            this.sessionAttributes.UpdateSessionAttributes(scopeAndSequence, userProfile.GetUserSchedule(), userProfile.GetMode());
            this.sessionAttributes.SessionState = STATE.Introduction;

            LOGGER.log.DEBUG("Launch", "HandleRequest", "Next word: " + sessionAttributes.CurrentWord);
            LOGGER.log.DEBUG("Function", "HandleLaunchRequest", "Lesson: " + sessionAttributes.Lesson);

            if (this.sessionAttributes.LessonMode == MODE.Teach)
            {
                this.teachMode = new TeachMode(this.sessionAttributes);
                WordAttributes wordAttributes = await WordAttributes.GetWordAttributes(this.sessionAttributes.CurrentWord);
                return this.teachMode.Introduction(wordAttributes);
            }
            else
            {
                return AlexaResponse.Introduction("Greetings my fellow Moycan! Lets learn to read. Are you ready to begin ?", "Say yes or no to continue.");
            }

        }


    }
}
