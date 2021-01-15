using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Moyca.Database.GlobalConstants;


namespace Moyca.Database
{
    using DatabaseItem = Dictionary<string, AttributeValue>;

    public class ScopeAndSequenceDB : DatabaseClient
    {
        public static string TableName { get { return "scope-and-sequence"; } }
        public static string PrimaryPartitionKey { get { return "Index"; } }

        public List<string> WordsToRead { get; set; }
        public string TeachMode { get; set; }
        public string Skill { get; set; }
        public LESSON Lesson { get; set; }

        public ScopeAndSequenceDB() : base(ScopeAndSequenceDB.TableName, ScopeAndSequenceDB.PrimaryPartitionKey)
        {
            this.WordsToRead = new List<string>();
        }

        public async Task GetSessionDataWithNumber(int orderNumber)
        {
            DatabaseItem item = await GetEntryByKey(orderNumber);

            if (item.TryGetValue("WordsToRead", out AttributeValue words))
            {
                this.WordsToRead = words.SS;
            }
            
            if (item.TryGetValue("Mode", out AttributeValue Mode))
            {
                this.TeachMode = Mode.S;
            }
            
            if (item.TryGetValue("Skill", out AttributeValue Skill))
            {
                this.Skill = Skill.S;
            }

            // Determine lesson plan 

            // WORD FAMILY
            if(item.TryGetValue("WF", out AttributeValue wf))
            {
                if (!wf.S.Equals("FALSE")) { Lesson = LESSON.WordFamilies; }
            }            
            // CONSONANT VOWEL CONSONANT
            else if (item.TryGetValue("CVC", out AttributeValue cvc))
            {
                if (cvc.S.Equals("TRUE")) { Lesson = LESSON.CVC; }
            }
            // CONSONANT DIGRAPH
            else if (item.TryGetValue("CD", out AttributeValue cd))
            {
                if (!cd.S.Equals("FALSE")) { Lesson = LESSON.ConsonantDigraph; }
            }
            // CONSONANT BLEND
            else if (item.TryGetValue("CB", out AttributeValue cb))
            {
                if (!cb.S.Equals("FALSE")) { Lesson = LESSON.ConsonantBlend; }
            }
            // E CONTROLLED VOWEL
            else if (item.TryGetValue("E", out AttributeValue e))
            {
                if (!e.S.Equals("FALSE")) { Lesson = LESSON.EControlledVowel; }
            }
            // R CONTROLLED VOWEL
            else if (item.TryGetValue("R", out AttributeValue r))
            {
                if (!r.S.Equals("FALSE")) { Lesson = LESSON.RControlledVowel; }
            }
            // L CONTROLLED VOWEL
            else if (item.TryGetValue("L", out AttributeValue l))
            {
                if (!l.S.Equals("FALSE")) { Lesson = LESSON.LControlledVowel; }
            }
            // VOWEL BLENDS
            else if (item.TryGetValue("VB", out AttributeValue vb))
            {
                if (!vb.S.Equals("FALSE")) { Lesson = LESSON.VowelBlends; }
            }
            // SIGHT WORDS
            else if (item.TryGetValue("SW", out AttributeValue sw))
            {
                if (sw.S.Equals("TRUE")) { Lesson = LESSON.SightWords; }
            }

        }       

        public async Task<Dictionary<string, string>> GetOrder(int number)
        {
            DatabaseItem items = await GetEntryByKey(number);

            Dictionary<string, string> wordOrder = new Dictionary<string, string>();

            foreach (KeyValuePair<string, AttributeValue> item in items)
            {
                if(item.Value.S != null && !item.Value.S.Equals("-"))
                {
                    wordOrder.Add(item.Key, item.Value.S);
                }
            }            

            return wordOrder;
        }

        public async Task PutItemBackWithWordsToRead(int index, List<string> wordsToRead)
        {
            AttributeValue pKey = new AttributeValue();
            pKey.N = index.ToString();
            await SetItemsAttribute(pKey, "WordsToRead", new AttributeValue(wordsToRead));
        }

    }
}
