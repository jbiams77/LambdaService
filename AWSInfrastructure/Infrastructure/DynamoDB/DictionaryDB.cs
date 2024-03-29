﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Infrastructure.GlobalConstants;
using Infrastructure.Logger;

namespace Infrastructure.DynamoDB
{
    
    using DatabaseItem = Dictionary<string, AttributeValue>;

    /// <summary>Class <c>DictionaryDB</c>: For accessing the dictionary database
    /// for retrieving words and for handling any changes to the scope and sequence
    /// and dictionary word additions.</summary>
    ///
    public class DictionaryDB : DatabaseClient
    {
        /// <summary> Required to access dictionary in dynamoDB, cannot be changed. </summary>
        private static string TableName { get { return "dictionary"; } }

        /// <summary> Represents collumn name in dynamoDB for accessing unique keyed words. </summary>
        private static string PrimaryPartitionKey { get { return "Word"; } }

        /// <summary> All global secondary indexes use this condition to access content by query. </summary>
        private static string keyCondition = "#partitionKey = :partition AND #sortKey = :sort";

        /// <summary> For queries that produce more words than required by a session, limit to 10. </summary>
        private int MaxWordCount { get { return 10; } }

        private bool useWF;  // Word Family
        private bool useCVC; // consonant vowel consnant
        private bool useCD;  // consonant digraph
        private bool useCB;  // consonant blend
        private bool useSW;  // sight words
        private bool useE;   // e-controlled vowels
        private bool useR;   // r-controlled vowels
        private bool useL;   // l-controlled vowels
        private bool useVB;  // vowel-blend
        private bool useVT;  // vowel-type
        private bool useV;   // vowel
        private bool useS;   // syllable
        private bool useFL;  // First Letter

        // for additional filtering due to limitation of dynamo GSI and to avoid costly scan's
        private Filter filter;

        // additional parameters
        private string wordFamily;

        // global secondary indexes specific to dictionary
        private CVC_WF_index cVC_WF_Index; 
        private SW_V_index  sW_V_Index;
        private CVC_V_index  cVC_V_Index;
        private BE_V_index   bE_V_Index;
        private CD_CB_index cD_CB_Index;
        private WF_CD_index wF_CD_Index;
        private WF_CB_index wF_CB_Index;
        private CD_VT_index cD_VT_Index;
        private CB_VT_index cB_VT_Index;

        // words retrieved from dictionary based on scope and sequence
        private List<string> wordsToRead;

        private MoycaLogger log;

        public DictionaryDB(MoycaLogger logger) : base(DictionaryDB.TableName, DictionaryDB.PrimaryPartitionKey, logger)
        {
            cVC_WF_Index = new CVC_WF_index();
            cVC_V_Index = new CVC_V_index();
            sW_V_Index = new SW_V_index();
            bE_V_Index = new BE_V_index();
            cD_CB_Index = new CD_CB_index();
            wF_CD_Index = new WF_CD_index();
            wF_CB_Index = new WF_CB_index();
            cD_VT_Index = new CD_VT_index();
            cB_VT_Index = new CB_VT_index();
            this.InitializeBoolsToFalse();
            this.wordsToRead = new List<string>();
            this.log = logger;
        }

        /// <summary>If GetWordsToReadWithOrder() is not called, returns null.</summary>
        /// /// <returns>List of strings to either populate dictionary with
        ///  or to populate live-session with. Will return no more than 10</returns>
        public List<string> GetWordsToRead()
        {
            return this.wordsToRead;
        }

        public int GetSizeOfWordsToRead()
        {
            return this.wordsToRead.Count;
        }

        /// <summary>Retrieves words from dictionary based on order provided by scope-and-sequence.
        /// Must be awaited to ensure dynamoDB response is aquired.
        /// A Key Value Pair of:
        /// Dictionary order = {{WF, "at"}, {CVC, TRUE}};        
        /// This will grab all words from dictionary that are in the "at" family
        /// and a consonant-vowel-consonant. </summary>
        /// <param name="order">Key is lesson type mapped to lesson value.</param>
        public async Task GetWordsToReadWithOrder( Dictionary<string, string> order)
        {            

            foreach (KeyValuePair<string, string> item in order)
            {
                SetBool(item.Key);
            }

            await this.GetWordsWithBestMethod(order);
            this.InitializeBoolsToFalse();
        }

        /// <summary>Retrieves words attributes from dictionary using primary index, word.
        /// /// <param name="word">The word to find in dictionary.</param>
        /// /// <returns>Dictionary<string, AttributeValues> : List of attributes as a 
        ///  database item.</returns>
        public async Task<DatabaseItem> GetWordAttributesFromDictionary(string word)
        {
            return await base.GetEntryByKey(word);
        }



        /*********************** PRIVATE METHODS ***********************/
        private async Task<List<string>> GetCVCwithWordFamily(string wordFamily)
        {
            List<DatabaseItem> databaseItem = await base.GetItemsWithQueryRequest(GenerateQuery(cVC_WF_Index, "TRUE", wordFamily));

            return ExtractWordsFromItem(databaseItem);
        }

        private async Task<List<string>> GetCVCwithVowel(string vowel)
        {
            List<DatabaseItem> databaseItem = await base.GetItemsWithQueryRequest(GenerateQuery(cVC_V_Index, "TRUE", vowel));

            return ExtractWordsFromItem(databaseItem);
        }

        private async Task<List<string>> GetSightWordWithVowel(string vowel)
        {
            List<DatabaseItem> databaseItem = await base.GetItemsWithQueryRequest(GenerateQuery(sW_V_Index, "TRUE", vowel));

            return ExtractWordsFromItem(databaseItem);
        }

        private async Task<List<string>> GetBossyEwithVowel(string vowel)
        {
            List<DatabaseItem> databaseItem = await base.GetItemsWithQueryRequest(GenerateQuery(bE_V_Index, "TRUE", vowel));

            return ExtractWordsFromItem(databaseItem);
        }

        private async Task<List<string>> GetConsonantDigraphWithConsonantBlend(string consonantDigraph, string consonantBlend)
        {            
            List<DatabaseItem> databaseItem = await base.GetItemsWithQueryRequest(GenerateQuery(cD_CB_Index, consonantDigraph, consonantBlend));

            return ExtractWordsFromItem(databaseItem);
        }

        private async Task<List<string>> GetWordFamilyWithConsonantDigraph(string wordFamily, string consonantDigraph)
        {
            List<DatabaseItem> databaseItem = await base.GetItemsWithQueryRequest(GenerateQuery(wF_CD_Index, wordFamily, consonantDigraph));

            return ExtractWordsFromItem(databaseItem);
        }

        private async Task<List<string>> GetConsonantBlendWithWordFamily(string wordFamily, string consonantBlend)
        {
            List<DatabaseItem> databaseItem = await base.GetItemsWithQueryRequest(GenerateQuery(wF_CB_Index, wordFamily, consonantBlend));

            return ExtractWordsFromItem(databaseItem);
        }

        private async Task<List<string>> GetConsonantDigraphWithVowelType(string consonantDigraph, string vowelType)
        {
            List<DatabaseItem> databaseItem = await base.GetItemsWithQueryRequest(GenerateQuery(cD_VT_Index, consonantDigraph, vowelType));

            return ExtractWordsFromItem(databaseItem);
        }

        private async Task<List<string>> GetConsonantBlendWithVowelType(string consonantBlend, string vowelType)
        {
            List<DatabaseItem> databaseItem = await base.GetItemsWithQueryRequest(GenerateQuery(cB_VT_Index, consonantBlend, vowelType));

            return ExtractWordsFromItem(databaseItem);
        }

        private async Task GetWordsWithBestMethod(Dictionary<string, string> order)
        {
            if (useVT || useV || useS || useFL)
            {
                SetFilter(order);
            }

            if (useWF && useCVC)
            {
                order.TryGetValue("WF", out string wf);
                this.wordsToRead = await GetCVCwithWordFamily(wf);
            }
            else if (useWF && useCB)
            {
                order.TryGetValue("WF", out string wf);
                order.TryGetValue("CB", out string cb);
                this.wordsToRead = await GetConsonantBlendWithWordFamily(wf, cb);
            }
            else if (useCVC && useV)
            {
                order.TryGetValue("V", out string v);
                this.wordsToRead = await GetCVCwithVowel(v);
            }
            else if (useCD && useCB)
            {
                order.TryGetValue("CD", out string cd);
                order.TryGetValue("CB", out string cb);
                this.wordsToRead = await GetConsonantDigraphWithConsonantBlend(cd, cb);
            }
            else if (useCD && useVT)
            {
                order.TryGetValue("CD", out string cd);
                order.TryGetValue("VT", out string vt);
                this.wordsToRead = await GetConsonantDigraphWithVowelType(cd, vt);
            }
            else if (useCB && useVT)
            {
                order.TryGetValue("CB", out string cb);
                order.TryGetValue("VT", out string vt);
                this.wordsToRead = await GetConsonantBlendWithVowelType(cb, vt);
            }
            else if (useWF && useCD)
            {
                order.TryGetValue("WF", out string wf);
                order.TryGetValue("CD", out string cd);
                this.wordsToRead = await GetWordFamilyWithConsonantDigraph(wf, cd);
            }
            else if (useSW && useV)
            {
                order.TryGetValue("V", out string v);
                this.wordsToRead = await GetSightWordWithVowel(v);
            }
            else if (useE && useV)
            {
                order.TryGetValue("V", out string v);
                this.wordsToRead = await GetBossyEwithVowel(v);
            }
            this.filter = null;
        }

        private void SetFilter(Dictionary<string, string> order)
        {
            if (!order.TryGetValue("VT", out string vt))
            {
                vt = "FALSE";
            }
            
            if (!order.TryGetValue("V", out string v))
            {
                v = "FALSE";
            }

            if (!order.TryGetValue("S", out string s))
            {
                s = "FALSE";
            }

            if (!order.TryGetValue("FL", out string fl))
            {
                fl = "FALSE";
            }

            this.filter = new Filter(vt, v, s, fl);
        }

        private void InitializeBoolsToFalse()
        {
            this.useWF = false;
            this.useCVC = false;
            this.useCD = false;
            this.useCB = false;
            this.useSW = false;
            this.useE = false;
            this.useR = false;
            this.useL = false;
            this.useVB = false;
            this.useVT = false;
            this.useV = false;
            this.useS = false;
            this.useFL = false;
        }

        private void SetBool(string item)
        {
            switch (item)
            {
                case "WF": useWF = true; break;
                case "CVC": useCVC = true; break;
                case "CD": useCD = true; break;
                case "CB": useCB = true; break;
                case "SW": useSW = true; break;
                case "E": useE = true; break;
                case "R": useR = true; break;
                case "L": useL = true; break;
                case "VB": useVB = true; break;
                case "VT": useVT = true; break;
                case "V": useV = true; break;
                case "S": useS = true; break;
                case "FL": useFL = true; break;
                default: break;
            }
        }
        private List<string> ExtractWordsFromItem(List<DatabaseItem> items)
        {
            List<string> extractedWords = new List<string>();

            foreach (Dictionary<string, AttributeValue> item in items)
            {

                if (extractedWords.Count < this.MaxWordCount && FilterWord(item, out string word))
                {
                    extractedWords.Add(word);
                }

            }
            return extractedWords;
        }

        /// <summary>
        /// The global secondary index can only query one column in table. When the requirement
        /// for additional filtering exist, this will reduce the database item based on 
        /// additional filters. 
        /// </summary>
        /// <param name="item">one item with all collumn attributes retrieved from dynamoDB</param>
        /// <param name="word">filter produces null if this item does not meet filter requirements.</param>
        /// <returns></returns>
        private bool FilterWord(DatabaseItem item, out string word)
        {
            AttributeValue wordVal = new AttributeValue();

            if (this.filter!=null)
            {

                item.TryGetValue("VowelType", out AttributeValue vowelType);
                item.TryGetValue("Vowel", out AttributeValue vowel);
                item.TryGetValue("Syllables", out AttributeValue syllables);
                item.TryGetValue("FirstLetter", out AttributeValue firstLetter);

                if (WordMatches(vowel.S, vowelType.S, syllables.S, firstLetter.S))
                {
                    item.TryGetValue("Word", out wordVal);
                    word = wordVal.S;                    
                    return true;
                } 
                else
                {
                    word = null;
                    return false;
                }
            }
            else
            {
                item.TryGetValue("Word", out wordVal);
                word = wordVal.S;
                return true;
            }
            
        }

        private bool WordMatches(string vowel, string vowelType, string syllables, string firstLetter)
        {
            bool matches = false;

            if (this.useVT)
            {
                matches = vowelType.Equals(this.filter.VowelType);
            }

            if (this.useV)
            {
                matches = vowel.Equals(this.filter.Vowel);
            }
            
            if (this.useS)
            {
                matches = syllables.Equals(this.filter.Syllables);
            }

            if (this.useFL)
            {
                matches = firstLetter.Equals(this.filter.FirstLetter);
            }

            return matches;


        }


        /// <summary>
        /// Creates the query paylod that is sent to AWS dynamoDB. The query is based
        /// on a Global index which is more effient than database scan. The projected
        /// expression ensures only Word, VowelType, Vowel, and syllables are retrieved.
        /// </summary>
        /// <param name="gi">interface that contains the propery global index name</param>
        /// <param name="pKey">primary key</param>
        /// <param name="sKey">secondary key</param>
        /// <returns></returns>
        private QueryRequest GenerateQuery(GlobalIndex gi, string pKey, string sKey)
        {   
            return new QueryRequest
            {
                TableName = DictionaryDB.TableName,
                IndexName = gi.Name,
                KeyConditionExpression = DictionaryDB.keyCondition,
                ExpressionAttributeNames = new Dictionary<String, String> {
                        {"#partitionKey", gi.PartitionKey },
                        {"#sortKey", gi.SortKey}
                    },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                        {":partition",  new AttributeValue { S = pKey }},
                        {":sort",       new AttributeValue { S =  sKey }}
                    },
                ProjectionExpression = "Word, VowelType, Vowel, Syllables, FirstLetter",
                ScanIndexForward = true
            };
        }

        /*********************** GLOBAL SECONDARY INDEXES ***********************/

        private class CVC_WF_index : GlobalIndex
        {
            public virtual string Name { get { return "CVC-WF-index"; } }
            public virtual string PartitionKey { get { return "CVC"; } }
            public virtual string SortKey { get { return "WordFamily"; } }
            public CVC_WF_index() { }
        }

        private class WF_CD_index : GlobalIndex
        {
            public virtual string Name { get { return "WF-CD-index"; } }
            public virtual string PartitionKey { get { return "WordFamily"; } }
            public virtual string SortKey { get { return "ConsonantDigraph"; } }
            public WF_CD_index() { }
        }
        private class WF_CB_index : GlobalIndex
        {
            public virtual string Name { get { return "WF-CB-index"; } }
            public virtual string PartitionKey { get { return "WordFamily"; } }
            public virtual string SortKey { get { return "ConsonantBlend"; } }
            public WF_CB_index() { }
        }

        private class SW_V_index : GlobalIndex
        {
            public virtual string Name { get { return "SW-V-index"; } }
            public virtual string PartitionKey { get { return "SightWord"; } }
            public virtual string SortKey { get { return "Vowel"; } }
            public SW_V_index() { }
        }

        private class CVC_V_index : GlobalIndex
        {
            public virtual string Name { get { return "CVC-V-index"; } }
            public virtual string PartitionKey { get { return "CVC"; } }
            public virtual string SortKey { get { return "Vowel"; } }
            public CVC_V_index() { }
        }

        private class BE_V_index : GlobalIndex
        {
            public virtual string Name { get { return "BE-V-index"; } }
            public virtual string PartitionKey { get { return "BossyE"; } }
            public virtual string SortKey { get { return "Vowel"; } }
            public BE_V_index() { }
        }

        private class CD_CB_index : GlobalIndex
        {
            public virtual string Name { get { return "CD-CB-index"; } }
            public virtual string PartitionKey { get { return "ConsonantDigraph"; } }
            public virtual string SortKey { get { return "ConsonantBlend"; } }
            public CD_CB_index() { }
        }

        private class CD_VT_index : GlobalIndex
        {
            public virtual string Name { get { return "CD-VT-index"; } }
            public virtual string PartitionKey { get { return "ConsonantDigraph"; } }
            public virtual string SortKey { get { return "VowelType"; } }
            public CD_VT_index() { }
        }

        private class CB_VT_index : GlobalIndex
        {
            public virtual string Name { get { return "CB-VT-index"; } }
            public virtual string PartitionKey { get { return "ConsonantBlend"; } }
            public virtual string SortKey { get { return "VowelType"; } }
            public CB_VT_index() { }
        }

    }

}
