using Infrastructure.GlobalConstants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlashCardService
{
    public class GradeBook
    {
        private static int ReAttempts {get {return 2;} }

        public static void Missed(SessionAttributes sessionAttributes)
        {           

            if (sessionAttributes.Attempts + 1 >= ReAttempts)
            {
                
                sessionAttributes.LessonMode = MODE.Teach;
            }
            else
            {
                sessionAttributes.LessonMode = MODE.Assess;
            }

            sessionAttributes.Attempts += 1;
        }

        public static void Passed(SessionAttributes sessionAttributes)
        {
            sessionAttributes.RemoveCurrentWord();
            sessionAttributes.Attempts = 0;
            sessionAttributes.LessonMode = MODE.Assess;
        }

    }
}
