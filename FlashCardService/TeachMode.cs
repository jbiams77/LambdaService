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
using AWSInfrastructure.DynamoDB;
using AWSInfrastructure.GlobalConstants;


namespace FlashCardService
{
    
    public class TeachMode
    {
        
        public static SkillResponse Introduction(LiveSessionDB liveSession, WordAttributes wordAttributes, bool displaySupported)
        {
            Function.log.INFO("TeachMode", "Introduction", "Provided for " + liveSession.CurrentSchedule);

            if (liveSession.Lesson == LESSON.WordFamilies)
            {
                Function.log.DEBUG("TeachMode", "Introduction", "WordFamilies");

                return TeachingPrompts.WordFamilyIntroduction(wordAttributes, displaySupported);
            }
            else if (liveSession.Lesson == LESSON.CVC)
            {
                Function.log.DEBUG("TeachMode", "Introduction", "CVC");

                return TeachingPrompts.CVCWordIntroduction(wordAttributes, displaySupported);
            }
            Function.log.DEBUG("TeachMode", "Introduction", "Default Selection");
            
            return AlexaResponse.Introduction("Hello Moycan! Are you ready to begin learning?", "You can say yes to continue or no to stop", displaySupported);
        }

        public static SkillResponse TeachTheWord(LiveSessionDB liveSession, WordAttributes wordAttributes, bool displaySupported)
        {
            Function.log.INFO("TeachMode", "TeachTheWord", "WORD: " + wordAttributes.Word + " LESSON: " + liveSession.Lesson);

            string teachingPrompts = "";

            if (liveSession.Lesson == LESSON.WordFamilies)
            {
                Function.log.DEBUG("TeachMode", "TeachTheWord", "WordFamilies");

                teachingPrompts = TeachingPrompts.WordFamilyTeachTheWord(wordAttributes);
            }

            Function.log.DEBUG("TeachMode", "TeachTheWord", "Default Selection");

            return AlexaResponse.GetResponse(wordAttributes.Word, teachingPrompts, "Please say " + wordAttributes.Word, displaySupported);
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

        public static string WordFamilyTeachTheWord(WordAttributes wordAttributes)
        {
            string[] decodedPhoneme = wordAttributes.DecodedPhoneme.Split('-');
            string[] decodedWord = wordAttributes.Decoded.Split('-');
            string wfPhoneme = RetrieveWordFamilyPhoneme(wordAttributes);

            
            string teachModel = "This word is spelled ";
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

            Function.log.DEBUG("TeachingPrompts", "WordFamilyTeachTheWord", "Alexa Says: " + teachModel);

            return teachModel;
        }

        public static string SigthWordsIntroduction(bool displaySupported)
        {
            string teachModel = "There are words used over, and over, and over, and over, and ";
            teachModel += PauseFor(.5);
            teachModel += " Well " + PauseFor(.5) + " you get the point. These are called sight words. ";
            teachModel += " It is helpful to just memorize them by sight. To see them and know what they say.";
            teachModel += " Are you ready to start? ";

            Function.log.DEBUG("TeachingPrompts", "SigthWordsIntroduction", "Alexa Says: " + teachModel);

            return teachModel;
        }

        public static SkillResponse WordFamilyIntroduction(WordAttributes wordAttributes, bool displaySupported)
        {
            string wfPhoneme = RetrieveWordFamilyPhoneme(wordAttributes);

            string teachModel = "Hello my Moycan! We are working with word families. ";
            teachModel += PauseFor(0.5);

            teachModel +=  "A word family is a group of words that are related " +
                                "because they have a common spelling or sound. Word families " +
                                "often rhyme or end the same.";
            teachModel += PauseFor(1.5);
            teachModel += " Lets begin with the " + Phoneme(wfPhoneme) + ", word family. ";
            teachModel += " Remember, all of these words will end with " + Phoneme(wfPhoneme) + ".";
            teachModel += " Are you ready to begin?";

            Function.log.DEBUG("TeachingPrompts", "WordFamilyIntroduction", "Alexa Says: " + teachModel);

            return AlexaResponse.GetResponse(wordAttributes.WordFamily, teachModel, "You can say yes to continue or no to stop", displaySupported);
        }

        public static SkillResponse CVCWordIntroduction(WordAttributes wordAttributes, bool displaySupported)
        {
            string teachModel = "Lets work on the " + Phoneme("æ") + " sound.";//wordAttributes.VowelPhoneme + " sound.";
            teachModel += PauseFor(1.5);
            teachModel += " Lets begin with the " + Phoneme(wordAttributes.WordFamily) + ", word family. ";
            teachModel += " Remember, all of these words will end with " + Phoneme(wordAttributes.WordFamily) + ".";
            teachModel += PauseFor(1.5);
            teachModel += " Listen for " + SayExtraSlow(Phoneme(wordAttributes.WordFamily)) + ", At the end of each word.";

            teachModel += PauseFor(1.0);
            teachModel += " Are your ready to give it a try?";

            Function.log.DEBUG("TeachingPrompts", "CVCWordIntroduction", "Alexa Says: " + teachModel);

            // change this from "Say yes" to something more helpful
            return AlexaResponse.Introduction(teachModel, "You can say yes to continue or no to stop", displaySupported);
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
