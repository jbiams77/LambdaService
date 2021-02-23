using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Request;
using AWSInfrastructure.DynamoDB;
using AWSInfrastructure.GlobalConstants;
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
