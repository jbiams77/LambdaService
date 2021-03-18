using Alexa.NET.Request;
using Alexa.NET.Response;
using FlashCardService.Factories;
using FlashCardService.Interfaces;
using Infrastructure.GlobalConstants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashCardService.Requests.Intents
{
    public class MakePurchase : Intent
    {

        public MakePurchase(SkillRequest request) : base(request) { }

        public async Task<SkillResponse> HandleIntent()
        {
            
            var intentRequest = (Alexa.NET.Request.Type.IntentRequest)skillRequest.Request;

            string lessonName = GetSlotValue(intentRequest);
            LOGGER.log.INFO("MakePurchase", "HandleIntent", "Lesson to purcase to: " + lessonName);
            ILesson lessonToPurchase = LessonFactory.GetLesson(lessonName);
            await base.products.GetAvailableProducts();

            if (base.products.IsUnpaid(lessonToPurchase.InSkillPurchaseName))
            {
                LOGGER.log.DEBUG("LaunchRequest", "HandleRequest", "Premium content requires purchase");

                this.sessionAttributes.ProductName = lessonToPurchase.InSkillPurchaseName;
                return AlexaResponse.PurchaseContentUpsell(base.products.GetProductId(lessonToPurchase.InSkillPurchaseName),
                    CommonPhrases.Upsell(), lessonToPurchase.InSkillPurchaseName);
            }

            
            await base.userProfile.ChangeLesson(lessonToPurchase);
            
            this.sessionAttributes.SessionState = STATE.Introduction;
            // relaunch for new lesson
            return await new LaunchRequest(base.skillRequest).HandleRequest();
        }

        private string GetSlotValue(Alexa.NET.Request.Type.IntentRequest input)
        {
            string slotValue = "";

            if (input.Intent.Slots?.Any() ?? false)
            {
                foreach (KeyValuePair<string, Slot> slot in input.Intent.Slots)
                {
                    slotValue = slot.Value.Value;
                }
            }

            return slotValue;
        }
    }
}
