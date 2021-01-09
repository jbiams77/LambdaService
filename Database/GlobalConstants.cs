using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;

namespace Moyca.Database.GlobalConstants
{
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
        FirstWord,
        Assess        
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

        public Filter(string vt, string v, string s)
        {
            this.VowelType = vt;
            this.Vowel = v;
            this.Syllables = s;
        }
    }
}
