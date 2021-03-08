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
                case "Word Families":
                case "word families":
                case "word_families":
                    return new WordFamilies(log);
                    
                case "CVC":
                case "Short Vowels":
                case "short vowels":
                case "short_vowels":
                    return new ShortVowels(log);
                    
                case "CD":
                case "Consonant Digraphs":
                case "consonant digraphs":
                case "consonant_digraphs":
                    return new ConsonantDigraph(log);

                case "CB":
                case "Consonant Blends":
                case "consonant blends":
                case "consonant_blends":
                    return new ConsonantBlend(log);
            }

            throw new NotImplementedException("Lesson Type does not exist");
        }
        
    }
}
