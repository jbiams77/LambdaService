using Infrastructure.Interfaces;
using Infrastructure.GlobalConstants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Logger;

namespace Infrastructure.Lessons
{
    public class ShortVowels : ILesson
    {
        public string ProductName => "Short Vowels";
        public string InSkillPurchaseName => "short_vowels";
        public string LessonTypeName => "CVC";
        public int FreeStartIndex => 1036;
        public int FreeEndIndex => 1038;
        public int CostStartIndex => 1039;
        public int CostEndIndex => 1095;
        public MoycaLogger Log { get; set; }

        public ShortVowels(MoycaLogger log)
        {
            Log = log;
        }

        public string Introduction(WordAttributes wordAttributes)
        {
            Log.INFO("ShortVowels", "Introduction", "WORD: " + wordAttributes.Word);

            string teachModel = "In the alphabet, there are two types of letters.";
            string vowel = wordAttributes.Vowel;
            string vowelSound = wordAttributes.VowelPhoneme;
            teachModel += SSML.PauseFor(0.5);
            teachModel += "Vowels and Consonants.";
            teachModel += SSML.PauseFor(0.5);
            teachModel += "Can you say Vowels?";
            teachModel += SSML.PauseFor(0.5);
            teachModel += "Can you say Consonants?";
            teachModel += SSML.PauseFor(0.5);
            teachModel += "Vowels are " + SSML.SayExtraSlow("a") + SSML.SayExtraSlow("e") + SSML.SayExtraSlow("i") + 
                          SSML.SayExtraSlow("o") + SSML.SayExtraSlow("u");
            teachModel += " and sometimes y.";
            teachModel += "Right now we are going to work with the vowel " + vowel;
            teachModel += SSML.PauseFor(1.5);
            teachModel += " A short " + vowel + " makes the sound " + SSML.SayExtraSlow(SSML.Phoneme(vowelSound)) + ".";
            teachModel += SSML.PauseFor(1.0);
            teachModel += " Are your ready to learn some words with " + vowel;

            return teachModel;

        }

        public string TeachTheWord(WordAttributes wordAttributes)
        {
            Log.INFO("ShortVowels", "TeachTheWord", "WORD: " + wordAttributes.Word);

            string[] decodedWord = wordAttributes.Word.Select(x => x.ToString()).ToArray();
            string vowelSound = wordAttributes.VowelPhoneme;
            string teachModel = "";
            teachModel += "The word is spelled ";
            foreach (string sound in decodedWord)
            {
                teachModel += SSML.PauseFor(0.2) + SSML.SayExtraSlow(sound) + SSML.PauseFor(0.2);
            }
            teachModel += SSML.PauseFor(1.2) + SSML.SayExtraSlow(SSML.Phoneme(decodedWord[0])) + SSML.PauseFor(0.2);
            teachModel += SSML.SayExtraSlow(SSML.Phoneme(vowelSound)) + SSML.PauseFor(0.2) + 
                          SSML.SayExtraSlow(SSML.Phoneme(decodedWord[2]));
            teachModel += SSML.PauseFor(1.0);
            teachModel += SSML.SayExtraSlow(wordAttributes.Word);
            teachModel += SSML.PauseFor(0.5);
            teachModel += "Now you try. Say the word ";
            
            return teachModel;
        }
    }
}
