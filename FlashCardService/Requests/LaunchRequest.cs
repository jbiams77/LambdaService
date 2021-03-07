using System;
using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Infrastructure.DynamoDB;
using Infrastructure.GlobalConstants;
using FlashCardService.Interfaces;
using Infrastructure;
using Infrastructure.Interfaces;
using Infrastructure.Lessons;

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
                string productName = this.userProfile.scopeAndSequenceDB.ProductName;

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

            LOGGER.log.DEBUG("Launch", "HandleRequest", "Next word: " + sessionAttributes.CurrentWord);
            LOGGER.log.DEBUG("Function", "HandleLaunchRequest", "Lesson: " + sessionAttributes.LessonType);

            if (this.sessionAttributes.LessonMode == MODE.Teach)
            {
                WordAttributes wordAttributes = await WordAttributes.GetWordAttributes(this.sessionAttributes.CurrentWord, LOGGER.log);
                string lesson = LessonFactory.GetLesson(this.sessionAttributes.LessonType, LOGGER.log).Introduction(wordAttributes);
                return AlexaResponse.PresentFlashCard(wordAttributes.Word, lesson, "Please say " + wordAttributes.Word);
            }
            else
            {
                return AlexaResponse.Introduction("Greetings my fellow Moycan! Lets learn to read. Are you ready to begin ?", "Say yes or no to continue.");
            }

        }


    }
}
