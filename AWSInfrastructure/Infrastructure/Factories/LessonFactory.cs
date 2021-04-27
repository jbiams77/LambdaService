using Infrastructure.Interfaces;
using Infrastructure.Lessons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Factories
{
    public class LessonFactory
    {
        public static ILesson GetLesson(string lessonType, bool display)
        {            

            switch (lessonType)
            {
                case "WF":
                case "Word Families":
                case "word families":
                case "word_families":
                    return new WordFamiliesLesson(display);
                    
                case "CVC":
                case "Short Vowels":
                case "short vowels":
                case "short_vowels":
                    return new ShortVowelsLesson(display);
                    
                case "CD":
                case "Consonant Digraphs":
                case "consonant digraphs":
                case "consonant_digraphs":
                    return new ConsonantDigraphLesson(display);

                case "CB":
                case "Consonant Blends":
                case "consonant blends":
                case "consonant_blends":
                    return new ConsonantBlendLesson(display);

                case "SW":
                case "Sight Words":
                case "sight words":
                case "sight_words":
                    return new SightWordsLesson(display);

                case "E":
                case "Long Vowels":
                case "long vowels":
                case "long_vowels":
                    return new LongVowelsLesson(display);
            }

            throw new NotImplementedException("Lesson Type does not exist: " + lessonType);
        }
        
    }
}
