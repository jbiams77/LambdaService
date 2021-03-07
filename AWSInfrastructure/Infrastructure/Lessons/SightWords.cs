using Infrastructure;
using Infrastructure.Interfaces;
using Infrastructure.GlobalConstants;
using System;
using System.Collections.Generic;
using System.Text;
using Infrastructure.Logger;

namespace Infrastructure.Lessons
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
        public MoycaLogger Log { get; set; }

        public SightWords(MoycaLogger log)
        {
            Log = log;
        }

        public string Introduction(WordAttributes wordAttributes)
        {
            Log.INFO("SightWords", "Introduction", "WORD: " + wordAttributes.Word);

            string teachModel = "There are words used over, and over, and over, and over, and ";
            teachModel += SSML.PauseFor(.5);
            teachModel += " Well " + SSML.PauseFor(.5) + " you get the point. These are called sight words. ";
            teachModel += " It is helpful to just memorize them by sight. To see them and know what they say.";
            teachModel += " Are you ready to start? ";            

            return teachModel;
        }

        public string TeachTheWord(WordAttributes wordAttributes)
        {
            Log.INFO("SightWords", "TeachTheWord", "WORD: " + wordAttributes.Word);

            throw new NotImplementedException();
        }
    }
}
