using Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.GlobalConstants;
using Infrastructure.Logger;

namespace Infrastructure.Lessons
{
    public class WordFamilies : ILesson
    {
        public string ProductName => "word families";
        public string InSkillPurchaseName => "word_family";
        public string LessonTypeName => "WF";
        public int FreeStartIndex => 1000;
        public int FreeEndIndex => 1002;
        public int CostStartIndex => 1003;
        public int CostEndIndex => 1035;
        public MoycaLogger Log { get; set; }

        public WordFamilies(MoycaLogger log)
        {
            Log = log;            
        }

        public string Introduction(WordAttributes wordAttributes)
        {
            Log.INFO("WordFamilies", "Introduction", "WORD: " + wordAttributes.Word);

            string wf = wordAttributes.WordFamily;

            string teachModel = " my Moycan! We are working with word families. ";
            teachModel += SSML.PauseFor(0.5);

            teachModel += "A word family is a group of words that are related " +
                                "because they have a common spelling or sound. Word families " +
                                "often rhyme or end the same.";
            teachModel += SSML.PauseFor(1.5);
            teachModel += " Lets begin with the " + wf + ", word family. ";
            teachModel += " Remember, all of these words will end with " + wf + ".";
            teachModel += " Are you ready to begin?";


            return teachModel;
        }

        public string TeachTheWord(WordAttributes wordAttributes)
        {
            Log.INFO("WordFamilies", "TeachTheWord", "WORD: " + wordAttributes.Word);

            string[] decodedWord = wordAttributes.Word.Select(x => x.ToString()).ToArray();
            string wordFamily = wordAttributes.WordFamily;
            string teachModel = " This word is spelled ";
            foreach (string sound in decodedWord)
            {
                teachModel += SSML.PauseFor(0.2) + SSML.SayExtraSlow(sound) + SSML.PauseFor(0.2);
            }
            teachModel += SSML.PauseFor(.5);
            teachModel += "The sounds are ";
            teachModel += SSML.PauseFor(0.2) + SSML.SayExtraSlow(SSML.Phoneme(decodedWord[0])) + SSML.PauseFor(0.2);
            teachModel += SSML.PauseFor(0.2) + SSML.SayExtraSlow(wordFamily) + SSML.PauseFor(1.0);

            teachModel += SSML.SayExtraSlow(wordAttributes.Word);
            teachModel += SSML.PauseFor(0.5);
            teachModel += "Now you try. Say the word. ";

            return teachModel;
        }
    }
}
