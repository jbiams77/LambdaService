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
using Amazon.DynamoDBv2.Model;
using Moyca.Database;
using Moyca.Database.GlobalConstants;


namespace FlashCardService
{
    
    public class TeachMode
    {

        public static SkillResponse Introduction(LiveSessionDB liveSession, WordAttributes wordAttributes, bool displaySupported)
        {
            SsmlOutputSpeech teachingPrompts = new SsmlOutputSpeech();

            if (liveSession.Lesson == LESSON.WordFamilies)
            {
                teachingPrompts = TeachingPrompts.WordFamilyIntroduction(wordAttributes);
            }
            else if (liveSession.Lesson == LESSON.CVC)
            {
                teachingPrompts = TeachingPrompts.CVCWordIntroduction(wordAttributes);
            }
            return AlexaResponse.Introduction(teachingPrompts, "You can say yes to continue or no to stop", displaySupported);
        }

        public static SkillResponse TeachTheWord(LiveSessionDB liveSession, WordAttributes wordAttributes)
        {
            SsmlOutputSpeech teachingPrompts = new SsmlOutputSpeech();

            if (liveSession.Lesson == LESSON.WordFamilies)
            {
                teachingPrompts = TeachingPrompts.WordFamilyTeachTheWord(wordAttributes);
            }
            return AlexaResponse.GetResponse(wordAttributes.Word, teachingPrompts, "Please say " + wordAttributes.Word);
        }
    }


    public class TeachingPrompts
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

        public static SsmlOutputSpeech WordFamilyTeachTheWord(WordAttributes wordAttributes)
        {
            string[] decodedPhoneme = wordAttributes.DecodedPhoneme.Split('-');
            string[] decodedWord = wordAttributes.Decoded.Split('-');
            string wfPhoneme = RetrieveWordFamilyPhoneme(wordAttributes);

            string teachModel = StartTag;

            teachModel += "This word is spelled ";
            foreach (string sound in decodedWord)
            {
                teachModel += PauseFor(0.2) + SayExtraSlow(sound) + PauseFor(0.2);
            }
            teachModel += PauseFor(.5);
            teachModel += "The sounds are ";
            
            teachModel += PauseFor(0.2) + SayExtraSlow(Phoneme(decodedPhoneme[0])) + PauseFor(0.2);
            teachModel += PauseFor(0.2) + SayExtraSlow(Phoneme(wfPhoneme)) + PauseFor(0.2);
            teachModel += SayExtraSlow(wordAttributes.Word);
            teachModel += PauseFor(0.5);
            teachModel += "Now you try. Say the word ";
            teachModel += EndTag;
            return new SsmlOutputSpeech(teachModel);
        }

        public static SsmlOutputSpeech SigthWordsIntroduction()
        {
            string teachModel = StartTag;
            teachModel += "There are words used over, and over, and over, and over, and ";
            teachModel += PauseFor(.5);
            teachModel += " Well " + PauseFor(.5) + " you get the point. These are called sight words. ";
            teachModel += " It is helpful to just memorize them by sight. To see them and know what they say.";
            teachModel += " Are you ready to start? ";
            teachModel += EndTag;

            return new SsmlOutputSpeech(teachModel);
        }

        public static SsmlOutputSpeech WordFamilyIntroduction(WordAttributes wordAttributes)
        {
            string wfPhoneme = RetrieveWordFamilyPhoneme(wordAttributes);

            string teachModel = StartTag + "Hello my Moycan! We are working with word families. ";
            teachModel += PauseFor(0.5);

            teachModel +=  "A word family is a group of words that are related " +
                                "because they have a common spelling or sound. Word families " +
                                "often rhyme or end the same.";
            teachModel += PauseFor(1.5);
            teachModel += " Lets begin with the " + Phoneme(wfPhoneme) + ", word family. ";
            teachModel += " Remember, all of these words will end with " + Phoneme(wfPhoneme) + ".";
            teachModel += " Are you ready to begin?";

            teachModel += EndTag;

            // change this from "Say yes" to something more helpful
            return new SsmlOutputSpeech(teachModel);
        }

        public static SsmlOutputSpeech CVCWordIntroduction(WordAttributes wordAttributes)
        {
            string teachModel = StartTag + "Lets work on the " + Phoneme("æ") + " sound.";//wordAttributes.VowelPhoneme + " sound.";
            teachModel += PauseFor(1.5);
            teachModel += " Lets begin with the " + Phoneme(wordAttributes.WordFamily) + ", word family. ";
            teachModel += " Remember, all of these words will end with " + Phoneme(wordAttributes.WordFamily) + ".";
            teachModel += PauseFor(1.5);
            teachModel += " Listen for " + SayExtraSlow(Phoneme(wordAttributes.WordFamily)) + ", At the end of each word.";

            teachModel += PauseFor(1.0);
            teachModel += " Are your ready to give it a try?";
            teachModel += EndTag;

            // change this from "Say yes" to something more helpful
            return new SsmlOutputSpeech(teachModel);
        }


        public static SsmlOutputSpeech NextWordFamily(string familyPhoneme, List<string> wordFamilyList)
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

            return new SsmlOutputSpeech(teachModel);
        }

        public static SsmlOutputSpeech WordFamilyModel(string currentWord, string wordFamily, string endPhoneme)
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
            return  new SsmlOutputSpeech(teachModel);
        }

        public static SsmlOutputSpeech SightWordModel(string currentWord, string phoneme, string example)
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

            return new SsmlOutputSpeech(teachModel);
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

        public static string RetrieveWordFamilyPhoneme(WordAttributes wordAttributes)
        {
            string[] phoneme = wordAttributes.DecodedPhoneme.Split('-');
            return phoneme[1] + phoneme[2];
        }

        public static string SpellOut(string word)
        {
            return @"<say-as interpret-as=""spell-out"">" + word + @"</say-as>";
        }

        private static readonly Dictionary<string, string> PhonemesConsonants = new Dictionary<string, string>
        {
         // sound | IPA
            {"b" , "b" },
            {"d" , "d" },
            {"j" , "d͡ʒ"},
            {"f" , "f" },
            {"g" , "g" },
            {"h" , "h" },
            {"y" , "j" },
            {"c" , "k" },
            {"l" , "l" },
            {"m" , "m" },
            {"n" , "n" },
            {"ng", "ŋ" },
            {"p" , "p" },
            {"r" , "ɹ" },
            {"s" , "s" },
            {"sh", "ʃ" },
            {"t" , "t" },
            {"ch", "t͡ʃ"},
            {"th", "θ" },
            {"v" , "v" },
            {"w" , "w" },
            {"z" , "z" }
        };
    }

}
