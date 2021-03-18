using FlashCardService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.GlobalConstants;
using Infrastructure.Logger;
using Infrastructure;
using Alexa.NET.Response;
using Alexa.NET;

namespace FlashCardService.Lessons
{
    public class WordFamilies : ILesson
    {
        public string ProductName => "Word Families";
        public string InSkillPurchaseName => "word_family";
        public string LessonTypeName => "WF";
        public int FreeStartIndex => 1000;
        public int FreeEndIndex => 1002;
        public int CostStartIndex => 1003;
        public int CostEndIndex => 1035;

        private string quickReply;
        public string QuickReply 
        {
            set { quickReply = value; }
            get { return quickReply + SSML.PauseFor(1) ?? (quickReply = ""); }             
        }

        public SkillResponse Introduction(WordAttributes wordAttributes)
        {
            LOGGER.log.INFO("WordFamilies", "Introduction", "WORD: " + wordAttributes.Word);

            string wf = wordAttributes.WordFamily;

            string teachModel = "Hello my Moycan! We are working with word families. ";
            teachModel += SSML.PauseFor(0.5);

            teachModel += "A word family is a group of words that are related " +
                                "because they have a common spelling or sound. Word families " +
                                "often rhyme or end the same.";
            teachModel += SSML.PauseFor(1.5);
            teachModel += " Lets begin with the " + wf + ", word family. ";
            teachModel += " Remember, all of these words will end with " + wf + ".";
            teachModel += " Are you ready to begin?";

            return AlexaResponse.Introduction(teachModel, " Please say yes to continue or no to quit");
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

        private SkillResponse TeachTheWord(WordAttributes wordAttributes)
        {
            LOGGER.log.INFO("WordFamilies", "TeachTheWord", "WORD: " + wordAttributes.Word);

            string[] decodedWord = wordAttributes.Word.Select(x => x.ToString()).ToArray();
            string wordFamily = wordAttributes.WordFamily;
            string teachModel = QuickReply;
            teachModel += SSML.PauseFor(1);
            teachModel += " This word is spelled ";
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

            return AlexaResponse.TeachFlashCard(wordAttributes.Word, teachModel);
        }

        private SkillResponse AssessTheWord(WordAttributes wordAttributes)
        {
            string output = QuickReply + " Say the word";
            return AlexaResponse.PresentFlashCard(wordAttributes.Word, output, CommonPhrases.TryAgain);
        }

    }
}
