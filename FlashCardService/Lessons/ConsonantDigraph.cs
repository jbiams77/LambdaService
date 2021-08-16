using Infrastructure.GlobalConstants;
using FlashCardService.Interfaces;
using Infrastructure.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;
using Alexa.NET.Response;
using Alexa.NET;

namespace FlashCardService.Lessons
{
    public class ConsonantDigraph : ILesson
    {
        public string ProductName => "Consonant Digraph";
        public string InSkillPurchaseName => "digraph_blends";
        public string LessonTypeName => "CD";
        public int FreeStartIndex => 1098;
        public int FreeEndIndex => 1099;
        public int CostStartIndex => 1100;
        public int CostEndIndex => 1109;

        private string quickReply;
        public string QuickReply
        {
            set { quickReply = value; }
            get { return quickReply + SSML.PauseFor(1) ?? (quickReply = ""); }
        }

        public SkillResponse Dialogue(MODE mode, WordAttributes wordAttributes)
        {
            switch (mode)
            {
                case MODE.Assess:
                    return AssessTheWord(wordAttributes);
                case MODE.Teach:
                    return TeachTheWord(wordAttributes);
                default:
                    return ResponseBuilder.Tell("ERROR");
            }
        }

        public SkillResponse Introduction(WordAttributes wordAttributes)
        {
            LOGGER.log.INFO("ConsonantDigraph", "Introduction", "WORD: " + wordAttributes.Word);

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

            return AlexaResponse.Introduction(teachModel, " Please say yes to continue or no to quit");
        }

        public SkillResponse TeachTheWord(WordAttributes wordAttributes)
        {
            LOGGER.log.INFO("ConsonantDigraph", "TeachTheWord", "WORD: " + wordAttributes.Word);

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

            return AlexaResponse.TeachFlashCard(wordAttributes.Word, teachModel); 
        }

        private SkillResponse AssessTheWord(WordAttributes wordAttributes)
        {
            string output = QuickReply + " Say the word";
            return AlexaResponse.PresentFlashCard(wordAttributes.Word, output, CommonPhrases.TryAgain);
        }

        public SkillResponse Introduction(WordAttributes wordAttributes, bool display)
        {
            throw new NotImplementedException();
        }
    }
}
