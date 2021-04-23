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
    public class LaunchRequest : IRequestAsync
    {
        public LaunchRequest(SkillRequest skillRequest)
        {

        }

        public async Task<SkillResponse> HandleRequest()
        {
            LOGGER.log.INFO("LaunchRequest", "HandleRequest");

            var wordSession = new WordFamilies();            

            MoycaResponse.SetSessionAttribute(wordSession);
            MoycaResponse.SetSessionPromptAndReprompt(wordSession.Introduction());
            MoycaResponse.SetSessionDisplayValue(wordSession.CurrentWord);
            MoycaResponse.ShouldEndSession(false);

            LOGGER.log.DEBUG("Launch", "HandleRequest", "Word to Read: " + wordSession.CurrentWord);

            return MoycaResponse.Deliver();
        }


    }
}
