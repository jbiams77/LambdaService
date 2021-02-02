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
        
        public static SkillResponse Introduction(LiveSessionDB liveSession, WordAttributes wordAttributes)
        {
            Function.log.INFO("TeachMode", "Introduction", "Provided for schedule" + liveSession.CurrentSchedule);

            Function.log.DEBUG("TeachMode", "Introduction", "Lesson Introduction: " + liveSession.Lesson.ToString());

            SkillResponse skillResponse; 

            switch (liveSession.Lesson)
            {
                case LESSON.WordFamilies:
                    skillResponse = TeachingPrompts.WordFamilyIntroduction(wordAttributes);
                    break;
                case LESSON.CVC:
                    skillResponse = TeachingPrompts.CVCWordIntroduction(wordAttributes);
                    break;
                case LESSON.ConsonantDigraph:
                    skillResponse = TeachingPrompts.CDIntroduction(wordAttributes);
                    break;
                case LESSON.ConsonantBlend:
                    skillResponse = TeachingPrompts.CBIntroduction(wordAttributes);
                    break;
                default: 
                    skillResponse = AlexaResponse.Introduction("Hello Moycan! Are you ready to begin learning?", "You can say yes to continue or no to stop");
                    break;
            }

            return skillResponse;
        }

        public static SkillResponse TeachTheWord(string beggining, LiveSessionDB liveSession, WordAttributes wordAttributes)
        {
            Function.log.INFO("TeachMode", "TeachTheWord", "WORD: " + wordAttributes.Word + " LESSON: " + liveSession.Lesson);

            string teachingPrompts = beggining + " ";

            Function.log.DEBUG("TeachMode", "TeachTheWord", "Lesson to Teach: " + liveSession.Lesson.ToString());

            switch (liveSession.Lesson)
            {
                case LESSON.WordFamilies:
                    teachingPrompts += TeachingPrompts.WordFamilyTeachTheWord(wordAttributes);
                    break;
                case LESSON.CVC:
                    teachingPrompts += TeachingPrompts.CVCTeachTheWord(wordAttributes);
                    break;
                case LESSON.ConsonantDigraph:
                    teachingPrompts += TeachingPrompts.CDTeachTheWord(wordAttributes);
                    break;
                case LESSON.ConsonantBlend:
                    teachingPrompts += TeachingPrompts.CBTeachTheWord(wordAttributes);
                    break;
                default:
                    teachingPrompts = " ERROR ";
                    break;
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

        public static string WordFamilyTeachTheWord(WordAttributes wordAttributes)
        {
            string[] decodedWord = wordAttributes.Word.Select(x => x.ToString()).ToArray();
            string wordFamily = wordAttributes.WordFamily;
            string teachModel = " This word is spelled ";
            foreach (string sound in decodedWord)
            {
                teachModel += PauseFor(0.2) + SayExtraSlow(sound) + PauseFor(0.2);
            }
            teachModel += PauseFor(.5);
            teachModel += "The sounds are ";
            teachModel += PauseFor(0.2) + SayExtraSlow(Phoneme(decodedWord[0])) + PauseFor(0.2);
            teachModel += PauseFor(0.2) + SayExtraSlow(wordFamily) + PauseFor(1.0);

            teachModel += SayExtraSlow(wordAttributes.Word);
            teachModel += PauseFor(0.5);
            teachModel += "Now you try. Say the word. ";

            Function.log.DEBUG("TeachingPrompts", "WordFamilyTeachTheWord", "Alexa Says: " + teachModel);

            return teachModel;
        }

        public static string CVCTeachTheWord(WordAttributes wordAttributes)
        {
            string[] decodedWord = wordAttributes.Word.Select(x => x.ToString()).ToArray();
            string vowelSound = wordAttributes.VowelPhoneme;
            string teachModel = "";
            teachModel += "The word is spelled ";
            foreach (string sound in decodedWord)
            {
                teachModel += PauseFor(0.2) + SayExtraSlow(sound) + PauseFor(0.2);
            }
            teachModel += PauseFor(1.2) + SayExtraSlow(Phoneme(decodedWord[0])) + PauseFor(0.2);
            teachModel += SayExtraSlow(Phoneme(vowelSound)) + PauseFor(0.2) + SayExtraSlow(Phoneme(decodedWord[2]));
            teachModel += PauseFor(1.0);
            teachModel += SayExtraSlow(wordAttributes.Word);
            teachModel += PauseFor(0.5);
            teachModel += "Now you try. Say the word ";

            Function.log.DEBUG("TeachingPrompts", "WordFamilyTeachTheWord", "Alexa Says: " + teachModel);

            return teachModel;
        }

        public static string CDTeachTheWord(WordAttributes wordAttributes)
        {
            string[] decodedWord = wordAttributes.Word.Select(x => x.ToString()).ToArray();
            string vowelSound = wordAttributes.VowelPhoneme;
            string teachModel = "";
            teachModel += "The word is spelled ";
            foreach (string sound in decodedWord)
            {
                teachModel += PauseFor(0.2) + SayExtraSlow(sound) + PauseFor(0.2);
            }
            teachModel += PauseFor(1.2) + SayExtraSlow(Phoneme(decodedWord[0])) + PauseFor(0.2);
            teachModel += SayExtraSlow(Phoneme(vowelSound)) + PauseFor(0.2) + SayExtraSlow(Phoneme(decodedWord[2]));
            teachModel += PauseFor(1.0);
            teachModel += SayExtraSlow(wordAttributes.Word);
            teachModel += PauseFor(0.5);
            teachModel += "Now you try. Say the word ";

            Function.log.DEBUG("TeachingPrompts", "WordFamilyTeachTheWord", "Alexa Says: " + teachModel);

            return teachModel;
        }

        public static string SigthWordsIntroduction(WordAttributes wordAttributes)
        {
            string teachModel = "There are words used over, and over, and over, and over, and ";
            teachModel += PauseFor(.5);
            teachModel += " Well " + PauseFor(.5) + " you get the point. These are called sight words. ";
            teachModel += " It is helpful to just memorize them by sight. To see them and know what they say.";
            teachModel += " Are you ready to start? ";

            Function.log.DEBUG("TeachingPrompts", "SigthWordsIntroduction", "Alexa Says: " + teachModel);

            return teachModel;
        }

        public static SkillResponse CDIntroduction(WordAttributes wordAttributes)
        {
            string consonantDigraph = wordAttributes.ConsonantDigraph;
            string[] cdLetters = consonantDigraph.Split("");
            string teachModel = "When two consonants work together to make one sound, this is called a digraph.";
            teachModel += PauseFor(.5);
            teachModel += "Can you say digraph?";
            teachModel += PauseFor(1.5);
            teachModel += "The digraph is made up of these two letters:";
            teachModel += cdLetters[0] + " and a " + cdLetters[1] + ".";
            cdPhoneme.TryGetValue(consonantDigraph, out string cdp);
            teachModel += " The sound they make is " + SayExtraSlow(Phoneme(cdp));
            teachModel += " Are you ready to begin?";

            Function.log.DEBUG("TeachingPrompts", "SigthWordsIntroduction", "Alexa Says: " + teachModel);

            return AlexaResponse.Introduction(teachModel, "You can say yes to continue or no to stop");
        }

        public static SkillResponse CBIntroduction(WordAttributes wordAttributes)
        {
            string consonantBlend = wordAttributes.ConsonantDigraph;
            string[] cBLetters = consonantBlend.Split("");
            string teachModel = "When consonants are stuck together, we call that a consonant blend.";
            teachModel += PauseFor(.5);
            teachModel += "The letters still make their individual sounds.";
            teachModel += PauseFor(1.5);
            teachModel += "This blend is made up of these two letters:";
            teachModel += cBLetters[0] + " and a " + cBLetters[1] + ".";
            teachModel += " The sound they make is " + SayExtraSlow(consonantBlend);
            teachModel += " Are you ready to begin?";

            Function.log.DEBUG("TeachingPrompts", "SigthWordsIntroduction", "Alexa Says: " + teachModel);

            return AlexaResponse.Introduction(teachModel, "You can say yes to continue or no to stop");
        }

        public static SkillResponse WordFamilyIntroduction(WordAttributes wordAttributes)
        {
            string wf = wordAttributes.WordFamily;

            string teachModel = CommonPhrases.Greeting + " my Moycan! We are working with word families. ";
            teachModel += PauseFor(0.5);

            teachModel +=  "A word family is a group of words that are related " +
                                "because they have a common spelling or sound. Word families " +
                                "often rhyme or end the same.";
            teachModel += PauseFor(1.5);
            teachModel += " Lets begin with the " + wf + ", word family. ";
            teachModel += " Remember, all of these words will end with " + wf + ".";
            teachModel += " Are you ready to begin?";

            Function.log.DEBUG("TeachingPrompts", "WordFamilyIntroduction", "Alexa Says: " + teachModel);

            return AlexaResponse.GetResponse(wordAttributes.WordFamily, teachModel, "You can say yes to continue or no to stop");
        }

        public static SkillResponse CVCWordIntroduction(WordAttributes wordAttributes)
        {
            string teachModel = "In the alphabet, there are two types of letters.";
            string vowel = wordAttributes.Vowel;
            string vowelSound = wordAttributes.VowelPhoneme;
            teachModel += PauseFor(0.5);
            teachModel += "Vowels and Consonants.";
            teachModel += PauseFor(0.5);
            teachModel += "Can you say Vowels?";
            teachModel += PauseFor(0.5);
            teachModel += "Can you say Consonants?";
            teachModel += PauseFor(0.5);
            teachModel += "Vowels are " + SayExtraSlow("a") + SayExtraSlow("e") + SayExtraSlow("i") + SayExtraSlow("o") + SayExtraSlow("u");
            teachModel += " and sometimes y.";
            teachModel += "Right now we are going to work with the vowel " + vowel;
            teachModel += PauseFor(1.5);
            teachModel += " A short " + vowel + " makes the sound " + SayExtraSlow(Phoneme(vowelSound)) + ".";
            teachModel += PauseFor(1.0);
            teachModel += " Are your ready to learn some words with " + vowel;

            Function.log.DEBUG("TeachingPrompts", "CVCWordIntroduction", "Alexa Says: " + teachModel);
            
            return AlexaResponse.Introduction(teachModel, "You can say yes to continue or no to stop");
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


        public static string SpellOut(string word)
        {
            return @"<say-as interpret-as=""spell-out"">" + word + @"</say-as>";
        }

        private static readonly Dictionary<string, string> cdPhoneme = new Dictionary<string, string>
        {
         // sound | IPA
            {"ch" , "tʃ" },
            {"ck" , "k" },
            {"ll" , "ɫ"},
            {"sh" , "ʃ" },
            {"th" , "θ" },
            {"wh" , "hw" }
        };
    }

}
