using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moyca.Database;
using Amazon.DynamoDBv2.Model;
using Moyca.Database.GlobalConstants;

namespace Moyca.Database
{
    using DatabaseItem = Dictionary<string, AttributeValue>;

    public class LiveSessionDB : DatabaseClient
    {
        public static string TableName { get { return "live-session"; } }
        public static string PrimaryPartitionKey { get { return "UserID"; } }

        public List<string> wordsToRead;
        
        private string userId;

        public MODE TeachMode { get; set; }
        public STATE State { get; set; }
        public SKILL Skill { get; set; }
        public int CurrentSchedule { get; set; }


        public LiveSessionDB(string userId) : base(LiveSessionDB.TableName, LiveSessionDB.PrimaryPartitionKey)
        {
            this.userId = userId;
        }

        public async Task UpdateLiveSession()
        {
            AttributeValue currentSchedule = new AttributeValue();
            currentSchedule.N = this.CurrentSchedule.ToString();

            var updateRequest = new UpdateItemRequest
            {
                TableName = LiveSessionDB.TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { LiveSessionDB.PrimaryPartitionKey, new AttributeValue(this.userId) }
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":wordsToRead", new AttributeValue(this.wordsToRead) },
                    { ":teachMode", new AttributeValue(this.TeachMode.ToString())},
                    { ":skill", new AttributeValue(this.Skill.ToString()) },
                    { ":state", new AttributeValue(this.State.ToString()) },
                    { ":curSched", currentSchedule }
                },
                UpdateExpression = "SET WordsToRead = :wordsToRead,  TeachMode = :teachMode, Skill = :skill, " +
                                   "CurrentState = :state, CurrentSchedule = :curSched"
            };

            await SetItemsAttributeWithRequest(updateRequest);
        }

        public async Task GetDataFromLiveSession()
        {
            DatabaseItem items = await GetItemWithString(this.userId);

            items.TryGetValue("WordsToRead", out AttributeValue words);
            this.wordsToRead = words.SS;

            items.TryGetValue("TeachMode", out AttributeValue mode);
            this.TeachMode = (MODE)Enum.Parse(typeof(MODE), mode.S);

            items.TryGetValue("CurrentState", out AttributeValue state);
            this.State = (STATE)Enum.Parse(typeof(STATE), state.S);

            items.TryGetValue("Skill", out AttributeValue skill);
            this.Skill = (SKILL)Enum.Parse(typeof(SKILL), skill.S);

            items.TryGetValue("CurrentSchedule", out AttributeValue curSched);
            this.CurrentSchedule = int.Parse(curSched.N);

        }

        public bool Remove(string currentWord)
        {
            wordsToRead.Remove(currentWord);
            return (wordsToRead.Count == 0) ? true : false;            
        }

        public string GetCurrentWord()
        {
            if (wordsToRead[0] != null)
            {
                return wordsToRead[0];
            }

            return null;
        }

        public string GetWordsRemaining()
        {
            return wordsToRead.Count.ToString();
        }


    }
}
