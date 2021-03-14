using Infrastructure;
using Infrastructure.GlobalConstants;
using FlashCardService.Interfaces;
using Infrastructure.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexa.NET.Response;
using Alexa.NET;

namespace FlashCardService.Lessons
{
    public class ConsonantBlend : ILesson
    {
        public string ProductName => "Consonant Blends";
        public string InSkillPurchaseName => "digraph_blends";
        public string LessonTypeName => "CB";
        public int FreeStartIndex => 1110;
        public int FreeEndIndex => 1111;
        public int CostStartIndex => 1112;
        public int CostEndIndex => 1140;

        private string quickReply;
        public string QuickReply
        {
            set { quickReply = value; }
            get { return quickReply + SSML.PauseFor(1) ?? (quickReply = ""); }
        }

        public SkillResponse Introduction(WordAttributes wordAttributes)
        {
            LOGGER.log.INFO("ConsonantBlend", "Introduction", "WORD: " + wordAttributes.Word);

            string[] cBLetters = wordAttributes.ConsonantBlend.Select(x => x.ToString()).ToArray();
            string teachModel = "When consonants are stuck together and both make their sounds, we call that a consonant blend.";
            teachModel += SSML.PauseFor(.5);
            teachModel += "The letters still make their individual sounds.";
            teachModel += SSML.PauseFor(1.5);
            teachModel += "This blend is made up of these two letters:";
            teachModel += SSML.SayExtraSlow(cBLetters[0]) + " and a " + SSML.SayExtraSlow(cBLetters[1]) + ".";
            teachModel += SSML.PauseFor(1.5);
            if (SSML.cbPhoneme.TryGetValue(wordAttributes.ConsonantBlend, out string cbp))
            {
                teachModel += " The sound they make is " + SSML.PauseFor(.5) + SSML.SayExtraSlow(SSML.Phoneme(cbp));
            }
            teachModel += SSML.PauseFor(1.5);
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

        public SkillResponse TeachTheWord(WordAttributes wordAttributes)
        {
            LOGGER.log.INFO("ConsonantBlend", "TeachTheWord", "WORD: " + wordAttributes.Word);

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
    }
}
