using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Infrastructure.DynamoDB;
using Infrastructure.GlobalConstants;
using Infrastructure.Alexa;
using MoycaSubtraction;
using Alexa.NET;

namespace MoycaSubtraction.Requests
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
