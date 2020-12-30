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

        public List<string> wordsToRead;
        public string teachMode;
        public string skill;

        public ScopeAndSequenceDB() : base(ScopeAndSequenceDB.TableName, ScopeAndSequenceDB.PrimaryPartitionKey)
        {
            this.wordsToRead = new List<string>();
        }

        public async Task GetSessionDataWithNumber(int orderNumber)
        {
            DatabaseItem item = await GetItemWithNumber(orderNumber);

            item.TryGetValue("WordsToRead", out AttributeValue words);
            this.wordsToRead = words.SS;

            item.TryGetValue("Mode", out AttributeValue Mode);
            this.teachMode = Mode.S;

            item.TryGetValue("Skill", out AttributeValue Skill);
            this.skill = Skill.S;
        }       

        public async Task<Dictionary<string, string>> GetOrder(int number)
        {
            DatabaseItem items = await GetItemWithNumber(number);

            Dictionary<string, string> wordOrder = new Dictionary<string, string>();

            foreach (KeyValuePair<string, AttributeValue> item in items)
            {
                if(item.Value.S != null && !item.Value.S.Equals(""))
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
