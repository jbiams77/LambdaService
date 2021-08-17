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
using System.Linq;

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
        
        [JsonProperty("Attempts")]
        public int Attempts { get; set; }

        [JsonProperty("Purchased")]
        public bool Purchased { get; set; }

        [JsonProperty("ProductId")]
        public string ProductId { get; set; }

        [JsonProperty("Purchasable")]
        public bool Purchasable { get; set; }

        private string WORD_LIST_URL;

        private int SIZE;
        // free index is the "at" family
        private static readonly int FREE_LESSON = 5;
        protected ILesson lesson;

        [JsonConstructor]
        public WordsToRead() { }

        

        /// <summary>
        /// Constructor called to convert session attributes to WordsToRead object.
        /// </summary>
        /// <param name="skillRequest"> Payload from Alexa containing Session Attributes. </param>
        /// <param name="lesson_name"> Ex. Word Families, Sight Words, Long Vowels.  </param>
        public WordsToRead(SkillRequest skillRequest, string lesson_name)
        {
            skillRequest.Session.Attributes.TryGetValue("SessionAttribute", out object sessionObject);
            JObject jObject = (JObject)sessionObject;
            WordsToRead words = jObject.ToObject<WordsToRead>();

            this.Purchasable = words.Purchasable;
            this.ProductId = words.ProductId;
            this.Purchased = words.Purchased;
            this.Attempts = words.Attempts;
            this.CurrentWord = words.CurrentWord;
            this.SessionIndex = words.SessionIndex;
            this.ListOfSessionWords = words.ListOfSessionWords;
            this.lesson = LessonFactory.GetLesson(lesson_name, MoycaResponse.DisplaySupported);
        }

        /// <summary>
        /// Constructor called to initialize sessision words from launch. Retrieves JSON 
        /// list of words from S3 bucket by public URL. Randomly Selects list.
        /// </summary>
        /// <param name="size"> Amount of words in list. </param>
        /// <param name="url"> S3 bucket URL. </param>
        /// <param name="lesson_name"> Ex. Word Families, Sight Words, Long Vowels. </param>
        public WordsToRead(int size, string url, string lesson_name, ProductInventory product)
        {
            this.SIZE = size;
            this.WORD_LIST_URL = url;
            this.Purchased = product.Purchased;
            this.ProductId = product.ProductId;
            this.Purchasable = product.Purchasable;

            if (this.Purchased)
            {
                GetRandomSession();
            }
            else
            {
                GetFreeLesson();
            }
            
            lesson = LessonFactory.GetLesson(lesson_name, MoycaResponse.DisplaySupported);
        }

        private void GetFreeLesson()
        {
            ListOfSessionWords = new List<WordEntry>();
            JObject jObject = ReadJsonFile();
            SessionIndex = FREE_LESSON;

            if (jObject.TryGetValue(SessionIndex.ToString(), out var wordsToRead))
            {
                ConvertWordsToRead(wordsToRead);
            }
            if (ListOfSessionWords.Count > 0)
            {
                CurrentWord = ListOfSessionWords[0].Word;
            }
        }

        private void GetRandomSession()
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

        public bool RemoveCurrentWord()
        {
            

            if (ListOfSessionWords.Count > 1)
            {
                ListOfSessionWords.Remove(ListOfSessionWords.Single(s => s.Word.Equals(CurrentWord)));
                CurrentWord = ListOfSessionWords[0].Word;
                return false;
            }

            return true;
            
        }

        /// <summary>
        /// Retrieves the introduction for word families lesson.
        /// </summary>
        /// <returns> Lesson prompt of word family for Alexa to say. </returns>
        public string Introduction()
        {
            return this.lesson.Introduction(this.ListOfSessionWords[0]);
        }

        /// <summary>
        /// Teaches the lesson for specific word.
        /// </summary>
        /// <returns> Lesson prompt of word for Alexa to say. </returns>
        public string Teach()
        {
            return this.lesson.TeachTheWord(ListOfSessionWords.Find(s => s.Word.Equals(CurrentWord)));
        }

        public string SayTheWord()
        {
            if (!MoycaResponse.DisplaySupported)
            {
                return SSML.SpellOut(this.CurrentWord);
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Gives a hint when helps is asked.
        /// </summary>
        /// <returns> Lesson prompt of word for Alexa to say. </returns>
        public string Help()
        {
            return this.lesson.HelpWithWord(ListOfSessionWords.Find(s => s.Word.Equals(CurrentWord)));
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
