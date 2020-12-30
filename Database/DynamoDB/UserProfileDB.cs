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



    }
}
