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

namespace MoycaWordFamilies.Utility
{
    [JsonObject("WordsToRead")]
    public class WordsToRead
    {
        [JsonProperty("CurrentWord")]
        public string CurrentWord { get; set; }

        [JsonProperty("SessionIndex")]
        public int SessionIndex { get; set; }

        [JsonProperty("ListOfSessionWords")]
        List<WordEntry> ListOfSessionWords;

        [JsonProperty("SessionState")]
        public static STATE sessionState { get; set; }

        // TODO: these fields will be used in child class, above fields in parent class
        private string Filelocation => System.IO.Directory.GetCurrentDirectory() + @"\word-families.json";
        private int WordFamilySize => 36;

        public WordsToRead(Session session)
        {

        }

        public WordsToRead()
        {   
        }

        public async Task GetRandomSession()
        {
            Random random = new Random();
            SessionIndex = random.Next(WordFamilySize);
            ListOfSessionWords = new List<WordEntry>();
            JObject jObject = await ReadJsonFile();

            if (jObject.TryGetValue(SessionIndex.ToString(), out var wordsToRead))
            {
                ConvertWordsToRead(wordsToRead);
            }
            if (ListOfSessionWords.Count > 0)
            {
                CurrentWord = ListOfSessionWords[0].Words;
            }
        }


        private async Task<JObject> ReadJsonFile()
        {
            StreamReader sr = await S3.GetFile("moyca-lambda-dependancies", "word-families.json");
            string test = sr.ReadToEnd();
            JsonTextReader reader = new JsonTextReader(sr);
            using (reader)
            {
                return (JObject)JToken.ReadFrom(reader);
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
