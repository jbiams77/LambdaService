using System;
using System.Collections.Generic;
using System.Text;
using Alexa.NET.APL.Components;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Response.APL;
using Alexa.NET.Response.Directive;
using Alexa.NET.Response.Directive.Templates;

namespace FlashCardService
{
    public class AlexaResponse
    {
        private static string StartTag { get { return "<speak>"; } }
        private static string EndTag { get { return "</speak>"; } }

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

        public static SkillResponse Introduction(IOutputSpeech introduction, string reprompt, bool displaySupported)
        {
            var response = AlexaResponse.SayWithReprompt(introduction, reprompt);

            if (displaySupported)
            {
                response.Response.Directives.Add(Create_IntroPresentation_Directive());
            }

            return response;
        }

        public static SkillResponse Introduction(bool displaySupported)
        {
            string introduction = StartTag + "Greetings my fellow Moycan! Lets learn to read. Are you ready to begin ?" + EndTag;

            var response = AlexaResponse.SayWithReprompt(new SsmlOutputSpeech(introduction), "Say yes or no to continue. ");
            
            if (displaySupported)
            {
                response.Response.Directives.Add(Create_IntroPresentation_Directive());
            }

            return response;
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

        public static SkillResponse GetResponse(string slotWord, string output, string reprompt, bool displaySupported)
        {


            if (displaySupported)
            {
                SkillResponse response = new SkillResponse { Version = "1.1" };

                var displayDirective = AlexaResponse.Create_CurrentWordPresentation_Directive(slotWord);
                var directive = AlexaResponse.Create_DynamicEntityDirective(slotWord);

                ResponseBody body = new ResponseBody
                {
                    ShouldEndSession = false,
                    OutputSpeech = new PlainTextOutputSpeech(output),
                    Reprompt = new Reprompt(reprompt)
                };

                response.Response = body;
                response.Response.Directives.Add(displayDirective);
                response.Response.Directives.Add(directive);


                return response;
            }
            else
            {
                string failedResponse = StartTag + "Oh no! Your current device does not support a display. Please try again on a device with display capabilities." + EndTag;
                return BuildResponse(new SsmlOutputSpeech(failedResponse), true, null, null, null);
            }

            
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
                            new Image("https://moyca-alexa-display.s3-us-west-2.amazonaws.com/Logo-01.png") { Width = "100%", Height = "100%", Align = "center" }
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

