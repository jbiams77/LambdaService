using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Infrastructure.DynamoDB;
using Infrastructure.GlobalConstants;
using FlashCardService.Interfaces;
using Infrastructure.Alexa;
using MoycaAddition;
using Alexa.NET;
using Infrastructure.Interfaces;

namespace MoycaAddition.Requests
{
    public class SessionEnded : IRequest
    {
        public SessionEnded() {}

        public Task<SkillResponse> HandleRequest()
        {
            LOGGER.log.INFO("SessionEnded", "HandleRequest");
            return Task.FromResult(ResponseBuilder.Tell("Goodbye Moycan!"));
        }
    }
}
