using Alexa.NET.Request;
using Alexa.NET.Response;
using Infrastructure.Factories;
using Infrastructure.Interfaces;
using Infrastructure.GlobalConstants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alexa.NET;
using Infrastructure.Alexa;

namespace MoycaWordFamilies.Requests.Intents
{
    public class MakePurchase : Intent
    {

        public MakePurchase(SkillRequest request) : base(request) { }

        public async Task<SkillResponse> HandleIntent()
        {
            if (!base.words.Purchased && base.words.Purchasable)
            {
                MoycaResponse.Prompt += CommonPhrases.Upsell(" Word Family ", " Word Families");
                return MoycaResponse.PurchaseContentUpsell(base.words.ProductId, "word_families");
            }
            else if (!base.words.Purchased && !base.words.Purchasable)
            {
                MoycaResponse.Prompt += CommonPhrases.NotPurchaseable();
                return await new Launch(base.skillRequest).HandleRequest();
            }

            MoycaResponse.Prompt = "You already purchased all available content. " + SSML.PauseFor(0.5);

            return await new Launch(base.skillRequest).HandleRequest();
        }
    }
}
