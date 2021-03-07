using Infrastructure.Interfaces;
using Infrastructure.Lessons;
using Infrastructure.Logger;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Lessons
{
    public class LessonFactory
    {
        public static ILesson GetLesson(string lessonType, MoycaLogger log)
        {
            log.INFO("LessonFactory", "GetLesson", "Lesson Type: " + lessonType);
            switch (lessonType)
            {
                case "WF":
                    return new WordFamilies(log);
                    
                case "CVC":
                    return new ShortVowels(log);
                    
                case "CD":
                    return new ConsonantDigraph(log);

                case "CB":
                    return new ConsonantBlend(log);
            }

            throw new NotImplementedException("Lesson Type does not exist");
        }
        
    }
}
