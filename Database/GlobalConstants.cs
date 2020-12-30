using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;

namespace Moyca.Database.GlobalConstants
{
    public enum MODE
    {
        Teach,
        Assess
    }

    public enum STATE
    {
        Introduction,
        WordFamilyModel,
        WordFamilyAssess,
        SightWordModel,
        SightWordAssess
    }

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

    interface GlobalIndex
    {
        string Name { get; }
        string PartitionKey { get; }
        string SortKey { get; }
    }

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
