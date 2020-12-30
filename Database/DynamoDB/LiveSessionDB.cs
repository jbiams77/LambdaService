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


        public LiveSessionDB(string userId) : base(LiveSessionDB.TableName, LiveSessionDB.PrimaryPartitionKey)
        {
            this.userId = userId;
        }

        public async Task UpdateLiveSession(string userId, List<string> wordsToRead, string teachMode, string skill, string state)
        {
            var updateRequest = new UpdateItemRequest
            {
                TableName = LiveSessionDB.TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { LiveSessionDB.PrimaryPartitionKey, new AttributeValue(userId) }
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":wordsToRead", new AttributeValue(wordsToRead) },
                    { ":teachMode", new AttributeValue(teachMode) },
                    { ":skill", new AttributeValue(skill) },
                    { ":state", new AttributeValue(state) },

                },
                UpdateExpression = "SET WordsToRead = :wordsToRead,  TeachMode = :teachMode, Skill = :skill, CurrentState = :state"
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
        }

        public string GetCurrentWord()
        {
            if (wordsToRead[0] != null)
            {
                return wordsToRead[0];
            }

            return null;
        }


    }
}
