using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Moyca.Database.GlobalConstants;

namespace Moyca.Database
{
    using DatabaseItem = Dictionary<string, AttributeValue>;

    public class DictionaryDB : DatabaseClient
    {
        public static string TableName { get { return "dictionary"; } }
        public static string PrimaryPartitionKey { get { return "Word"; } }

        public static string keyCondition = "#partitionKey = :partition AND #sortKey = :sort";
        public int MaxWordCount { get { return 10; } }

        private bool useWF;
        private bool useCVC;
        private bool useCD;
        private bool useCB;
        private bool useSW;
        private bool useE;
        private bool useR;
        private bool useL;
        private bool useVB;
        private bool useVT;
        private bool useV;
        private bool useS;

        private Filter filter;

        private CVC_WF_index cVC_WF_Index;
        private SW_VT_index  sW_VT_Index;
        private CVC_V_index  cVC_V_Index;
        private BE_V_index   bE_V_Index;
        private CD_CB_index cD_CB_Index;

        private List<string> wordsToRead;

        public DictionaryDB() : base(DictionaryDB.TableName, DictionaryDB.PrimaryPartitionKey)
        {
            cVC_WF_Index = new CVC_WF_index();
            cVC_V_Index = new CVC_V_index();
            sW_VT_Index = new SW_VT_index();
            bE_V_Index = new BE_V_index();
            cD_CB_Index = new CD_CB_index();
            this.InitializeBoolsToFalse();
            this.wordsToRead = new List<string>();
        }

        public List<string> GetWordsToRead()
        {
            return this.wordsToRead;
        }
        public async Task GetWordsToReadWithOrder( Dictionary<string, string> order)
        {            

            foreach (KeyValuePair<string, string> item in order)
            {
                SetBool(item.Key);
            }

            await this.GetWordsWithBestMethod(order);
            this.InitializeBoolsToFalse();
        }

        public async Task<List<string>> GetCVCwithWordFamily(string wordFamily)
        {
            List<DatabaseItem> databaseItem = await base.GetItemsWithQueryRequest(GenerateQuery(cVC_WF_Index, "TRUE", wordFamily));

            return ExtractWordsFromItem(databaseItem);
        }

        public async Task<List<string>> GetCVCwithVowel(string vowel)
        {
            List<DatabaseItem> databaseItem = await base.GetItemsWithQueryRequest(GenerateQuery(cVC_V_Index, "TRUE", vowel));

            return ExtractWordsFromItem(databaseItem);
        }

        public async Task<List<string>> GetSightWordWithVowelType(string vowelType)
        {
            List<DatabaseItem> databaseItem = await base.GetItemsWithQueryRequest(GenerateQuery(sW_VT_Index, "TRUE", vowelType));

            return ExtractWordsFromItem(databaseItem);
        }

        public async Task<List<string>> GetBossyEwithVowel(string vowel)
        {
            List<DatabaseItem> databaseItem = await base.GetItemsWithQueryRequest(GenerateQuery(bE_V_Index, "TRUE", vowel));

            return ExtractWordsFromItem(databaseItem);
        }

        public async Task<List<string>> GetConsonantDigraphWithConsonantBlend(string consonantDigraph, string consonantBlend)
        {            
            List<DatabaseItem> databaseItem = await base.GetItemsWithQueryRequest(GenerateQuery(cD_CB_Index, consonantDigraph, consonantBlend));

            return ExtractWordsFromItem(databaseItem);
        }

        /*********************** PRIVATE METHODS ***********************/
        private async Task GetWordsWithBestMethod(Dictionary<string, string> order)
        {
            if (useVT && useV && useS)
            {
                SetFilter(order);
            }

            if (useWF && useCVC)
            {
                order.TryGetValue("WF", out string wf);
                this.wordsToRead = await GetCVCwithWordFamily(wf);
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
            else if (useSW && useVT)
            {
                order.TryGetValue("VT", out string vt);
                this.wordsToRead = await GetSightWordWithVowelType(vt);
            }
        }

        private void SetFilter(Dictionary<string, string> order)
        {
            order.TryGetValue("VT", out string vt);
            order.TryGetValue("V", out string v);
            order.TryGetValue("S", out string s);
            this.filter = new Filter(vt, v, s);
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

        private bool FilterWord(DatabaseItem item, out string word)
        {
            AttributeValue wordVal = new AttributeValue();

            if (this.filter!=null)
            {
                item.TryGetValue("VowelType", out AttributeValue vowelType);
                item.TryGetValue("Vowel", out AttributeValue vowel);
                item.TryGetValue("Syllables", out AttributeValue syllables);
                
                if (vowelType.S.Equals(this.filter.VowelType) && vowel.S.Equals(this.filter.Vowel) && syllables.S.Equals(this.filter.Syllables))
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
                ProjectionExpression = "Word, VowelType, Vowel, Syllables",
                ScanIndexForward = true
            };
        }

        /*********************** GLOBAL INDEXES ***********************/

        public class CVC_WF_index : GlobalIndex
        {
            public virtual string Name { get { return "CVC-WF-index"; } }
            public virtual string PartitionKey { get { return "CVC"; } }
            public virtual string SortKey { get { return "WordFamily"; } }
            public CVC_WF_index() { }
        }

        public class SW_VT_index : GlobalIndex
        {
            public virtual string Name { get { return "SW-VT-index"; } }
            public virtual string PartitionKey { get { return "SightWord"; } }
            public virtual string SortKey { get { return "VowelType"; } }
            public SW_VT_index() { }
        }

        public class CVC_V_index : GlobalIndex
        {
            public virtual string Name { get { return "CVC-V-index"; } }
            public virtual string PartitionKey { get { return "CVC"; } }
            public virtual string SortKey { get { return "Vowel"; } }
            public CVC_V_index() { }
        }

        public class BE_V_index : GlobalIndex
        {
            public virtual string Name { get { return "BE-V-index"; } }
            public virtual string PartitionKey { get { return "BossyE"; } }
            public virtual string SortKey { get { return "Vowel"; } }
            public BE_V_index() { }
        }

        public class CD_CB_index : GlobalIndex
        {
            public virtual string Name { get { return "CD-CB-index"; } }
            public virtual string PartitionKey { get { return "ConsonantDigraph"; } }
            public virtual string SortKey { get { return "ConsonantBlend"; } }
            public CD_CB_index() { }
        }
    }

}
