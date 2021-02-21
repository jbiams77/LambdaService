using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.InSkillPricing.Responses;
using AWSInfrastructure.DynamoDB;
using AWSInfrastructure.GlobalConstants;
using AWSInfrastructure.CognitoPool;
using AWSInfrastructure.Logger;
using System.Linq;

namespace FlashCardService.Requests
{
    public class SessionEnded : Request
    {
        public UserProfileDB userProfile;
        public ScopeAndSequenceDB scopeAndSequence;
        public SkillResponse response;
        public string userId;
        private SessionAttributes sessionAttributes;
        private TeachMode teachMode;
        private ProductInventory purchase;
        private SkillRequest skillRequest;

        public SessionEnded(SkillRequest request)
        {
            this.sessionAttributes = new SessionAttributes(Function.log);
            this.teachMode = new TeachMode(this.sessionAttributes);
            this.purchase = new ProductInventory(request);
            this.userId = request.Session.User.UserId;
            this.userProfile = new UserProfileDB(userId, Function.log);
            this.scopeAndSequence = new ScopeAndSequenceDB(Function.log);
        }

        public override async Task<SkillResponse> HandleRequest()
        {
            this.sessionAttributes.SessionState = STATE.Off;
            return AlexaResponse.Say("Goodbye Moycan!");
        }
    }
}
