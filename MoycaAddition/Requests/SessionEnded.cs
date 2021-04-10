using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Infrastructure.DynamoDB;
using Infrastructure.GlobalConstants;
using FlashCardService.Interfaces;
using Infrastructure.Alexa;
using MoycaAddition;
using Alexa.NET;

namespace MoycaAddition.Requests
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
