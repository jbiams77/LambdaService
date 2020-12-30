using System;
using System.Collections.Generic;
using System.Text;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;

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

        //public static SkillResponse Introduction(List<FlashCard> flashCards)
        //{
        //    string introduction = StartTag + "Greetings my fellow Moycan! Today, we are working on ";

        //    int i;
        //    for (i = 0; i < flashCards.Count - 1; i++)
        //    {
        //        introduction += flashCards[i] + ",";
        //    }
        //    if (flashCards.Count > 1)
        //    {
        //        introduction += " and ";
        //    }
        //    introduction += flashCards[i];
        //    introduction += ", Are you ready to begin ?" + EndTag;

        //    return AlexaResponse.SayWithReprompt(new SsmlOutputSpeech(introduction), "Say yes or no to continue. ");
        //}

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

        public static SkillResponse GetResponse(string slotWord, IOutputSpeech output, string reprompt)
        {

            DialogUpdateDynamicEntities directive = AlexaResponse.Create_DynamicEntityDirective(slotWord);

            SkillResponse response = new SkillResponse { Version = "1.0" };

            ResponseBody body = new ResponseBody
            {
                ShouldEndSession = false,
                OutputSpeech = output,
                Reprompt = new Reprompt(reprompt)
            };

            response.Response = body;
            response.Response.Directives.Add(directive);

            return response;
        }

        public static SkillResponse GetResponse(string slotWord, string output, string reprompt)
        {

            DialogUpdateDynamicEntities directive = AlexaResponse.Create_DynamicEntityDirective(slotWord);

            SkillResponse response = new SkillResponse { Version = "1.0" };

            ResponseBody body = new ResponseBody
            {
                ShouldEndSession = false,
                OutputSpeech = new PlainTextOutputSpeech(output),
                Reprompt = new Reprompt(reprompt)
            };

            response.Response = body;
            response.Response.Directives.Add(directive);

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

    }

}

