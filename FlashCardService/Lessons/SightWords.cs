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
        public string ProductName => "Sight Words";
        public string InSkillPurchaseName => "sight_words";
        public string LessonTypeName => "SW";

        public int FreeStartIndex => 1155;

        public int FreeEndIndex => 1156;

        public int CostStartIndex => 1157;

        public int CostEndIndex => 1164;

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
