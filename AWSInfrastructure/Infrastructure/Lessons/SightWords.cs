using Infrastructure;
using Infrastructure.Interfaces;
using Infrastructure.GlobalConstants;
using System;
using System.Collections.Generic;
using System.Text;
using Infrastructure.Logger;
using Alexa.NET.Response;
using Alexa.NET;
using Infrastructure.Alexa;

namespace Infrastructure.Lessons
{
    public class SightWords : ILesson
    {
        public string ProductName => "Sight Words";
        public string InSkillPurchaseName => "sight_words";
        public string LessonTypeName => "SW";

        private string quickReply;
        public string QuickReply
        {
            set { quickReply = value; }
            get { return quickReply + SSML.PauseFor(1) ?? (quickReply = ""); }
        }

        public string Dialogue(MODE mode, WordEntry wordAttributes)
        {
            switch (mode)
            {
                case MODE.Assess:
                    return AssessTheWord(wordAttributes);
                case MODE.Teach:
                    return TeachTheWord(wordAttributes);
                default:
                    return "ERROR";
            }
        }

        public string Introduction(WordEntry wordAttributes)
        {
            string teachModel = "There are words used over, and over, and over, and over, and ";
            teachModel += SSML.PauseFor(.5);
            teachModel += " Well " + SSML.PauseFor(.5) + " you get the point. These are called sight words. ";
            teachModel += " It is helpful to just memorize them by sight. To see them and know what they say.";
            teachModel += " Are you ready to start? ";            

            return teachModel; 
        }

        public string TeachTheWord(WordEntry wordAttributes)
        {
            throw new NotImplementedException();
        }

        private string AssessTheWord(WordEntry wordAttributes)
        {
            string output = QuickReply + " Say the word";
            return output;
        }
    }
}
