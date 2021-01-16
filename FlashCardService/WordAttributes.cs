using System;
using System.Collections.Generic;
using System.Text;
using Amazon.DynamoDBv2.Model;
using AWSInfrastructure.DynamoDB;
using System.Threading.Tasks;
using AWSInfrastructure.Logger;

namespace FlashCardService
{
    using DatabaseItem = Dictionary<string, AttributeValue>;

    public class WordAttributes
    {
        public string ConsonantBlend { get; set;}
        public string ConsonantDigraph { get; set;}
        public string Decoded { get; set;}
        public string DecodedPhoneme { get; set;}
        public string Phoneme { get; set;}
        public string Sentence { get; set;}
        public string Syllables { get; set;}
        public string Vowel { get; set; }
        public string VowelBlend { get; set; }
        public string VowelPhoneme { get; set; }
        public string VowelType { get; set; }
        public string WordFamily { get; set; }
        public string Word { get; set; }

        DictionaryDB dictionaryDB;

        private WordAttributes() { }

        async public static Task<WordAttributes> GetWordAttributes(string word, MoycaLogger logger)
        {
            var wordAttributes = new WordAttributes();
            await wordAttributes.GetAttributes(word, logger);
            return wordAttributes;
        }
        private async Task GetAttributes(string word, MoycaLogger logger)
        {
            dictionaryDB = new DictionaryDB(logger);
            DatabaseItem items = await dictionaryDB.GetWordAttributesFromDictionary(word);

            if (items.TryGetValue("ConsonantBlend", out AttributeValue cb))
            {
                this.ConsonantBlend = cb.S;
            }

            if (items.TryGetValue("ConsonantDigraph", out AttributeValue cd))
            {
                this.ConsonantDigraph = cd.S;
            }

            if (items.TryGetValue("Decoded", out AttributeValue d))
            {
                this.Decoded = d.S;
            }

            if (items.TryGetValue("DecodedPhoneme", out AttributeValue dp))
            {
                this.DecodedPhoneme = dp.S;
            }

            if (items.TryGetValue("Sentence", out AttributeValue se))
            {
                this.Sentence = se.S;
            }

            if (items.TryGetValue("Syllables", out AttributeValue sy))
            {
                this.Syllables = sy.S;
            }

            if (items.TryGetValue("Vowel", out AttributeValue v))
            {
                this.Vowel = v.S;
            }

            if (items.TryGetValue("VowelBlend", out AttributeValue vb))
            {
                this.VowelBlend = vb.S;
            }

            if (items.TryGetValue("VowelPhoneme", out AttributeValue vp))
            {
                this.VowelPhoneme = vp.S;
            }

            if (items.TryGetValue("VowelType", out AttributeValue vt))
            {
                this.VowelType = vt.S;
            }


            if (items.TryGetValue("WordFamily", out AttributeValue wf))
            {
                this.WordFamily = wf.S;
            }

            if (items.TryGetValue("Word", out AttributeValue w))
            {
                this.Word = w.S;
            }

        }





    }


}
