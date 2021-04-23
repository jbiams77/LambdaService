using Infrastructure.Interfaces;
using Infrastructure.Lessons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Factories
{
    public class LessonFactory
    {
        public static ILesson GetLesson(string lessonType)
        {            

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

                case "SW":
                case "Sight Words":
                case "sight words":
                case "sight_words":
                    return new SightWords();

                case "E":
                case "Long Vowels":
                case "long vowels":
                case "long_vowels":
                    return new LongVowels();
            }

            throw new NotImplementedException("Lesson Type does not exist: " + lessonType);
        }
        
    }
}
