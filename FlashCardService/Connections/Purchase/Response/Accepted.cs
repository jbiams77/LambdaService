using Alexa.NET.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlashCardService.Connections.Purchase.Response
{
    public class Accepted
    {
        public SkillResponse Handle()
        {
            return AlexaResponse.Say(CommonPhrases.UpSellAccepted());
        }
    }
}
