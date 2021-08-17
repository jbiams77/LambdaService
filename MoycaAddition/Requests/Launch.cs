using System;
using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Request;
using FlashCardService.Interfaces;
using Infrastructure.Alexa;
using MoycaAddition;
using MoycaAddition.Utility;
using Infrastructure.Interfaces;

namespace MoycaAddition.Requests
{
    public class Launch : IRequest
    {
        public Launch(SkillRequest skillRequest)
        {

        }

        public Task<SkillResponse> HandleRequest()
        {
            LOGGER.log.INFO("LaunchRequest", "HandleRequest");

            var addition = new Addition();           
            
            MoycaResponse.SessionAttributes = addition.problem;
            MoycaResponse.Prompt += addition.ProblemPompt;
            MoycaResponse.Reprompt = addition.ProblemPompt;            
            MoycaResponse.DisplayValue = addition.ProblemPompt;
            MoycaResponse.ShouldEndSession = false;


            LOGGER.log.DEBUG("Launch", "HandleRequest", "Math Problem: " + addition.ProblemDisplay);

            return Task.FromResult(MoycaResponse.Deliver());
        }

    }
}
