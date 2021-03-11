using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Infrastructure.GlobalConstants;
using Infrastructure.Logger;

namespace Infrastructure.DynamoDB
{
    using DatabaseItem = Dictionary<string, AttributeValue>;

    public class ScopeAndSequenceDB : DatabaseClient
    {
        public static string TableName { get { return "scope-and-sequence"; } }
        public static string PrimaryPartitionKey { get { return "Index"; } }        
        public List<string> WordsToRead { get; set; }
        public string TeachMode { get; set; }
        public string Skill { get; set; }
        public string ProductName { get; set; }
        public string Lesson { get; set; }

        private MoycaLogger log;

        public ScopeAndSequenceDB(MoycaLogger logger) : base(ScopeAndSequenceDB.TableName, ScopeAndSequenceDB.PrimaryPartitionKey, logger)
        {
            this.WordsToRead = new List<string>();
            this.log = logger;
        }

        /// <summary>
        /// Uses order (primary key) to retrieve the words to read and lesson type from scope and sequence Database.
        /// </summary>
        /// @param orderNumber - Integer number used as primary key for geting list of words from database. </param>        
        public async Task GetSessionDataWithNumber(int orderNumber)
        {
            log.INFO("ScopeAndSequenceDB", "GetSessionDataWithNumber", "Order number: " + orderNumber);

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

            if (item.TryGetValue("InSkillPurchase", out AttributeValue inSkillPurchase))
            {
                this.ProductName = inSkillPurchase.S;
            }

            // Determine lesson plan 
            if (item.TryGetValue("Lesson", out AttributeValue lessonType))
            {
                this.Lesson = lessonType.S;
            }            
            
            log.INFO("ScopeAndSequenceDB", "GetSessionDataWithNumber", "Lesson: " + this.Lesson.ToString());
        }

        public async Task<Dictionary<string, string>> GetOrder(int number)
        {
            log.INFO("ScopeAndSequenceDB", "GetOrder", "Order Number: " + number);

            DatabaseItem items = await GetEntryByKey(number);

            Dictionary<string, string> wordOrder = new Dictionary<string, string>();

            foreach (KeyValuePair<string, AttributeValue> item in items)
            {
                if(item.Value.S != null && !item.Value.S.Equals("-"))
                {
                    wordOrder.Add(item.Key, item.Value.S);
                }
            }
            log.INFO("ScopeAndSequenceDB", "GetOrder", "Word Order: " + wordOrder.ToString());
            return wordOrder;
        }

        public async Task PutItemBackWithWordsToRead(int index, List<string> wordsToRead)
        {
            log.INFO("ScopeAndSequenceDB", "PutItemBackWithWordsToRead", "Index: " + index);

            AttributeValue pKey = new AttributeValue();
            pKey.N = index.ToString();
            await SetItemsAttribute(pKey, "WordsToRead", new AttributeValue(wordsToRead));
        }

    }
}
