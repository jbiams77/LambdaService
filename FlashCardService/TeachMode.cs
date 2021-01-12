using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Request.Type;
using Alexa.NET.Response.Converters;
using Alexa.NET.Response.Directive;
using Newtonsoft.Json;
using Alexa.NET;


namespace FlashCardService
{
    static class TeachMode
    {
        private static string StartTag { get { return "<speak>"; } }
        private static string EndTag { get { return "</speak>"; } }

 
        public static SkillResponse RichTextResponse()
        {
            return AlexaResponse.Say(new SsmlOutputSpeech
            {
                Ssml = StartTag + "Hello" + EndTag
            });
        }

        public static SkillResponse SigthWordsIntroduction()
        {
            string teachModel = StartTag;
            teachModel += "There are words used over, and over, and over, and over, and ";
            teachModel += PauseFor(.5);
            teachModel += " Well " + PauseFor(.5) + " you get the point. These are called sight words. ";
            teachModel += " It is helpful to just memorize them by sight. To see them and know what they say.";
            teachModel += " Are you ready to start? ";
            teachModel += EndTag;

            return AlexaResponse.SayWithReprompt(new SsmlOutputSpeech(teachModel), "Say yes");
        }

        public static SkillResponse WordFamilyIntroduction(string familyPhoneme, List<string> wordFamilyList)
        {
            string teachModel = StartTag + "A word family is a group of words that are related " +
                                "because they have a common spelling or sound. Word families " +
                                "often rhyme or end the same.";
            teachModel += PauseFor(1.5);
            teachModel += " Lets begin with the " + Phoneme(familyPhoneme) + ", word family. ";
            teachModel += " Remember, all of these words will end with " + Phoneme(familyPhoneme) + ".";
            teachModel += PauseFor(1.5);
            teachModel += " Listen for " + SayExtraSlow(Phoneme(familyPhoneme)) + ", At the end of each word.";
            teachModel += PauseFor(1.5);

            foreach (string word in wordFamilyList)
            {
                teachModel += PauseFor(1.0);
                teachModel += ", ";
                teachModel += SayExtraSlow(word);
            }

            teachModel += PauseFor(1.0);
            teachModel += " Are your ready to give it a try?";
            teachModel += EndTag;

            // change this from "Say yes" to something more helpful
            return AlexaResponse.SayWithReprompt(new SsmlOutputSpeech(teachModel), "Say yes");
        }

        public static SkillResponse NextWordFamily(string familyPhoneme, List<string> wordFamilyList)
        {
            string teachModel = StartTag;
            teachModel += " Next is the " + Phoneme(familyPhoneme) + ", word family. ";
            teachModel += " Remember, all of these words will end with " + Phoneme(familyPhoneme) + ".";
            teachModel += PauseFor(1.5);
            teachModel += " Listen for " + SayExtraSlow(Phoneme(familyPhoneme)) + ", At the end of each word.";
            teachModel += PauseFor(1.5);

            foreach (string word in wordFamilyList)
            {
                teachModel += PauseFor(1.0);
                teachModel += ", ";
                teachModel += SayExtraSlow(word);
            }

            teachModel += PauseFor(1.0);
            teachModel += " Are your ready to give it a try?";
            teachModel += EndTag;

            return AlexaResponse.SayWithReprompt(new SsmlOutputSpeech(teachModel), "Say yes");
        }

        public static SkillResponse WordFamilyModel(string currentWord, string wordFamily, string endPhoneme)
        {
            string teachModel = StartTag;
            string reprompt = "Say the word " + currentWord;
            string beggingSound = currentWord.Remove(currentWord.Length - wordFamily.Length, wordFamily.Length);

            teachModel += @"The word is " + SayExtraSlow(currentWord);
            teachModel += PauseFor(0.5);

            if (PhonemesConsonants.TryGetValue(beggingSound, out string sound))
            {
                teachModel += SayExtraSlow(Phoneme(sound));
            }

            teachModel += PauseFor(0.5);
            teachModel += Phoneme(endPhoneme);
            teachModel += PauseFor(0.5);
            teachModel += SayExtraSlow(currentWord);
            teachModel += PauseFor(0.5);
            teachModel += " Say the word with me. ";
            teachModel += PauseFor(0.5);
            teachModel += SayExtraSlow(currentWord);
            teachModel += PauseFor(0.5);
            teachModel += " Now its your turn. Say the word ";
            teachModel += SayExtraSlow(currentWord);
            teachModel += EndTag;
            return AlexaResponse.GetResponse(currentWord, new SsmlOutputSpeech(teachModel), reprompt);
        }

        public static SkillResponse SightWordModel(string currentWord, string phoneme, string example)
        {
            string teachModel = StartTag;
            string reprompt = "Say the word " + currentWord;
            string[] exampleWords = example.Split(' ');


            teachModel += @"This sight word is " + SayExtraSlow(Phoneme(phoneme));
            teachModel += PauseFor(0.5);
            teachModel += "As in ";
            teachModel += PauseFor(1);

            foreach (string word in exampleWords)
            {

                if (word.Equals(currentWord.ToLower()))
                {
                    teachModel += " " + PauseFor(.5) + " ";
                    teachModel += " " + SayExtraSlow(Phoneme(phoneme));
                    teachModel += PauseFor(.5) + " ";
                }
                else
                {
                    teachModel += " " + word + " ";
                }

            }

            teachModel += PauseFor(0.5);
            teachModel += Phoneme(phoneme);
            teachModel += PauseFor(0.5);
            teachModel += SayExtraSlow(Phoneme(phoneme));
            teachModel += PauseFor(0.5);
            teachModel += " Say the word with me. ";
            teachModel += PauseFor(0.5);
            teachModel += SayExtraSlow(Phoneme(phoneme));
            teachModel += PauseFor(0.5);
            teachModel += " Now its your turn. Say the word ";
            teachModel += SayExtraSlow(Phoneme(phoneme));
            teachModel += EndTag;

            return AlexaResponse.GetResponse(currentWord, new SsmlOutputSpeech(teachModel), reprompt);
        }

        public static string PauseFor(double delay)
        {
            return @"<break time=""" + delay.ToString() + @"s""/>";
        }

        public static string Phoneme(string phoneme)
        {
            return @"<phoneme alphabet=""ipa"" ph=""" + phoneme + @""">" + phoneme + "</phoneme>";

        }

        public static string SayExtraSlow(string word)
        {
            return @"<prosody rate=""x-slow"">" + word + @"</prosody>";
        }

    }

}
