using Infrastructure.Interfaces;
using Infrastructure.GlobalConstants;
using System.Linq;
using Infrastructure;
using Alexa.NET.Response;
using Alexa.NET;
using Infrastructure.Alexa;

namespace Infrastructure.Lessons
{
    public class ShortVowelsLesson : ILesson
    {
        public string ProductName => "Short Vowels";
        public string InSkillPurchaseName => "short_vowels";
        public string LessonTypeName => "CVC";
        public bool Display { get; set; }
        public ShortVowelsLesson(bool display)
        {
            Display = display;
        }

        public string HelpWithWord(WordEntry wordAttributes)
        {
            throw new System.NotImplementedException();
        }

        public string Introduction(WordEntry wordAttributes)
        {
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
            teachModel += "Vowels are " + SSML.SayExtraSlow("a") + SSML.PauseFor(0.5) + 
                          SSML.SayExtraSlow("e") + SSML.PauseFor(0.5) + SSML.SayExtraSlow("i") + SSML.PauseFor(0.5) +
                          SSML.SayExtraSlow("o") + SSML.PauseFor(0.5) + SSML.SayExtraSlow("u");
            teachModel += SSML.PauseFor(0.5);
            teachModel += " and sometimes y. ";
            teachModel += " Right now we are going to work with the vowel " + vowel;
            teachModel += SSML.PauseFor(1.5);
            teachModel += " A short " + SSML.SpellOut(vowel) + " makes the sound " + SSML.SayExtraSlow(SSML.Phoneme(vowelSound)) + ".";
            teachModel += SSML.PauseFor(1.0);
            teachModel += " Are your ready to learn some words with " + vowel;

            return teachModel;

        }

        public string TeachTheWord(WordEntry wordAttributes)
        {
            string[] decodedWord = wordAttributes.Word.Select(x => x.ToString()).ToArray();
            string vowelSound = wordAttributes.VowelPhoneme;
            string teachModel = "";
            teachModel += SSML.PauseFor(1);
            teachModel += " The word is spelled ";
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
