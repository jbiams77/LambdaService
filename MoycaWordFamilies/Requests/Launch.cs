using System;
using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Infrastructure.Alexa;
using MoycaWordFamilies.Utility;
using Infrastructure.GlobalConstants;
using Infrastructure.Interfaces;

namespace MoycaWordFamilies.Requests
{
    public class Launch : IRequest
    {
        SkillRequest skillRequest;

        public Launch(SkillRequest skillRequest)
        {
            this.skillRequest = skillRequest;
        }

        public async Task<SkillResponse> HandleRequest()
        {
            LOGGER.log.INFO("LaunchRequest", "HandleRequest");

            var productInventory = new ProductInventory(skillRequest);

            try
            {
                await productInventory.UpdateProductInfo("word_families");
            }
            catch (Exception e)
            {
                LOGGER.log.WARN("LaunchRequest", "HandleRequest::UpdateProductInfo", e.Message);
            }

            var words = new WordFamilies(productInventory)
            {
                Attempts = 1
            };
            
            MoycaResponse.SessionAttributes = words;
            MoycaResponse.Prompt += words.Introduction();
            MoycaResponse.Reprompt = words.Introduction();
            MoycaResponse.SlotName = "wordToReadType";
            MoycaResponse.SlotWord = words.CurrentWord;
            MoycaResponse.DisplayValue = words.CurrentWord;
            MoycaResponse.ShouldEndSession = false;

            LOGGER.log.DEBUG("Launch", "HandleRequest", "Word to Read: " + words.CurrentWord);

            return MoycaResponse.Deliver();
        }


    }
}
