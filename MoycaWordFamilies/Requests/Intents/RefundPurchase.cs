using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Infrastructure.Alexa;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MoycaWordFamilies.Requests.Intents
{
    public class RefundPurchase : Intent
    {
        public RefundPurchase(SkillRequest request) : base(request) { }

        public async Task<SkillResponse> HandleIntent()
        {
            if (!base.words.Purchased && !base.words.Purchasable)
            {
                MoycaResponse.Prompt += CommonPhrases.InvalidRefund();
                return await new Launch(base.skillRequest).HandleRequest();
            }
            else
            {
                return MoycaResponse.RefundPurchaseResponse(base.words.ProductId, "word_families");
            }
        }

    }
}
