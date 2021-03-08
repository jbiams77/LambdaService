using Infrastructure.GlobalConstants;
using Infrastructure.Interfaces;
using Infrastructure.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Lessons
{
    public class ConsonantDigraph : ILesson
    {
        public string ProductName => "Consonant Digraph";
        public string InSkillPurchaseName => "digraph_blends";
        public string LessonTypeName => "CD";
        public int FreeStartIndex => 1098;
        public int FreeEndIndex => 1100;
        public int CostStartIndex => 1101;
        public int CostEndIndex => 1109;
        public MoycaLogger Log { get; set; }

        public ConsonantDigraph(MoycaLogger log)
        {
            Log = log;
        }

        public string Introduction(WordAttributes wordAttributes)
        {
            Log.INFO("ConsonantDigraph", "Introduction", "WORD: " + wordAttributes.Word);

            string[] cdLetters = wordAttributes.ConsonantDigraph.Select(x => x.ToString()).ToArray();
            string teachModel = "When two consonants work together to make one sound, this is called a digraph.";
            teachModel += SSML.PauseFor(.5);
            teachModel += "Can you say digraph?";
            teachModel += SSML.PauseFor(1.5);
            teachModel += "The digraph we are learning now is made up of these two letters:";
            teachModel += SSML.PauseFor(1);
            teachModel += SSML.SayExtraSlow(cdLetters[0]) + " and a " + SSML.SayExtraSlow(cdLetters[1]) + ".";
            teachModel += SSML.PauseFor(1.5);
            if (SSML.cdPhoneme.TryGetValue(wordAttributes.ConsonantDigraph, out string cdp))
            {
                teachModel += " The sound they make is " + SSML.PauseFor(.5) + SSML.SayExtraSlow(SSML.Phoneme(cdp));
            }
            teachModel += SSML.PauseFor(1.5);
            teachModel += " Are you ready to begin?";

            return teachModel;
        }

        public string TeachTheWord(WordAttributes wordAttributes)
        {
            Log.INFO("ConsonantDigraph", "TeachTheWord", "WORD: " + wordAttributes.Word);

            string[] decodedWord = wordAttributes.Word.Select(x => x.ToString()).ToArray();
            string vowelSound = wordAttributes.VowelPhoneme;
            string teachModel = "";
            teachModel += "The word is spelled ";
            foreach (string sound in decodedWord)
            {
                teachModel += SSML.PauseFor(0.2) + SSML.SayExtraSlow(sound) + SSML.PauseFor(0.2);
            }
            teachModel += SSML.PauseFor(1.0);
            teachModel += SSML.SayExtraSlow(wordAttributes.Word);
            teachModel += SSML.PauseFor(0.5);
            teachModel += "Now you try. Say the word ";

            return teachModel;
        }
    }
}
