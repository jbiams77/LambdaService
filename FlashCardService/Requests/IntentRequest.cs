using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Infrastructure.DynamoDB;
using FlashCardService.Requests.Intents;
using FlashCardService.Interfaces;

namespace FlashCardService.Requests
{

    public class IntentRequest : IRequest
    {
        private SkillRequest skillRequest;

        public IntentRequest(SkillRequest skillRequest) 
        {
            this.skillRequest = skillRequest;
        }


        public async Task<SkillResponse> HandleRequest()
        {
            string intentName = ((Alexa.NET.Request.Type.IntentRequest)skillRequest.Request).Intent.Name;            

            LOGGER.log.INFO("IntentRequest", "HandleRequest", intentName);
            
            switch (intentName)
            {
                case "AMAZON.YesIntent":
                    return await new Yes(this.skillRequest).HandleIntent();
                    
                case "AMAZON.NoIntent":
                    return await new No(this.skillRequest).HandleIntent();
                    
                case "AMAZON.CancelIntent":
                    return new Cancel(this.skillRequest).HandleIntent();

                case "AMAZON.StopIntent":
                    return new Stop(this.skillRequest).HandleIntent();

                case "AMAZON.HelpIntent":
                    return await new Help(this.skillRequest).HandleIntent();

                case "WordsToReadIntent":
                    return await new WordsToRead(this.skillRequest).HandleIntent();

                case "MoveToNewLessonIntent":
                    return await new MoveToNewLesson(this.skillRequest).HandleIntent();

                case "AMAZON.FallbackIntent":
                    return await new Fallback(this.skillRequest).HandleIntent();
                    
                default:                    
                    return ResponseBuilder.Tell("When you are ready to begin say, 'Alexa, open Moyca Readers'. Goodbye.");
            }

        }

    }
}