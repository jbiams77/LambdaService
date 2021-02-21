using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using AWSInfrastructure.DynamoDB;
using FlashCardService.Requests.Intents;

namespace FlashCardService.Requests.Intents
{
    public class Intent
    {
        protected UserProfileDB userProfile;
        protected ScopeAndSequenceDB scopeAndSequence;
        protected SkillResponse response;
        protected string userId;
        protected SessionAttributes sessionAttributes;
        protected TeachMode teachMode;
        protected ProductInventory purchase;
        protected SkillRequest skillRequest;
        protected IntentRequest derivedIntent;

        public Intent(SkillRequest request)
        {
            this.skillRequest = request;
            this.sessionAttributes = new SessionAttributes(Function.log);
            this.sessionAttributes.UpdateSessionAttributes(skillRequest.Session.Attributes);
            AlexaResponse.SetSessionAttributeHandler(sessionAttributes);
            this.teachMode = new TeachMode(this.sessionAttributes);
            this.purchase = new ProductInventory(request);
        }

    }
}
