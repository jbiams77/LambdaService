using Infrastructure.Interfaces;
using Infrastructure.GlobalConstants;
using System.Linq;
using Infrastructure;
using Alexa.NET.Response;
using Alexa.NET;
using Infrastructure.Alexa;

namespace Infrastructure.Lessons
{
    public class LongVowels : ILesson
    {
        public string ProductName => "Long Vowels";
        public string InSkillPurchaseName => "long_vowels";
        public string LessonTypeName => "E";

        private string quickReply;
        public string QuickReply
        {
            set { quickReply = value; }
            get { return quickReply + SSML.PauseFor(1) ?? (quickReply = ""); }
        }

        public string Introduction(WordEntry wordAttributes)
        {
            string vowel = wordAttributes.Vowel;
            string vowelSound = wordAttributes.VowelPhoneme;

            string teachModel = "Not every letter in a word makes a sound. In the following words, the " + SSML.SpellOut("e") +
                                " is silent but " + SSML.Excited("bossy", "medium") + ". ";
            teachModel += SSML.PauseFor(1);
            teachModel += " This means the bossy " + SSML.SpellOut("e") + " makes the other vowel say its name.";
            teachModel += SSML.PauseFor(1);
            teachModel += "Right now we are going to work with the vowel " + vowel;
            teachModel += SSML.PauseFor(1.5);
            teachModel += " A long " + SSML.SpellOut(vowel) + " makes the sound " + SSML.SayExtraSlow(SSML.Phoneme(vowelSound)) + ".";
            teachModel += SSML.PauseFor(1.0);
            teachModel += " Are your ready to learn some words with " + vowel;

            return teachModel;

        }

        public string Dialogue(MODE mode, WordEntry wordAttributes)
        {
            switch (mode)
            {
                case MODE.Assess:
                    return AssessTheWord(wordAttributes);
                case MODE.Teach:
                    return TeachTheWord(wordAttributes);
                default:
                    return "ERROR";
            }
        }

        public string TeachTheWord(WordEntry wordAttributes)
        {
            string[] decodedWord = wordAttributes.Word.Select(x => x.ToString()).ToArray();
            string vowelSound = wordAttributes.VowelPhoneme;
            string teachModel = QuickReply;
            teachModel += SSML.PauseFor(1);
            teachModel += " The word is spelled ";
            teachModel += SSML.SpellOut(wordAttributes.Word);

            teachModel += SSML.PauseFor(1);
            teachModel += " Remember, the " + SSML.SpellOut("e") + " is silent and the " +
                          SSML.SpellOut(wordAttributes.Vowel) + " says its name. ";
            teachModel += SSML.PauseFor(1.0);
            teachModel += SSML.SayExtraSlow(wordAttributes.Word);
            teachModel += SSML.PauseFor(0.5);
            teachModel += "Now you try. Say the word ";

            return teachModel;
        }

        private string AssessTheWord(WordEntry wordAttributes)
        {
            string output = QuickReply + " Say the word";
            return output;
        }
    }
}
