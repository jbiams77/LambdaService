using FlashCardService.Interfaces;
using FlashCardService.Lessons;
using Infrastructure.Logger;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlashCardService.Factories
{
    public class LessonFactory
    {
        public static ILesson GetLesson(string lessonType)
        {
            LOGGER.log.INFO("LessonFactory", "GetLesson", "Lesson Type: " + lessonType);
            switch (lessonType)
            {
                case "WF":
                case "Word Families":
                case "word families":
                case "word_families":
                    return new WordFamilies();
                    
                case "CVC":
                case "Short Vowels":
                case "short vowels":
                case "short_vowels":
                    return new ShortVowels();
                    
                case "CD":
                case "Consonant Digraphs":
                case "consonant digraphs":
                case "consonant_digraphs":
                    return new ConsonantDigraph();

                case "CB":
                case "Consonant Blends":
                case "consonant blends":
                case "consonant_blends":
                    return new ConsonantBlend();
            }

            throw new NotImplementedException("Lesson Type does not exist");
        }
        
    }
}
