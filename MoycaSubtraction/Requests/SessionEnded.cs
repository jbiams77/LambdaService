using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Infrastructure.DynamoDB;
using Infrastructure.GlobalConstants;
using Infrastructure.Alexa;
using MoycaSubtraction;
using Alexa.NET;
using Infrastructure.Interfaces;

namespace MoycaSubtraction.Requests
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
