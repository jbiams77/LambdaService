using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using AWSInfrastructure.GlobalConstants;

namespace AWSInfrastructure.DynamoDB
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

        // Keys used to access database elements
        private readonly string DEFAULT_DB_KEY = "default";
        private readonly string SCHEDULE_KEY = "Schedule";

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
            DatabaseItem items = await GetEntryByKey(this.UserID);
            AttributeValue dbSchedule;
            if (!items.TryGetValue(SCHEDULE_KEY, out dbSchedule))
            {
                // The user profile didnt contain a schedule, copy the default schedule into this user's profile
                DatabaseItem defaultDb = await GetEntryByKey(DEFAULT_DB_KEY);
                defaultDb.TryGetValue(SCHEDULE_KEY, out AttributeValue defaultSchedule);

                AttributeValue key = new AttributeValue{ S = this.UserID };
                await SetItemsAttribute(key, SCHEDULE_KEY, defaultSchedule);

                dbSchedule = defaultSchedule;
            }

            foreach (AttributeValue item in dbSchedule.L)
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
