using Alexa.NET.Request;
using Infrastructure.GlobalConstants;
using Infrastructure.S3;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Infrastructure.Interfaces;
using Infrastructure.Factories;

namespace Infrastructure.Alexa
{
    [JsonObject("WordsToRead")]
    public class WordsToRead
    {
        [JsonProperty("CurrentWord")]
        protected string CurrentWord { get; set; }

        [JsonProperty("SessionIndex")]
        protected int SessionIndex { get; set; }

        [JsonProperty("ListOfSessionWords")]
        protected List<WordEntry> ListOfSessionWords;

        [JsonProperty("SessionState")]
        protected static STATE sessionState { get; set; }

        private string WORD_LIST_URL;

        private int SIZE;

        protected ILesson lesson;

        public WordsToRead(Session session)
        {

        }

        public WordsToRead(int size, string url, string lesson_name)
        {
            this.SIZE = size;
            this.WORD_LIST_URL = url;            
            GetRandomSession();
            lesson = LessonFactory.GetLesson(lesson_name);
        }

        public void GetRandomSession()
        {            
            Random random = new Random();
            SessionIndex = random.Next(SIZE);
            ListOfSessionWords = new List<WordEntry>();
            JObject jObject = ReadJsonFile();

            if (jObject.TryGetValue(SessionIndex.ToString(), out var wordsToRead))
            {
                ConvertWordsToRead(wordsToRead);
            }
            if (ListOfSessionWords.Count > 0)
            {
                CurrentWord = ListOfSessionWords[0].Word;
            }
        }


        private JObject ReadJsonFile()
        {
            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString(WORD_LIST_URL);
                return JObject.Parse(json);
            }            
        }

        private void ConvertWordsToRead(JToken wordsToRead)
        {            

            foreach (JProperty word in wordsToRead)
            {                
                ListOfSessionWords.Add(JsonConvert.DeserializeObject<WordEntry>(word.First.ToString()));
            }
        }

    }
}
