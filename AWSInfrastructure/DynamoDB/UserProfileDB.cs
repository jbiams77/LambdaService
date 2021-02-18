﻿using System;
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

        public int schedule;
        public MODE TeachMode;

        // This is a temporary fix to prevent user schedule from going out of bounds
        public readonly int MIN_SCHEDULE_INDEX = 1000;
        public readonly int MAX_SCHEDULE_INDEX = 1060;

        // Keys used to access database elements
        private readonly string SCHEDULE_KEY = "Schedule";
        private readonly string TEACH_MODE_KEY = "TeachMode";
        private MoycaLogger log;

        public UserProfileDB(string userId, MoycaLogger logger) : base(UserProfileDB.TableName, UserProfileDB.PrimaryPartitionKey, logger)
        {
            this.UserID = userId;
            this.log = logger;
            this.schedule = MIN_SCHEDULE_INDEX;
            this.TeachMode = MODE.Assess;
        }

        public async Task<int> GetUserSchedule()
        {
            DatabaseItem items = await GetEntryByKey(this.UserID);
            AttributeValue dbSchedule;
            if (items.TryGetValue(SCHEDULE_KEY, out dbSchedule))
            {
                log.INFO("UserProfileDB", "GetUserSchedule", "The user profile does not exist, start the user at schedule 1000");

                this.schedule = MIN_SCHEDULE_INDEX;
            }
            else
            {
                await CreateNewUser();
            }

            return schedule;
        }

        //public async Task IncrementUserSchedule(int completedSchedule)
        //{

        //    await GetUserSchedule();

        //    int scheduleToAdd = MIN_SCHEDULE_INDEX;
        //    if (schedule.Any())
        //    {
        //        scheduleToAdd = schedule[schedule.Count - 1] + 1;
        //    }

        //    if (scheduleToAdd > MAX_SCHEDULE_INDEX)
        //    {
        //        // User reached the end of allowed schedules
        //        scheduleToAdd = MIN_SCHEDULE_INDEX;
        //    }

        //    log.INFO("UserProfileDB", "RemoveCompletedScheduleFromUserProfile", "Removing Schedule: " + completedSchedule);
        //    if (await RemoveSchedule(completedSchedule))
        //    {
        //        log.INFO("UserProfileDB", "RemoveCompletedScheduleFromUserProfile", "Adding Schedule: " + scheduleToAdd.ToString());
        //        await AppendSchedule(scheduleToAdd);
        //    }
        //}

        public async Task DecrementUserSchedule(int completedSchedule)
        {
            var scheduleToAdd = completedSchedule - 1;

            if (scheduleToAdd >= 1000)
            {
                await InsertScheduleFront(scheduleToAdd);
            }
        }
        private async Task<bool> CreateNewUser()
        {
            Dictionary<string, AttributeValue> attributes = new Dictionary<string, AttributeValue>();
            // The primary key is required to determine which database entry to update
            attributes[PrimaryPartitionKey] = new AttributeValue { S = this.UserID };
            attributes[this.SCHEDULE_KEY] = new AttributeValue { N = this.schedule.ToString() };
            attributes[this.TEACH_MODE_KEY] = new AttributeValue { S = Enum.GetName(typeof(MODE), this.TeachMode) };

            var putRequest = new PutItemRequest
            {
                TableName = TableName,
                Item = attributes
            };

            return await base.PutAttributes(putRequest);
        }

        //private async Task<bool> RemoveSchedule(int scheduleToRemove)
        //{
        //    var scheduleIndex = schedule.IndexOf(scheduleToRemove);

        //    if (schedule.Remove(scheduleToRemove))
        //    {

        //        var updateRequest = new UpdateItemRequest
        //        {
        //            TableName = UserProfileDB.TableName,
        //            Key = new Dictionary<string, AttributeValue>
        //            {
        //                { UserProfileDB.PrimaryPartitionKey, new AttributeValue(this.UserID) }
        //            },
        //            UpdateExpression = "REMOVE Schedule[" + scheduleIndex + "]" //, SET Schedule = list_append(Schedule, :schedToAdd)"
        //        };

        //        return await SetItemsAttributeWithRequest(updateRequest);
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        private async Task<bool> AppendSchedule(int scheduleToAdd)
        {
            var newSchedule = new AttributeValue
            {
                N = scheduleToAdd.ToString()
            };
            var scheduleList = new List<AttributeValue> { newSchedule };

            var updateRequest = new UpdateItemRequest
            {
                TableName = UserProfileDB.TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { UserProfileDB.PrimaryPartitionKey, new AttributeValue(this.UserID) }
                },
                UpdateExpression = "SET #attrName = list_append(#attrName, :attrValue)",
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    { "#attrName", SCHEDULE_KEY },
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":attrValue", new AttributeValue()
                        {
                            L = scheduleList
                        }
                    }
                }
            };

            return await SetItemsAttributeWithRequest(updateRequest);
        }

        private async Task<bool> InsertScheduleFront(int scheduleToAdd)
        {
            var newSchedule = new AttributeValue
            {
                N = scheduleToAdd.ToString()
            };
            var scheduleList = new List<AttributeValue> { newSchedule };

            var updateRequest = new UpdateItemRequest
            {
                TableName = UserProfileDB.TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { UserProfileDB.PrimaryPartitionKey, new AttributeValue(this.UserID) }
                },
                UpdateExpression = "SET #attrName = list_append(:attrValue, #attrName)",
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    { "#attrName", SCHEDULE_KEY },
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":attrValue", new AttributeValue()
                        {
                            L = scheduleList
                        }
                    }
                }
            };

            return await SetItemsAttributeWithRequest(updateRequest);
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
