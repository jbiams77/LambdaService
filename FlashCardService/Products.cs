using System;
using System.Collections.Generic;
using System.Text;

namespace FlashCardService
{
    public class ShortVowelProduct : Products
    {
        public ShortVowelProduct() : base("short_vowel", 1036, 1095) { }
    }

    public class WordFamiliesProduct : Products
    {
        public WordFamiliesProduct() : base("word_family", 1005, 1035) { }
    }

    public class Products : Purchaseable
    {
        public string ReferenceName { get; set; }
        public int IndexBegin { get; set; }
        public int IndexEnd { get; set; }
        public bool Purchased { get; set; }
        public Products(string referenceName, int indexBegin, int indexEnd)
        {
            ReferenceName = referenceName;
            IndexBegin = indexBegin;
            IndexEnd = indexEnd;
        }
    }

    interface Purchaseable
    {
        String ReferenceName { get; set; }
        int IndexBegin { get; set; }
        int IndexEnd { get; set; }
        bool Purchased { get; set; }
    }
    
}
