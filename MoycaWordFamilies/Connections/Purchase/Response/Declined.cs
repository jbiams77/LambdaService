using Alexa.NET.Response;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET;
using Infrastructure.Alexa;
using MoycaWordFamilies.Requests;

namespace MoycaWordFamilies.Connections.Purchase.Response
{
    public class Declined
    {
        SkillRequest skillRequest;
        public Declined(SkillRequest skillRequest)
        {
            this.skillRequest = skillRequest;
        }

        public async Task<SkillResponse> Handle()
        {
            MoycaResponse.Prompt = CommonPhrases.UpSellDeclined();
            return await new Launch(skillRequest).HandleRequest();
        }

    }
}
