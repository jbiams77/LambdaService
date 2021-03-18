using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Infrastructure.GlobalConstants;
using FlashCardService.Interfaces;
using Infrastructure.Logger;
using Infrastructure.DynamoDB;
using FlashCardService.Factories;

namespace FlashCardService
{
    using DatabaseItem = Dictionary<string, AttributeValue>;


    public class UserProfileDB : DatabaseClient
    {
        public static string TableName { get { return "user-profile"; } }
        public static string PrimaryPartitionKey { get { return "UserID"; } }
        public string UserID { get; set; }
        public static int LastFreeIndex { get { return 1004; } }

        public int Schedule { get; set; }
        public MODE teachMode;
        public ILesson lesson;

        // This is a temporary fix to prevent user schedule from going out of bounds
        public readonly int MIN_SCHEDULE_INDEX = 1000;
        public readonly int MAX_SCHEDULE_INDEX = 1060;

        // Keys used to access database elements
        private readonly string SCHEDULE_KEY = "Schedule";
        private readonly string TEACH_MODE_KEY = "TeachMode";
        private MoycaLogger log;
        public ScopeAndSequenceDB scopeAndSequenceDB;

        public UserProfileDB(string userId, MoycaLogger logger) : base(UserProfileDB.TableName, UserProfileDB.PrimaryPartitionKey, logger)
        {
            this.UserID = userId;
            this.log = logger;
            this.Schedule = MIN_SCHEDULE_INDEX;
            this.teachMode = MODE.Assess;
            this.scopeAndSequenceDB = new ScopeAndSequenceDB(logger);
        }

        /// <summary>
        /// Retrieves user profile data from DynamoDB 'user-profile'
        /// </summary>
        public async Task GetUserProfileData()
        {
            DatabaseItem items = await GetEntryByKey(this.UserID);

            if (items.TryGetValue(SCHEDULE_KEY, out AttributeValue dbSchedule))
            {   
                if (items.TryGetValue(TEACH_MODE_KEY, out AttributeValue tM))
                {
                    this.teachMode = (MODE)Enum.Parse(typeof(MODE), tM.S);
                }
                else
                {
                    this.teachMode = MODE.Assess;
                }

                this.Schedule = int.Parse(dbSchedule.N);
                await this.scopeAndSequenceDB.GetSessionDataWithNumber(this.Schedule);
                log.INFO("UserProfileDB", "GetUserSchedule", "User profile exist, start the user at: " + this.Schedule);
            }
            else
            {   
                await CreateNewUser();
                log.INFO("UserProfileDB", "GetUserSchedule", "User profile does not exist, start the user at: " + this.Schedule);
            }
            this.lesson = LessonFactory.GetLesson(this.scopeAndSequenceDB.Lesson);
        }       

        public async Task DecrementUserProfileSchedule()
        {
            DatabaseItem items = await GetEntryByKey(this.UserID);

            if (items.TryGetValue(SCHEDULE_KEY, out AttributeValue dbSchedule))
            {
                this.Schedule = int.Parse(dbSchedule.N);
                this.Schedule -= 1;
                await UpdateSchedule();
            }
        }

        public async Task IncrementUserProfileSchedule()
        {
            DatabaseItem items = await GetEntryByKey(this.UserID);
            log.DEBUG("UserProfileDB", "IncrementUserProfileSchedule");

            if (items.TryGetValue(SCHEDULE_KEY, out AttributeValue dbSchedule))
            {
                this.Schedule = int.Parse(dbSchedule.N);
                this.Schedule += 1;
                log.DEBUG("UserProfileDB", "IncrementUserProfileSchedule", "Incremented Schedule to " + this.Schedule);
                await UpdateSchedule();
            }
        }

        public bool CurrentScheduleRequiresPurchase()
        {
            return (this.Schedule > this.lesson.FreeEndIndex);
        }

        public bool RequiresPurchase()
        {
            return (this.Schedule > this.lesson.FreeEndIndex);
        }

        public MODE GetMode()
        {
            return this.teachMode;
        }

        public async Task ChangeLesson(ILesson lesson)
        {
            // change user profile schedule to user requested index
           this.Schedule = lesson.FreeStartIndex;
           await UpdateSchedule();
        }

        private async Task<bool> CreateNewUser()
        {
            Dictionary<string, AttributeValue> attributes = new Dictionary<string, AttributeValue>();
            // The primary key is required to determine which database entry to update
            attributes[PrimaryPartitionKey] = new AttributeValue { S = this.UserID };
            attributes[this.SCHEDULE_KEY] = new AttributeValue { N = this.Schedule.ToString() };
            attributes[this.TEACH_MODE_KEY] = new AttributeValue { S = Enum.GetName(typeof(MODE), this.teachMode) };

            var putRequest = new PutItemRequest
            {
                TableName = TableName,
                Item = attributes
            };

            return await base.PutAttributes(putRequest);
        }


        private async Task<bool> UpdateSchedule()
        {
            Dictionary<string, AttributeValue> attributes = new Dictionary<string, AttributeValue>();
            // The primary key is required to determine which database entry to update
            attributes[PrimaryPartitionKey] = new AttributeValue { S = this.UserID };
            attributes[this.SCHEDULE_KEY] = new AttributeValue { N = this.Schedule.ToString() };

            var putRequest = new PutItemRequest
            {
                TableName = TableName,
                Item = attributes
            };

            return await PutAttributes(putRequest);
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
