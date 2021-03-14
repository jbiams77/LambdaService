using System;
using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Infrastructure.DynamoDB;
using Infrastructure.GlobalConstants;
using FlashCardService.Interfaces;
using Infrastructure;
using FlashCardService.Factories;

namespace FlashCardService.Requests
{
    public class LaunchRequest : IRequest
    {
        private UserProfileDB userProfile;
        private SessionAttributes sessionAttributes;
        private ProductInventory productInventory;


        public LaunchRequest(SkillRequest skillRequest)
        {
            this.userProfile = new UserProfileDB(skillRequest.Session.User.UserId, LOGGER.log);
            this.sessionAttributes = new SessionAttributes(LOGGER.log);                     
            this.productInventory = new ProductInventory(skillRequest);                        
            AlexaResponse.SetSessionAttributeHandler(sessionAttributes);
        }

        public async Task<SkillResponse> HandleRequest()
        {
            LOGGER.log.INFO("LaunchRequest", "HandleRequest");

            await this.userProfile.GetUserProfileData();

            if (userProfile.RequiresPurchase())
            {
                LOGGER.log.DEBUG("LaunchRequest", "HandleRequest", "Schedule is premium content");

                await productInventory.GetAvailableProducts();
                string productName = this.userProfile.lesson.InSkillPurchaseName;

                if (productInventory.IsUnpaid(productName))
                {
                    LOGGER.log.DEBUG("LaunchRequest", "HandleRequest", "Premium content requires purchase");

                    this.sessionAttributes.ProductName = productName;
                    return AlexaResponse.PurchaseContentUpsell(productInventory.GetProductId(productName),
                        CommonPhrases.Upsell(), productName);
                }
            }

            this.sessionAttributes.UpdateSessionAttributes(userProfile);
            this.sessionAttributes.SessionState = STATE.Introduction;

            WordAttributes wordAttributes = await WordAttributes.GetWordAttributes(this.sessionAttributes.CurrentWord, LOGGER.log);

            LOGGER.log.DEBUG("Launch", "HandleRequest", "Next word: " + sessionAttributes.CurrentWord);

            return LessonFactory.GetLesson(this.sessionAttributes.LessonType).Introduction(wordAttributes);
        }


    }
}
