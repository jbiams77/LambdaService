using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Alexa
{
    [JsonObject("wordEntry")]
    public class WordEntry
    {
        [JsonProperty("words")]
        public string Word { get; set; }

        [JsonProperty("wordfamily")]
        public string WordFamily{ get; set; }

        [JsonProperty("cvc")]
        public string CVC{ get; set; }

        [JsonProperty("sightword")]
        public string SightWord{ get; set; }

        [JsonProperty("e_controlled")]
        public string E_controlled{ get; set; }

        [JsonProperty("r_controlled")]
        public string R_controlled{ get; set; }

        [JsonProperty("l_controlled")]
        public string L_controlled{ get; set; }

        [JsonProperty("consonantDigraph")]
        public string ConsonantDigraph{ get; set; }

        [JsonProperty("consonantBlend")]
        public string ConsonantBlend{ get; set; }

        [JsonProperty("vowelBlend")]
        public string VowelBlend{ get; set; }

        [JsonProperty("vowelType")]
        public string VowelType{ get; set; }

        [JsonProperty("vowel")]
        public string Vowel{ get; set; }

        [JsonProperty("firstLeter")]
        public string FirstLeter{ get; set; }

        [JsonProperty("syllables")]
        public string Syllables{ get; set; }

        [JsonProperty("vowelPhoneme")]
        public string VowelPhoneme{ get; set; }

        [JsonProperty("phoneme")]
        public string Phoneme{ get; set; }

        [JsonProperty("Sentence")]
        public string Sentence{ get; set; }


    }
}
