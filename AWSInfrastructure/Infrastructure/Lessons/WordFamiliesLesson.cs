using Infrastructure.Interfaces;
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
using Infrastructure.Alexa;

namespace Infrastructure.Lessons
{
    public class WordFamiliesLesson : ILesson
    {
        public string ProductName => "Word Families";
        public string InSkillPurchaseName => "word_family";
        public string LessonTypeName => "WF";
        public bool Display { get; set; }

        public WordFamiliesLesson(bool display)
        {
            Display = display;
        }

        public string Introduction(WordEntry wordAttributes)
        {
            string wf = wordAttributes.WordFamily;

            string teachModel = "We are working with word families. ";
            teachModel += " Lets begin with the " + wf + ", word family. ";
            teachModel += " Remember, all of these words will end with " + wf + ".";
            teachModel += " Please say the word. ";
            
            if (!Display)
            {
                teachModel += SSML.SpellOut(wordAttributes.Word);
            }

            return teachModel;
        }

        public string TeachTheWord(WordEntry wordAttributes)
        {
            string teachModel = "";
            teachModel += SSML.PauseFor(1);
            teachModel += " This word is spelled ";
            teachModel += SSML.SpellOut(wordAttributes.Word);
            teachModel += SSML.PauseFor(.5);
            teachModel += SSML.SayExtraSlow(wordAttributes.Word);
            teachModel += SSML.PauseFor(0.5);
            teachModel += "Now you try. Say the word. ";

            return teachModel;
        }

        public string HelpWithWord(WordEntry wordAttributes)
        {            
            string help = "Ok, let me give you a hint. A word family is a group of words that are related " +
                    "because they have a common spelling or sound. Word families " +
                    "often rhyme or end the same.";
            help += SSML.PauseFor(1);
            help += " This word is in the " + SSML.PauseFor(0.5) + SSML.SayExtraSlow(wordAttributes.WordFamily) + SSML.PauseFor(0.5) + " family. ";
            help += SSML.PauseFor(0.5) + " Now try and say the word. ";

            if (!Display)
            {
                help += SSML.SpellOut(wordAttributes.Word);
            }

            return help;
        }

    }
}
