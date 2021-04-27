using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Infrastructure.Alexa;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MoycaWordFamilies.Requests;

namespace MoycaWordFamilies.Connections.Purchase.Response
{
    public class Accepted
    {
        SkillRequest skillRequest;
        public Accepted(SkillRequest skillRequest)
        {
            this.skillRequest = skillRequest;
        }

        public async Task<SkillResponse> Handle()
        {
            MoycaResponse.Prompt = CommonPhrases.UpSellAccepted("Word Families");
            return await new Launch(skillRequest).HandleRequest();
        }
    }
}
