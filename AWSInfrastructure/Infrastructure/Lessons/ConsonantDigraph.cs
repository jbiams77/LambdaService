using Infrastructure.GlobalConstants;
using Infrastructure.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;
using Alexa.NET.Response;
using Alexa.NET;
using Infrastructure.Interfaces;
using Infrastructure.Alexa;

namespace Infrastructure.Lessons
{
    public class ConsonantDigraph : ILesson
    {
        public string ProductName => "Consonant Digraph";
        public string InSkillPurchaseName => "digraph_blends";
        public string LessonTypeName => "CD";

        private string quickReply;
        public string QuickReply
        {
            set { quickReply = value; }
            get { return quickReply + SSML.PauseFor(1) ?? (quickReply = ""); }
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

        public string Introduction(WordEntry wordAttributes)
        {
            
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

        public string TeachTheWord(WordEntry wordAttributes)
        {           

            string[] decodedWord = wordAttributes.Word.Select(x => x.ToString()).ToArray();
            string vowelSound = wordAttributes.VowelPhoneme;
            string teachModel = QuickReply;
            teachModel += SSML.PauseFor(1);
            teachModel += " The word is spelled ";
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

        private string AssessTheWord(WordEntry wordAttributes)
        {
            string output = QuickReply + " Say the word";
            return output;
        }
    }
}
