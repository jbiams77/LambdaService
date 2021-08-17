using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;
using Alexa.NET.InSkillPricing.Directives;
using Infrastructure.Logger;
using Newtonsoft.Json;
using System.Collections.Generic;
using Alexa.NET.Request;
using Infrastructure.GlobalConstants;
using System.Threading.Tasks;

namespace Infrastructure.Alexa
{
    public class MoycaResponse
    {
        public static MoycaLogger log;
        private static string StartTag { get { return "<speak>"; } }
        private static string EndTag { get { return "</speak>"; } }

        private static readonly AlexaDisplay alexaDisplay = new AlexaDisplay();

        public static object SessionAttributes { get; set; }
        public static bool ShouldEndSession { get; set; }
        public static bool DisplaySupported { get; set; }
        public static string Prompt { get; set; }
        public static string Reprompt { get; set; }
        public static string SlotName { get; set; }
        public static string SlotWord { get; set; }
        public static string DisplayValue { get; set; }

        public static SkillResponse Deliver()
        {

            SkillResponse response = new SkillResponse { Version = "1.1" };

            response.Response = new ResponseBody
            {
                ShouldEndSession = ShouldEndSession,
                OutputSpeech = new SsmlOutputSpeech(StartTag + Prompt + EndTag),
                Reprompt = new Reprompt()
                {
                    OutputSpeech = new SsmlOutputSpeech(StartTag + Reprompt + EndTag)
                }
            };            

            if (SlotName != null)
            {
                var directive = MoycaResponse.Create_DynamicEntityDirective(SlotName, SlotWord);
                response.Response.Directives.Add(directive);
            }
            

            if (SessionAttributes != null) response.SessionAttributes = new Dictionary<string, object>()
            {
                {
                    "SessionAttribute",
                    SessionAttributes
                }
            };

            if (DisplaySupported)
            {
                var displayDirective = alexaDisplay.GetCurrentWordDirective(DisplayValue);
                response.Response.Directives.Add(displayDirective);
            }

            return response;
        }

        public static SkillResponse PurchaseContentUpsell(string productId, string productName)
        {
            var response = ResponseBuilder.Empty();
            response.Response.ShouldEndSession = false;
            response.Response.Directives.Add(new UpsellDirective(productId, "correlationToken", Prompt));

            if (SessionAttributes != null) response.SessionAttributes = new Dictionary<string, object>()
            {
                {
                    "SessionAttribute",
                    SessionAttributes
                }
            };

            if (DisplaySupported)
            {
                var displayDirective = alexaDisplay.GetUpsellDirective(productName);
                response.Response.Directives.Add(displayDirective);
            }

            return response;
        }

        public static SkillResponse RefundPurchaseResponse(string productId, string productName)
        {
            var response = ResponseBuilder.Empty();
            response.Response.ShouldEndSession = false;
            response.Response.Directives.Add(new CancelDirective(productId, "correlationToken"));

            if (SessionAttributes != null) response.SessionAttributes = new Dictionary<string, object>()
            {
                {
                    "SessionAttribute",
                    SessionAttributes
                }
            };

            if (DisplaySupported)
            {
                var displayDirective = alexaDisplay.GetUpsellDirective(productName);
                response.Response.Directives.Add(displayDirective);
            }

            return response;
        }

        private static DialogUpdateDynamicEntities Create_DynamicEntityDirective(string slotName, string slotWord)
        {
            var actual = new DialogUpdateDynamicEntities { UpdateBehavior = UpdateBehavior.Replace };
            var slotWordType = new SlotType
            {
                Name = slotName,
                Values = new[]
                {
                    new SlotTypeValue
                    {
                        Name = new SlotTypeValueName
                        {
                            Value = slotWord,
                            Synonyms = new[] {""}
                        }
                    }
                }
            };
            actual.Types.Add(slotWordType);
            return actual;
        }

    }

}

