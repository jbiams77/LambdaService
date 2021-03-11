using Infrastructure;
using FlashCardService.Interfaces;
using Infrastructure.GlobalConstants;
using System;
using System.Collections.Generic;
using System.Text;
using Infrastructure.Logger;
using Alexa.NET.Response;
using Alexa.NET;

namespace FlashCardService.Lessons
{
    public class SightWords : ILesson
    {
        public string ProductName => throw new NotImplementedException();

        public int FreeStartIndex => throw new NotImplementedException();

        public int FreeEndIndex => throw new NotImplementedException();

        public int CostStartIndex => throw new NotImplementedException();

        public int CostEndIndex => throw new NotImplementedException();

        public string InSkillPurchaseName => throw new NotImplementedException();

        public string LessonTypeName => throw new NotImplementedException();

        private string quickReply;
        public string QuickReply
        {
            set { quickReply = value; }
            get { return quickReply + SSML.PauseFor(1) ?? (quickReply = ""); }
        }

        public SkillResponse Dialogue(MODE mode, WordAttributes wordAttributes)
        {
            switch (mode)
            {
                case MODE.Assess:
                    return AssessTheWord(wordAttributes);
                case MODE.Teach:
                    return TeachTheWord(wordAttributes);
                default:
                    return ResponseBuilder.Tell("ERROR");
            }
        }

        public SkillResponse Introduction(WordAttributes wordAttributes)
        {
            LOGGER.log.INFO("SightWords", "Introduction", "WORD: " + wordAttributes.Word);

            string teachModel = "There are words used over, and over, and over, and over, and ";
            teachModel += SSML.PauseFor(.5);
            teachModel += " Well " + SSML.PauseFor(.5) + " you get the point. These are called sight words. ";
            teachModel += " It is helpful to just memorize them by sight. To see them and know what they say.";
            teachModel += " Are you ready to start? ";            

            return AlexaResponse.Introduction(teachModel, " Please say yes to continue or no to quit"); 
        }

        public SkillResponse TeachTheWord(WordAttributes wordAttributes)
        {
            LOGGER.log.INFO("SightWords", "TeachTheWord", "WORD: " + wordAttributes.Word);

            throw new NotImplementedException();
        }

        private SkillResponse AssessTheWord(WordAttributes wordAttributes)
        {
            string output = QuickReply + " Say the word";
            return AlexaResponse.PresentFlashCard(wordAttributes.Word, output, CommonPhrases.TryAgain);
        }
    }
}
