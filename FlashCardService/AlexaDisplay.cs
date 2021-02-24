using System;
using System.Collections.Generic;
using Alexa.NET.APL.Components;
using Alexa.NET.Response;
using Alexa.NET.Response.APL;

namespace FlashCardService
{
    public class AlexaDisplay
    {

        static readonly string FLASH_CARD_IMAGE = "https://moyca-alexa-display.s3-us-west-2.amazonaws.com/AlexaFlashCard.png";
        static readonly string INTRO_IMAGE = "https://moyca-alexa-display.s3-us-west-2.amazonaws.com/MoycaLogoSquareWords-01+(1).png";

        static readonly string SESSION_LOGOS_DIR = "https://moyca-alexa-display.s3-us-west-2.amazonaws.com/MoycaSessionLogos/";

        public RenderDocumentDirective GetCurrentWordDirective(string currentWord)
        {
            var directive = new RenderDocumentDirective
            {
                Token = "randomToken",
                Document = new APLDocument(APLDocumentVersion.V1_2)
                {
                    Imports = new List<Import> { new Import("alexa-layouts", "1.2.0") },
                    MainTemplate = new Layout(new[]
                    {
                        new Container(new APLComponent[]{
                            new AlexaBackground(){ BackgroundImageSource=FLASH_CARD_IMAGE},
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

        public RenderDocumentDirective GetUpsellDirective(string productId)
        {
            var display_image = GetSessionLogo(productId);

            var directive = new RenderDocumentDirective
            {
                Token = "randomToken",
                Document = new APLDocument
                {
                    MainTemplate = new Layout(new[]
                    {
                        new Container(
                            new Image(display_image) { Width = "100%", Height = "100%", Align = "center" }
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

        public RenderDocumentDirective GetIntroDirective()
        {

            var directive = new RenderDocumentDirective
            {
                Token = "randomToken",
                Document = new APLDocument
                {
                    MainTemplate = new Layout(new[]
                    {
                        new Container(
                            new Image(INTRO_IMAGE) { Width = "100%", Height = "100%", Align = "center" }
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

        private string GetSessionLogo(string productName)
        {
            return SESSION_LOGOS_DIR + productName + ".png";
        }

    }
}

