using System;
using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Infrastructure.Alexa;
using MoycaSubtraction;
using MoycaSubtraction.Utility;

namespace MoycaSubtraction.Requests
{
    public class LaunchRequest : IRequest
    {
        public LaunchRequest(SkillRequest skillRequest)
        {

        }

        public SkillResponse HandleRequest()
        {
            LOGGER.log.INFO("LaunchRequest", "HandleRequest");

            var subtraction = new Subtraction();
            
            MoycaResponse.SetSessionAttribute(subtraction.problem);
            MoycaResponse.SetSessionPromptAndReprompt(subtraction.ProblemPompt);
            //MoycaResponse.SetSessionSlotTypeAndValue("mathProblemType", subtraction.Answer.ToString());
            MoycaResponse.SetSessionDisplayValue(subtraction.ProblemDisplay);
            MoycaResponse.ShouldEndSession(false);

            LOGGER.log.DEBUG("Launch", "HandleRequest", "Math Problem: " + subtraction.ProblemDisplay);

            return MoycaResponse.Deliver();
        }


    }
}
