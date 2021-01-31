using System;
using System.Collections.Generic;
using System.Text;
using Alexa.NET.APL.Components;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Response.APL;
using Alexa.NET.Response.Directive;
using Alexa.NET.Response.Directive.Templates;
using AWSInfrastructure.Logger;

namespace FlashCardService
{
    public class AlexaResponse
    {
        public static MoycaLogger log;

        private static string StartTag { get { return "<speak>"; } }
        private static string EndTag { get { return "</speak>"; } }

        private static bool DisplaySupported;
        public static void SetDisplaySupported(bool displaySupported)
        {
            DisplaySupported = displaySupported;
        }
        public static void SetLogger(MoycaLogger logger)
        {
            log = logger;
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
            return BuildResponse(speechResponse, true, null, null, null);
        }

        public static SkillResponse SayWithReprompt(IOutputSpeech speechResponse, string reprompt)
        {
            return BuildResponse(speechResponse, false, null, new Reprompt(reprompt), null);
        }

        private static SkillResponse BuildResponse(IOutputSpeech outputSpeech, bool shouldEndSession, Session sessionAttributes, Reprompt reprompt, ICard card)
        {
            SkillResponse response = new SkillResponse { Version = "1.0" };
            if (sessionAttributes != null) response.SessionAttributes = sessionAttributes.Attributes;

            ResponseBody body = new ResponseBody
            {
                ShouldEndSession = shouldEndSession,
                OutputSpeech = outputSpeech
            };

            if (reprompt != null) body.Reprompt = reprompt;
            if (card != null) body.Card = card;

            response.Response = body;

            return response;
        }

        public static SkillResponse IntroductionWithCard(string cardText, string introduction, string reprompt)
        {
            return HandleFlashCard(cardText, 0, introduction, reprompt);
        }

        public static SkillResponse Introduction(string introduction, string reprompt)
        {
            string intro = StartTag + introduction + EndTag;

            var response = AlexaResponse.SayWithReprompt(new SsmlOutputSpeech(intro), reprompt);

            if (DisplaySupported)
            {
                response.Response.Directives.Add(Create_IntroPresentation_Directive());
            }

            return response;
        }

        public static SkillResponse PresentFlashCard(string flashCardWord, int attemptsMade, string output, string reprompt)
        {
            if (!DisplaySupported)
            {
                reprompt = Slow(SpellOut(flashCardWord), "x-slow");
                output += Slow(SpellOut(flashCardWord), "x-slow");
            }
            return HandleFlashCard(flashCardWord, attemptsMade, output, reprompt);
        }

        private static SkillResponse HandleFlashCard(string flashCardWord, int attemptsMade, string output, string reprompt)
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
            response.SessionAttributes = new Dictionary<string, object>()
            {
                { "TotalFailedAttempts", attemptsMade.ToString() }
            };

            if (DisplaySupported)
            {
                var displayDirective = AlexaResponse.Create_CurrentWordPresentation_Directive(flashCardWord);
                response.Response.Directives.Add(displayDirective);
            }

            return response;
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

        private static RenderDocumentDirective Create_CurrentWordPresentation_Directive(string currentWord)
        {
            var directive = new RenderDocumentDirective
            {
                Token = "randomToken",
                Document = new APLDocument(APLDocumentVersion.V1_2)
                {
                    Imports = new List<Import>{ new Import("alexa-layouts", "1.2.0") },
                    MainTemplate = new Layout(new[]
                    {
                        new Container(new APLComponent[]{
                            new AlexaBackground(){ BackgroundImageSource="https://moyca-alexa-display.s3-us-west-2.amazonaws.com/AlexaFlashCard.png"},
                            new Text(currentWord)
                            {
                                Width="100%",
                                Height="100%",
                                FontSize="160dp",
                                TextAlign="center",
                                TextAlignVertical="center",
                                Color="black"
                            },
                        })
                        {
                            Width="100%",
                            Height="100%",
                            AlignItems="center",
                            Direction="column",
                            JustifyContent="center",
                        }
                    })
                }
            };

            return directive;
        }

        private static RenderDocumentDirective Create_IntroPresentation_Directive()
        {

            var directive = new RenderDocumentDirective
            {
                Token = "randomToken",
                Document = new APLDocument
                {
                    MainTemplate = new Layout(new[]
                    {
                        new Container(
                            new Image("https://moyca-alexa-display.s3-us-west-2.amazonaws.com/MoycaLogoSquareWords-01+(1).png") { Width = "100%", Height = "100%", Align = "center" }
                        )
                        {
                            Width = "100%",
                            Height = "100%",
                            AlignItems = "center",
                            JustifyContent = "center",
                            Direction = "column",
                        }
                    })
                }
            };

            return directive;
        }

    }

}

