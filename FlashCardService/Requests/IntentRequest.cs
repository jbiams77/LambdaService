using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using AWSInfrastructure.DynamoDB;
using FlashCardService.Requests.Intents;

namespace FlashCardService.Requests
{

    public class IntentRequest : Request
    {
        private SkillRequest skillRequest;

        public IntentRequest(SkillRequest skillRequest) 
        {
            this.skillRequest = skillRequest;
        }


        public override async Task<SkillResponse> HandleRequest()
        {
            string intentName = ((Alexa.NET.Request.Type.IntentRequest)skillRequest.Request).Intent.Name;            

            Function.log.INFO("Intent", "HandleRequest", intentName);
            
            switch (intentName)
            {
                case "AMAZON.YesIntent":
                    Yes yes = new Yes(this.skillRequest);
                    return await yes.HandleIntent();
                    
                case "AMAZON.NoIntent":
                    No no = new No(this.skillRequest);
                    return await no.HandleIntent();
                    
                case "AMAZON.CancelIntent":
                    Cancel cancel = new Cancel(this.skillRequest);
                    return await cancel.HandleIntent();

                case "AMAZON.StopIntent":
                    Stop stop = new Stop(this.skillRequest);
                    return await stop.HandleIntent();

                case "AMAZON.HelpIntent":
                    Help help = new Help(this.skillRequest);
                    return await help.HandleIntent();

                case "WordsToReadIntent":
                    WordsToRead wordsToRead = new WordsToRead(this.skillRequest);
                    return await wordsToRead.HandleIntent();

                case "AMAZON.FallbackIntent":
                    Fallback fallback = new Fallback(this.skillRequest);
                    return await fallback.HandleIntent();
                    
                default:                    
                    return ResponseBuilder.Tell("When you are ready to begin say, 'Alexa, open Moyca Readers'. Goodbye.");
            }

        }

    }
}