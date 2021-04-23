using Alexa.NET.Request;
using Infrastructure.Alexa;
using Infrastructure.Factories;
using Infrastructure.GlobalConstants;
using Infrastructure.Interfaces;
using System;

namespace MoycaWordFamilies.Utility
{    
    public class WordFamilies : WordsToRead
    {
                
        private static readonly string WORD_FAMILIES = "https://moyca-lambda-dependancies.s3-us-west-2.amazonaws.com/word-families.json";
        private static int SIZE => 36;
        public new string CurrentWord => base.CurrentWord;
        private STATE state;

        public STATE State 
        {
            get 
            {
                return state;
            } 
            set 
            { 
                if (Enum.IsDefined(typeof(STATE), value))
                {
                    state = value;
                }
                else
                {
                    state = (STATE)value;                    
                }
            } 
        }

        public WordFamilies() : base(SIZE, WORD_FAMILIES, "word_families") 
        {
            state = STATE.Introduction;
            base.lesson = LessonFactory.GetLesson("word_families");
        }

        public WordFamilies(Session session) : base(session) 
        {
            
        }

        public string Introduction()
        {
            return base.lesson.Introduction(base.ListOfSessionWords[0]);
        }


    }
}
