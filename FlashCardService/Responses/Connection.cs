using System;
using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Request;
using AWSInfrastructure.DynamoDB;
using AWSInfrastructure.GlobalConstants;
using Alexa.NET.InSkillPricing.Responses;
using Alexa.NET.InSkillPricing;
using Alexa.NET;
using Alexa.NET.Request.Type;

namespace FlashCardService.Responses
{
    public class Connection 
    {
        private SkillRequest skillRequest;
        public Connection(SkillRequest skillRequest)
        {
            this.skillRequest = skillRequest;            
        }

        public SkillResponse HandleRequest()
        {

            string name = ((ConnectionResponseRequest<ConnectionResponsePayload>)skillRequest.Request).Name;
            
            switch (name)
            {
                case PaymentType.Buy:
                    return HandleBuy();

                case PaymentType.Upsell:
                    return HandleUpsell();

                case PaymentType.Cancel:
                    return HandleCancel();

            }

            return AlexaResponse.Say("Goodbye Moycan!"); ;
        }


        public SkillResponse HandleBuy()
        {
            return AlexaResponse.Say(" You want to buy Something.");
        }

        public SkillResponse HandleUpsell()
        {
            return AlexaResponse.Say(" You want to Upsell Something.");
        }

        public SkillResponse HandleCancel()
        {
            return AlexaResponse.Say(" You want to Upsell Something.");
        }


    }

}
