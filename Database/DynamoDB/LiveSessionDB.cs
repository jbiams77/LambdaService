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
    public class LiveSessionDB : DatabaseClient
    {
        public static string TableName { get { return "live-session"; } }
        public static string PrimaryPartitionKey { get { return "UserID"; } }

        public List<string> wordsToRead;
        public LiveSessionDB(string userId) : base(LiveSessionDB.TableName, LiveSessionDB.PrimaryPartitionKey)
        {

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

    }
}
