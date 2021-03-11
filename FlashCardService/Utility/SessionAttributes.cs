using System;
using System.Collections.Generic;
using Infrastructure.GlobalConstants;
using System.Linq;
using Infrastructure.Logger;
using System.ComponentModel;
using Newtonsoft.Json;
using FlashCardService;

namespace FlashCardService
{
    public class SessionAttributes
    {
        public STATE SessionState { get; set; }
        public string LessonType { get; set; }
        public MODE LessonMode { get; set; }
        public SKILL LessonSkill { get; set; }
        public int Schedule { get; set; }
        public List<string> WordsToRead { get; set; }
        public string CurrentWord { get; set; }
        public int TotalWordsInSession { get; set; }
        public int Attempts { get; set; }
        public string ProductName { get; set; }

        private MoycaLogger logger;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public SessionAttributes() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">Object used for logging</param>
        public SessionAttributes(MoycaLogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Converts the object to a dictionary of strings to objects
        /// </summary>
        /// <returns>this class as a ditionary</returns>
        public Dictionary<string, object> ToDictionary()
        {
                IDictionary<string, object> sessionAttributeDictionary = this.AsDictionary();
                return (Dictionary<string, object>)sessionAttributeDictionary;
        }

        /// <summary>
        /// Updates this session attributes with scopeAndSequenceData
        /// </summary>
        /// <param name="scopeAndSequence">Schedule data from dynamoDb</param>
        /// <param name="schedule">Schedule number</param>
        /// <param name="mode">Teach or assess modes determined by user profile</param>
        public void UpdateSessionAttributes(UserProfileDB userProfile)
        {
            LOGGER.log.INFO("Function", "PopulateSessionAttributes", "Transferring Data");
            this.WordsToRead = userProfile.scopeAndSequenceDB.WordsToRead;
            this.CurrentWord = GetCurrentWord();
            this.LessonMode = userProfile.GetMode();
            this.LessonSkill = (SKILL)(int.Parse(userProfile.scopeAndSequenceDB.Skill));
            this.LessonType = userProfile.scopeAndSequenceDB.Lesson;
            this.Schedule = userProfile.Schedule;
            this.TotalWordsInSession = userProfile.scopeAndSequenceDB.WordsToRead.Count();
            this.Attempts = 0;
            this.ProductName = "NOT PROVIDED";
        }


        /// <summary>
        /// Updates this object with the values in the sessionAttributeDict
        /// </summary>
        /// <param name="sessionAttributeDict">Dictionary whose keys match the public members of this class</param>
        public void UpdateSessionAttributes(Dictionary<string, object> sessionAttributeDict)
        {
            SessionAttributes updatedSessionAttributes;
            try
            {
                updatedSessionAttributes = JsonConvert.DeserializeObject<SessionAttributes>(JsonConvert.SerializeObject(sessionAttributeDict));
            }
            catch (Exception e)
            {
                logger?.WARN("SessionAttributes", "UpdateSessionAttributes", "Failed to update SessionAttributes. " + e.Message);
                return;
            }

            CurrentWord = updatedSessionAttributes.CurrentWord;
            SessionState = updatedSessionAttributes.SessionState;
            LessonType = updatedSessionAttributes.LessonType;
            LessonMode = updatedSessionAttributes.LessonMode;
            LessonSkill = updatedSessionAttributes.LessonSkill;
            Schedule = updatedSessionAttributes.Schedule;
            WordsToRead = updatedSessionAttributes.WordsToRead;
            TotalWordsInSession = updatedSessionAttributes.TotalWordsInSession;
            Attempts = updatedSessionAttributes.Attempts;
        }

        public void RemoveCurrentWord()
        {
            WordsToRead.Remove(GetCurrentWord());
            this.CurrentWord = GetCurrentWord();
        }

        private string GetCurrentWord()
        {
            if (WordsToRead.Any())
            {
                return WordsToRead.First();
            }
            else
            {
                logger?.WARN("SessionAttributes", "CurrentWord", "Requested CurrentWord, but WordsToRead was empty.");
                return "";
            }
        }
    }
}

// Utiltiy class used to convert between SessionAttribute object and dictionary
public static class DictionaryUtility
{
    public static IDictionary<string, object> AsDictionary(this object source)
    {
        return source.ToDictionary<object>();
    }

    public static IDictionary<string, T> ToDictionary<T>(this object source)
    {
        if (source == null) ThrowExceptionWhenSourceArgumentIsNull();

        var dictionary = new Dictionary<string, T>();
        foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
        {
            try
            {
                object value = property.GetValue(source);
                if (IsOfType<T>(value))
                {
                    dictionary.Add(property.Name, (T)value);
                }
            }
            catch (Exception e)
            {
                LOGGER.log.WARN("SessionAttributes", "ToDictionary", "Failed to convert SessionAttributes to dictionary. " + e.Message);
            }
        }
        return dictionary;
    }

    private static bool IsOfType<T>(object value)
    {
        return value is T;
    }

    private static void ThrowExceptionWhenSourceArgumentIsNull()
    {
        throw new NullReferenceException("Unable to convert anonymous object to a dictionary. The source anonymous object is null.");
    }
}
