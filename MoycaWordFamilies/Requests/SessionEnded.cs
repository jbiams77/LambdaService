using Alexa.NET.Response;
using Alexa.NET;
using Infrastructure.Interfaces;

namespace MoycaWordFamilies.Requests
{
    public class SessionEnded : IRequest
    {
        public SessionEnded() {}

        public SkillResponse HandleRequest()
        {
            LOGGER.log.INFO("SessionEnded", "HandleRequest");
            return ResponseBuilder.Tell("Goodbye Moycan!");
        }
    }
}
