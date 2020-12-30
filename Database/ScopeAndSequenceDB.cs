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

        public ScopeAndSequenceDB() : base(ScopeAndSequenceDB.TableName, ScopeAndSequenceDB.PrimaryPartitionKey)
        {
            
        }

        public async Task<List<string>> GetWordsToReadWithNumber(int orderNumber)
        {
            DatabaseItem item = await GetItemWithNumber(orderNumber);

            item.TryGetValue("WordsToRead", out AttributeValue val);

            return val.SS;
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
