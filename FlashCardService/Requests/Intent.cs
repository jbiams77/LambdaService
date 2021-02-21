using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using AWSInfrastructure.DynamoDB;
using FlashCardService.Requests.Intents;

namespace FlashCardService.Requests
{
    public class Intent : Request
    {
        protected UserProfileDB userProfile;
        protected ScopeAndSequenceDB scopeAndSequence;
        protected SkillResponse response;
        protected string userId;
        protected SessionAttributes sessionAttributes;
        protected TeachMode teachMode;
        protected ProductInventory purchase;
        protected SkillRequest skillRequest;


        public Intent(){ }

        public Intent(SkillRequest request)
        {
            SkillRequest skillRequest = request;
            this.sessionAttributes = new SessionAttributes(Function.log);
            this.teachMode = new TeachMode(this.sessionAttributes);
            this.purchase = new ProductInventory(request);
            this.userProfile = new UserProfileDB(request.Session.User.UserId, Function.log);
            this.scopeAndSequence = new ScopeAndSequenceDB(Function.log);
            AlexaResponse.SetSessionAttributeHandler(sessionAttributes);
        }

        public override async Task<SkillResponse> HandleRequest()
        {
            var request = (IntentRequest)skillRequest.Request;
            this.sessionAttributes.UpdateSessionAttributes(skillRequest.Session.Attributes);

            Function.log.INFO("Function", "HandleIntentRequest", request.Intent.Name);
            
            switch (request.Intent.Name)
            {
                case "AMAZON.YesIntent":
                    Yes yes = new Yes();
                    return await yes.HandleIntent();
                    
                case "AMAZON.NoIntent":
                    No no = new No();
                    return await no.HandleIntent();
                    
                case "AMAZON.CancelIntent":
                    Cancel cancel = new Cancel();
                    return await cancel.HandleIntent();

                case "AMAZON.StopIntent":
                    Stop stop = new Stop();
                    return await stop.HandleIntent();

                case "AMAZON.HelpIntent":
                    Help help = new Help();
                    return await help.HandleIntent();

                case "WordsToReadIntent":
                    WordsToRead wordsToRead = new WordsToRead();
                    return await wordsToRead.HandleIntent();

                case "AMAZON.FallbackIntent":
                    Fallback fallback = new Fallback();
                    return await fallback.HandleIntent();
                    
                default:                    
                    return ResponseBuilder.Tell("When you are ready to begin say, 'Alexa, open Moyca Readers'. Goodbye.");
            }

        }

    }
}