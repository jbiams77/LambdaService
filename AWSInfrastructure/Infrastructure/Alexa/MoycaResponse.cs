using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;
using Alexa.NET.InSkillPricing.Directives;
using Infrastructure.Logger;
using Newtonsoft.Json;
using System.Collections.Generic;
using Alexa.NET.Request;

namespace Infrastructure.Alexa
{
    public class MoycaResponse
    {
        public static MoycaLogger log;
        private static string StartTag { get { return "<speak>"; } }
        private static string EndTag { get { return "</speak>"; } }

        private static readonly AlexaDisplay alexaDisplay = new AlexaDisplay();

        private static object _sessionAttribute;
        private static bool _shouldEndSession;
        private static bool _displaySupported;
        private static string _prompt;
        private static string _reprompt;
        private static string _slotName;
        private static string _slotValue;
        private static string _displayValue;
        
        public static void ShouldEndSession(bool shouldEndSession)
        {
            _shouldEndSession = shouldEndSession;
        }

        public static void SetSessionDisplayValue(string value)
        {
            _displayValue = value;
        }

        public static void SetSessionSlotTypeAndValue(string SlotName, string slotValue)
        {
            _slotName = SlotName;
            _slotValue = slotValue;
        }

        public static void SetSessionPromptAndReprompt(string prompt, string reprompt)
        {
            _prompt = prompt;
            _reprompt = reprompt;
        }
        public static void SetSessionPromptAndReprompt(string prompt)
        {
            _prompt = prompt;
            _reprompt = prompt;
        }

        public static void SetSessionPrompt(string prompt)
        {
            _prompt = prompt;
            _reprompt = prompt;
        }

        public static void SetSessionAttribute(object sessionAttribute)
        {
            _sessionAttribute = sessionAttribute;
        }

        public static void SetDisplaySupported(bool displaySupported)
        {
            _displaySupported = displaySupported;
        }

        public static SkillResponse Deliver()
        {

            SkillResponse response = new SkillResponse { Version = "1.1" };

            response.Response = new ResponseBody
            {
                ShouldEndSession = _shouldEndSession,
                OutputSpeech = new SsmlOutputSpeech(StartTag + _prompt + EndTag),
                Reprompt = new Reprompt()
                {
                    OutputSpeech = new SsmlOutputSpeech(StartTag + _reprompt + EndTag)
                }
            };            

            if (_slotName != null)
            {
                var directive = MoycaResponse.Create_DynamicEntityDirective(_slotName, _slotValue);
                response.Response.Directives.Add(directive);
            }
            

            if (_sessionAttribute != null) response.SessionAttributes = new Dictionary<string, object>()
            {
                {
                    "SessionAttribute",
                    _sessionAttribute
                }
            };

            if (_displaySupported)
            {
                var displayDirective = alexaDisplay.GetCurrentWordDirective(_displayValue);
                response.Response.Directives.Add(displayDirective);
            }

            return response;
        }


        public static SkillResponse PurchaseContentUpsell(string productId, string upsellPrompt, string productName, object obj)
        {
            var response = ResponseBuilder.Empty();
            response.Response.ShouldEndSession = false;
            response.Response.Directives.Add(new UpsellDirective(productId, "correlationToken", upsellPrompt));

            if (_sessionAttribute != null) response.SessionAttributes = new Dictionary<string, object>()
            {
                {
                    "SessionAttribute",
                    _sessionAttribute
                }
            };

            if (_displaySupported)
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

