using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using AWSInfrastructure.GlobalConstants;
using AWSInfrastructure.Logger;

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
        private MoycaLogger log;

        public UserProfileDB(string userId, MoycaLogger logger) : base (UserProfileDB.TableName, UserProfileDB.PrimaryPartitionKey, logger)
        {
            this.UserID = userId;
            this.log = logger;
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
                log.INFO("UserProfileDB", "GetUserSchedule", "The user profile didnt contain a schedule, copy the default schedule into this user's profile");                

                DatabaseItem defaultDb = await base.GetEntryByKey(DEFAULT_DB_KEY);
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
            log.INFO("UserProfileDB", "RemoveCompletedScheduleFromUserProfile", "Removing Schedule: " + completedSchedule);

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
            log.INFO("UserProfileDB", "SetQueueUrl", "Setting QueueURL: " + queueURL);

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
