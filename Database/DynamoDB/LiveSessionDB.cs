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

    /// <summary>Class <c>LiveSessionDB</c>: For accessing the live-session database.
    /// Used primarily by FlashCardService to populate live-session with session data
    /// to ensure lambda function follow servless design criteria.</summary>
    public class LiveSessionDB : DatabaseClient
    {
        private static string TableName { get { return "live-session"; } }
        private static string PrimaryPartitionKey { get { return "UserID"; } }

        // local place holder for current words to read
        public List<string> wordsToRead;
        // uniqe userID used to access database with primary key
        private string userId;

        private string timeStamp;

        /// <summary>
        /// Indicates if Unity application is running, is set by Unity 
        /// application and known through live-session database
        /// </summary>
        public bool AppReady { get; set; }

        /// <summary> Alexa can currently teach or assess using words to read. </summary>
        public MODE TeachMode { get; set; }
        /// <summary> live-session state machine  </summary>
        public STATE CurrentState { get; set; }
        /// <summary> The common-core skill Alexa is currently teaching to. </summary>
        public SKILL Skill { get; set; }
        /// <summary> The lesson that Alexa will teach. </summary>
        public LESSON Lesson { get; set; }
        /// <summary> Four digit index value that corresponds with scope and sequence database. </summary>
        public int CurrentSchedule { get; set; }

        /// <summary>
        /// Initializes live session data base with uniqe userID.
        /// </summary>
        /// <param name="userId"></param>
        public LiveSessionDB(string userId) : base(LiveSessionDB.TableName, LiveSessionDB.PrimaryPartitionKey)
        {
            this.userId = userId;
        }

        /// <summary>
        /// Updates live-session with current state and variables. 
        /// Must be awaited to ensure response from AWS dynamo.
        /// </summary>
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
                    { ":currentstate", new AttributeValue(this.CurrentState.ToString()) },
                    { ":curSched", currentSchedule }
                },
                UpdateExpression = "SET WordsToRead = :wordsToRead,  TeachMode = :teachMode, Skill = :skill, " +
                                   "CurrentState = :currentstate, CurrentSchedule = :curSched"
            };

            await SetItemsAttributeWithRequest(updateRequest);
        }

        /// <summary>
        /// Populates object variables from live-session database. 
        /// Must be awaited to ensure response from AWS dynamo.
        /// </summary>
        /// <returns></returns>
        public async Task GetDataFromLiveSession()
        {
            DatabaseItem items = await GetEntryByKey(this.userId);

            if (items.TryGetValue("WordsToRead", out AttributeValue words))
            {
                this.wordsToRead = words.SS;
            }

            if (items.TryGetValue("TeachMode", out AttributeValue mode))
            {
                this.TeachMode = (MODE)Enum.Parse(typeof(MODE), mode.S);
            }

            if (items.TryGetValue("CurrentState", out AttributeValue state))
            {
                this.CurrentState = (STATE)Enum.Parse(typeof(STATE), state.S);
            }

            if (items.TryGetValue("Skill", out AttributeValue skill))
            {
                this.Skill = (SKILL)Enum.Parse(typeof(SKILL), skill.S);
            }

            if (items.TryGetValue("CurrentSchedule", out AttributeValue curSched))
            {
                this.CurrentSchedule = int.Parse(curSched.N);
            }

            if (items.TryGetValue("Timestamp", out AttributeValue timeStamp))
            {
                this.timeStamp = timeStamp.S;
            }

            if (items.TryGetValue("AppReady", out AttributeValue appReady))
            {
                this.AppReady = appReady.BOOL;
            }

        }

        /// <summary>
        /// Removes the current word from local variable, NOT FROM 
        /// DATABASE.
        /// </summary>
        /// <param name="currentWord">current word to remove</param>
        /// <returns>true if words to read list is empty, indicating 
        /// successful completion of session.</returns>
        public bool Remove(string currentWord)
        {
            wordsToRead.Remove(currentWord);
            return (wordsToRead.Count == 0) ? true : false;
        }

        /// <summary>
        /// Gets current word by looking at index 0 of wordsToRead. 
        /// </summary>
        /// <returns>Current word to prompt user.</returns>
        public string GetCurrentWord()
        {
            if (wordsToRead.Any())
            {
                return wordsToRead[0];
            }

            return null;
        }

        /// <summary>
        /// Get the amount of words left to read. Used by unity to 
        /// populate progress bar.
        /// </summary>
        /// <returns>The number of words remaining.</returns>
        public int GetWordsRemaining()
        {
            return wordsToRead.Count;
        }


    }
}
