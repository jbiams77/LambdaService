using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Infrastructure.DynamoDB;
using Infrastructure.GlobalConstants;
using FlashCardService.Interfaces;

namespace FlashCardService.Requests
{
    public class SessionEnded : IRequest
    {
        public SessionEnded() {}

        public async Task<SkillResponse> HandleRequest()
        {
            LOGGER.log.INFO("SessionEnded", "HandleRequest");
            return AlexaResponse.Say("Goodbye Moycan!");
        }
    }
}
