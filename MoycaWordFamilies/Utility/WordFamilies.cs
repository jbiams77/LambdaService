using Alexa.NET.Request;
using Infrastructure.Alexa;
using Infrastructure.Factories;
using Infrastructure.GlobalConstants;
using Infrastructure.Interfaces;
using Newtonsoft.Json.Linq;
using System;

namespace MoycaWordFamilies.Utility
{    
    public class WordFamilies : WordsToRead
    {
                
        private static readonly string WORD_FAMILIES = "https://moyca-lambda-dependancies.s3-us-west-2.amazonaws.com/word-families.json";
        private static int SIZE => 36;
        public new string CurrentWord => base.CurrentWord;

        public WordFamilies(ProductInventory product) : base(SIZE, WORD_FAMILIES, "word_families", product) 
        {            
        }

        public WordFamilies(SkillRequest skillRequest) : base(skillRequest, "word_families")
        {
        }

    }
}
