﻿using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Infrastructure.DynamoDB;
using FlashCardService.Requests.Intents;

namespace FlashCardService.Requests.Intents
{
    public class Intent
    {
        protected UserProfileDB userProfile;
        protected SessionAttributes sessionAttributes;
        protected ProductInventory products;
        protected SkillRequest skillRequest;

        public Intent(SkillRequest request)
        {
            this.skillRequest = request;
            this.userProfile = new UserProfileDB(skillRequest.Session.User.UserId, LOGGER.log);
            this.sessionAttributes = new SessionAttributes(LOGGER.log);
            this.sessionAttributes.UpdateSessionAttributes(skillRequest.Session.Attributes);
            AlexaResponse.SetSessionAttributeHandler(sessionAttributes);
            this.products = new ProductInventory(request);
        }

    }
}
