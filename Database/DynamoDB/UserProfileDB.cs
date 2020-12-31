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

    public class UserProfileDB : DatabaseClient
    {
        public static string TableName { get { return "user-profile"; } }
        public static string PrimaryPartitionKey { get { return "UserID"; } }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string QueueURL { get; set; }
        public string LastLogin { get; set; }

        public List<int> schedule;

        public UserProfileDB(string userId) : base (UserProfileDB.TableName, UserProfileDB.PrimaryPartitionKey)
        {
            this.UserID = userId;
            schedule = new List<int>();
        }

        public async Task<int> GetFirstScheduleNumber()
        {
            await GetUserSchedule();
            return schedule.ElementAt(0);
        }

        private async Task GetUserSchedule()
        {
            DatabaseItem items = await GetItemWithString(this.UserID);
            items.TryGetValue("Schedule", out AttributeValue val);            

            foreach(AttributeValue item in val.L)
            {
                schedule.Add(int.Parse(item.N));
            }
        }

        public async Task RemoveCompletedScheduleFromUserProfile(int completedSchedule)
        {
            await GetUserSchedule();

            schedule.Remove(completedSchedule);

            AttributeValue scheduleAttr = new AttributeValue();
            foreach(int num in this.schedule)
            {
                AttributeValue item = new AttributeValue();
                item.N = num.ToString();
                scheduleAttr.L.Add(item);
            }                

            var updateRequest = new UpdateItemRequest
            {
                TableName = UserProfileDB.TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { UserProfileDB.PrimaryPartitionKey, new AttributeValue(this.UserID) }
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":sched", scheduleAttr },
                },
                UpdateExpression = "SET Schedule = :sched"
            };

            await SetItemsAttributeWithRequest(updateRequest);
        }

        public async Task SetQueueUrl(string queueURL)
        {
            var updateRequest = new UpdateItemRequest
            {
                TableName = UserProfileDB.TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { UserProfileDB.PrimaryPartitionKey, new AttributeValue(this.UserID) }
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":queueURL", new AttributeValue(queueURL) },
                },
                UpdateExpression = "SET QueueURL = :queueURL"
            };

            await SetItemsAttributeWithRequest(updateRequest);
        }

    }
}
