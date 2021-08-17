using System;
using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Infrastructure.Alexa;
using MoycaSubtraction;
using MoycaSubtraction.Utility;
using Infrastructure.Interfaces;

namespace MoycaSubtraction.Requests
{
    public class Launch : IRequest
    {
        public Launch(SkillRequest skillRequest)
        {

        }

        public Task<SkillResponse> HandleRequest()
        {
            LOGGER.log.INFO("LaunchRequest", "HandleRequest");

            var subtraction = new Subtraction();

            MoycaResponse.SessionAttributes = subtraction.problem;
            MoycaResponse.Prompt += subtraction.ProblemPompt;
            MoycaResponse.Reprompt = subtraction.ProblemPompt;
            MoycaResponse.DisplayValue = subtraction.ProblemPompt;
            MoycaResponse.ShouldEndSession = false;

            LOGGER.log.DEBUG("Launch", "HandleRequest", "Math Problem: " + subtraction.ProblemDisplay);

            return Task.FromResult(MoycaResponse.Deliver());
        }


    }
}
