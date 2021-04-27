using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;

namespace Infrastructure.GlobalConstants
{

    public class SSML
    {
        public static string Excited(string word, string intensity = "medium")
        {
            return "<amazon:emotion name=\"excited\" intensity=\"" + intensity + "\">" + word + "</amazon:emotion>";
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

        public static readonly Dictionary<string, string> cdPhoneme = new Dictionary<string, string>
        {
         // sound | IPA
            {"ch" , "tʃ" },
            {"ck" , "k" },
            {"ll" , "ɫ"},
            {"sh" , "ʃ" },
            {"th" , "θ" },
            {"wh" , "hw" }
        };

        public static readonly Dictionary<string, string> cbPhoneme = new Dictionary<string, string>
        {
         // sound | IPA
            {"bl" , "bɫ" },
            {"br" , "bɹ" },
            {"cl" , "kɫ"},
            {"cr" , "kɹ" },
            {"dr" , "dɹ" },
            {"fl" , "fɫ" },
            {"fr" , "fɹ" },
            {"ft" , "ft" },
            {"gl" , "ɡɫ" },
            {"gr" , "ɡɹ" },
            {"lt" , "ɫt" },
            {"nd" , "nd" },
            {"ng" , "ŋ" },
            {"ngr", "ŋɡɹ" },
            {"nt" , "nt" },
            {"pl" , "pɫ" },
            {"pr" , "pɹ" },
            {"qu" , "kw" },
            {"rk" , "ɹk" },
            {"rm" , "ɹm" },
            {"rt" , "ɹt" },
            {"sc" , "sk" },
            {"sch", "sk" },
            {"scr", "skɹ" },
            {"shr" , "ʃɹ" },
            {"sk" , "sk" },
            {"sl" , "sɫ" },
            {"sm" , "sm" },
            {"sn" , "sn" },
            {"sp" , "sp" },
            {"spl", "spɫ" },
            {"spr", "spɹ" },
            {"st" , "st" },
            {"str" , "str" },
            {"sw" , "sw" },
            {"thr" , "θɹ" },
            {"tr" , "tɹ" },
            {"tw" , "tw" },
            {"wr" , "ɹ" }
        };
    }


    /// <summary>
    /// Determined by scope and sequence to tell FlashCardService 
    /// which lesson to teach or  assess word to read.
    /// </summary>
    public enum MODE
    {
        Teach,
        Assess
    }

    /// <summary>
    /// Internal state machine for keeping track of which intent
    /// should be executed.
    /// </summary>
    public enum STATE
    {
        Off,
        Introduction,
        Assess,
        MakingPurchase,
        Help
    }

    /// <summary>
    /// Will determine the method to which Alexa will
    /// teach the lesson with. Only used in Teach mode.
    /// </summary>
    public enum SKILL
    {
        LetterCaseRecognition,
        Tracking,
        SyllableCounting,
        LetterSounds,
        WordDecoding,
        WordEncoding,
        Phonics
    }


    /// <summary>Interface <i>Global Index</i>: All databases
    /// with a global secondary index must implement an index name,
    /// partition key, and sort key. These exact values can be found in
    /// DynamoDB.</summary>
    ///
    interface GlobalIndex
    {
        string Name { get; }
        string PartitionKey { get; }
        string SortKey { get; }
    }

    /// <summary>Class <c>Filter</c>: To provide additional filters based on desired
    /// vowel types, vowels, and syllable counts.</summary>
    ///
    public class Filter
    {
        public string VowelType { get; set; }
        public string Vowel { get; set; }
        public string Syllables { get; set; }
        public string FirstLetter { get; set; }

        public Filter(string vt, string v, string s, string fl)
        {           
            this.VowelType = vt;
            this.Vowel = v;
            this.Syllables = s;
            this.FirstLetter = fl;
        }

    }
}
