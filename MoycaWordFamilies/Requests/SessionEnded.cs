using Alexa.NET.Response;
using Alexa.NET;
using Infrastructure.Interfaces;
using System.Threading.Tasks;

namespace MoycaWordFamilies.Requests
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
