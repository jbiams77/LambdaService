using System;
using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Infrastructure.Alexa;
using MoycaWordFamilies.Utility;
using Infrastructure.GlobalConstants;

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

            var currentWordsToRead = new WordsToRead();
            await currentWordsToRead.GetRandomSession();

            MoycaResponse.SetSessionAttribute(currentWordsToRead);
            MoycaResponse.SetSessionPromptAndReprompt("TESTING");
            MoycaResponse.SetSessionDisplayValue("TESTING");
            MoycaResponse.ShouldEndSession(false);

            //LOGGER.log.DEBUG("Launch", "HandleRequest", "Math Problem: " + addition.ProblemDisplay);

            return MoycaResponse.Deliver();
        }


    }
}
