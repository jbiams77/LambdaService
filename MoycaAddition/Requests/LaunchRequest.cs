using System;
using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Request;
using FlashCardService.Interfaces;
using Infrastructure.Alexa;
using MoycaAddition;
using MoycaAddition.Utility;

namespace MoycaAddition.Requests
{
    public class LaunchRequest : IRequest
    {
        public LaunchRequest(SkillRequest skillRequest)
        {

        }

        public SkillResponse HandleRequest()
        {
            LOGGER.log.INFO("LaunchRequest", "HandleRequest");

            var addition = new Addition();
            
            MoycaResponse.SetSessionAttribute(addition.problem);
            MoycaResponse.SetSessionPromptAndReprompt(addition.ProblemPompt);
            MoycaResponse.SetSessionSlotTypeAndValue("mathProblemType", addition.Answer.ToString());
            MoycaResponse.SetSessionDisplayValue(addition.ProblemDisplay);
            MoycaResponse.ShouldEndSession(false);

            LOGGER.log.DEBUG("Launch", "HandleRequest", "Math Problem: " + addition.ProblemDisplay);

            return MoycaResponse.Deliver();
        }


    }
}
