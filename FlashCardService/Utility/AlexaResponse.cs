using System;
using System.Collections.Generic;
using System.Text;
using Alexa.NET;
using Alexa.NET.APL.Components;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Response.APL;
using Alexa.NET.Response.Directive;
using Alexa.NET.InSkillPricing;
using Alexa.NET.InSkillPricing.Directives;
using Alexa.NET.Response.Directive.Templates;
using Infrastructure.Logger;

namespace FlashCardService
{
    public class AlexaResponse
    {
        public static MoycaLogger log;
        private static SessionAttributes sessionAttributes;

        private static string StartTag { get { return "<speak>"; } }
        private static string EndTag { get { return "</speak>"; } }

        private static bool DisplaySupported;

        private static AlexaDisplay _alexaDisplay = new AlexaDisplay();
        public static void SetDisplaySupported(bool displaySupported)
        {
            DisplaySupported = displaySupported;
        }

        public static void SetSessionAttributeHandler(SessionAttributes sessionAttributesHandler)
        {
            sessionAttributes = sessionAttributesHandler;
        }

        /// <summary>
        /// Add a pause in Alexa's speech.
        /// </summary>
        /// <param name="milliseconds">Length of pause</param>
        /// <returns></returns>
        public static string Pause(int milliseconds)
        {
            return "<break time=\"" + milliseconds + "\"ms\"/>";
        }

        /// <summary>
        /// Speak the specified word slow
        /// </summary>
        /// <param name="rate"> The rate to speak. Default is slow. Options: x-slow, slow, medium, fast, x-fast</param>
        /// <returns></returns>
        public static string Slow(string word, string rate = "slow")
        {
            return "<prosody rate=\"" + rate + "\">" + word + "</prosody>";
        }

        /// <summary>
        /// Have Alexa say the word excitedly
        /// </summary>
        /// <param name="word">Word to sat</param>
        /// <param name="intensity">Excitement intencity. One of low, medium, high</param>
        /// <returns></returns>
        public static string Excited(string word, string intensity = "medium")
        {
            return "<amazon:emotion name=\"excited\" intensity=\""+intensity+"\">" + word + "</amazon:emotion>";
        }

        /// <summary>
        /// Have Alexa spell the specifed word
        /// </summary>
        /// <param name="word">Word to spell</param>
        /// <returns></returns>
        public static string SpellOut(string word)
        {
            return "<say-as interpret-as=\"spell-out\">" + word + "</say-as>";
        }

        public static SkillResponse Say(string speechResponse)
        {
            return Say(new PlainTextOutputSpeech { Text = speechResponse });
        }

        public static SkillResponse Say(IOutputSpeech speechResponse)
        {
            return BuildResponse(speechResponse, true, null, null);
        }

        public static SkillResponse SayWithReprompt(IOutputSpeech speechResponse, string reprompt)
        {
            return BuildResponse(speechResponse, false, new Reprompt(reprompt), null);
        }

        private static SkillResponse BuildResponse(IOutputSpeech outputSpeech, bool shouldEndSession, Reprompt reprompt, ICard card)
        {
            SkillResponse response = new SkillResponse { Version = "1.0" };

            ResponseBody body = new ResponseBody
            {
                ShouldEndSession = shouldEndSession,
                OutputSpeech = outputSpeech
            };

            if (reprompt != null) body.Reprompt = reprompt;
            if (card != null) body.Card = card;

            response.Response = body;
            response.SessionAttributes = sessionAttributes.ToDictionary();

            return response;
        }

        public static SkillResponse IntroductionWithCard(string cardText, string introduction, string reprompt)
        {
            return HandleFlashCard(cardText, introduction, reprompt);
        }

        public static SkillResponse Introduction(string introduction, string reprompt)
        {
            string intro = StartTag + introduction + EndTag;

            var response = AlexaResponse.SayWithReprompt(new SsmlOutputSpeech(intro), reprompt);

            if (DisplaySupported)
            {
                response.Response.Directives.Add(_alexaDisplay.GetIntroDirective());
            }

            response.SessionAttributes = sessionAttributes.ToDictionary();
            return response;
        }

        public static SkillResponse TeachFlashCard(string flashCardWord, string teachOutput)
        {
            return HandleFlashCard(flashCardWord, teachOutput, teachOutput);
        }

        public static SkillResponse PresentFlashCard(string flashCardWord, string output, string reprompt)
        {

            if (!DisplaySupported)
            {
                reprompt = Slow(SpellOut(flashCardWord), "x-slow");
                output += Slow(SpellOut(flashCardWord), "x-slow");
            }
            return HandleFlashCard(flashCardWord, output, reprompt);
        }

        private static SkillResponse HandleFlashCard(string flashCardWord, string output, string reprompt)
        {
            string reprompSpeech = StartTag + reprompt + EndTag;
            string speech = StartTag + output + EndTag;

            SkillResponse response = new SkillResponse { Version = "1.1" };

            var directive = AlexaResponse.Create_DynamicEntityDirective(flashCardWord);

            ResponseBody body = new ResponseBody
            {
                ShouldEndSession = false,
                OutputSpeech = new SsmlOutputSpeech(speech),
                Reprompt = new Reprompt()
                {
                    OutputSpeech = new SsmlOutputSpeech(reprompSpeech)
                }
            };

            response.Response = body;
            response.Response.Directives.Add(directive);
            response.SessionAttributes = sessionAttributes.ToDictionary();

            if (DisplaySupported)
            {
                var displayDirective = _alexaDisplay.GetCurrentWordDirective(flashCardWord);
                response.Response.Directives.Add(displayDirective);
            }

            return response;
        }

        public static SkillResponse PurchaseContentUpsell(string productId, string upsellPrompt, string productName)
        {
            var skillResponse = ResponseBuilder.Empty();
            skillResponse.Response.ShouldEndSession = false;
            skillResponse.Response.Directives.Add(
                            new UpsellDirective(productId, "correlationToken", upsellPrompt)
                        );
            skillResponse.SessionAttributes = sessionAttributes.ToDictionary();

            if (DisplaySupported)
            {
                var displayDirective = _alexaDisplay.GetUpsellDirective(productName);
                skillResponse.Response.Directives.Add(displayDirective);
            }

            return skillResponse;
        }

        private static DialogUpdateDynamicEntities Create_DynamicEntityDirective(string slotWord)
        {
            var actual = new DialogUpdateDynamicEntities { UpdateBehavior = UpdateBehavior.Replace };
            var wordsToReadType = new SlotType
            {
                Name = "wordsToReadType",
                Values = new[]
                {
                    new SlotTypeValue
                    {
                        Id = "1",
                        Name = new SlotTypeValueName
                        {
                            Value = slotWord,
                            Synonyms = new[] {""}
                        }
                    }
                }
            };
            actual.Types.Add(wordsToReadType);
            return actual;
        }

    }

}

