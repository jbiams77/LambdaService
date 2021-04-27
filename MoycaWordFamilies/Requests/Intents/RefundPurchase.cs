using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Infrastructure.Alexa;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoycaWordFamilies.Requests.Intents
{
    public class RefundPurchase : Intent
    {
        public RefundPurchase(SkillRequest request) : base(request) { }

        public SkillResponse HandleIntent()
        {
            return MoycaResponse.RefundPurchaseResponse(base.words.ProductId, "word_families");
        }

    }
}
