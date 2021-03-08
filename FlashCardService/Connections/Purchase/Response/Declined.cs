using Alexa.NET.Response;
using Infrastructure.DynamoDB;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Alexa.NET.Request;

namespace FlashCardService.Connections.Purchase.Response
{
    public class Declined
    {
        private UserProfileDB userProfile;

        public Declined(SkillRequest skillRequest)
        {
            this.userProfile = new UserProfileDB(skillRequest.Session.User.UserId, LOGGER.log);            
        }

        public async Task<SkillResponse> Handle()
        {
            await this.userProfile.GetUserProfileData();

            if (this.userProfile.CurrentScheduleRequiresPurchase())
            {
                await this.userProfile.DecrementUserProfileSchedule();
            }                     

            return AlexaResponse.Say(CommonPhrases.UpSellDeclined());
        }

    }
}
