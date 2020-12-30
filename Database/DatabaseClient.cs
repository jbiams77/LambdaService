using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Moyca.Database
{
    using DatabaseItem = Dictionary<string, AttributeValue>;

    /// Class handles all interactions with DynamoDB
    public abstract class DatabaseClient
    {
        private string tableName;
        private string primaryPartitionKey;
        private AmazonDynamoDBClient client = new AmazonDynamoDBClient();


        /// Constructor
        /// @param tableName - The name of the table that will be used to perform all functions of this class </param>
        /// @param primaryKeyName - The name of the primary lookup key for the specified table </param>
        public DatabaseClient(string tableName, string primaryKeyName)
        {
            this.tableName = tableName;
            this.primaryPartitionKey = primaryKeyName;
        }
        
        /// Accessor for a specific entry in the database
        /// @param primaryKey the key used to lookup the entry in the database
        protected async Task<DatabaseItem> GetItemWithString(string primaryKey)
        {
            var getRequest = new GetItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { primaryPartitionKey, new AttributeValue { S = primaryKey } }
                }
            };

            DatabaseItem item = new DatabaseItem();
            try
            {
                var response = await client.GetItemAsync(getRequest);
                if (response.IsItemSet)
                {
                    item = response.Item;
                }
            }
            catch (Exception e)
            {
                //info.Log("Failed to GetEntry + " + primaryKey + " from " + tableName + ": " + e);
            }

            return item;
        }

        /// Accessor for a specific entry in the database
        /// @param primaryKey the key used to lookup the entry in the database
        protected async Task<DatabaseItem> GetItemWithNumber(int primaryKey)
        {
            var getRequest = new GetItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { primaryPartitionKey, new AttributeValue { N = primaryKey.ToString() } }
                }
            };

            DatabaseItem item = new DatabaseItem();
            try
            {
                var response = await client.GetItemAsync(getRequest);
                if (response.IsItemSet)
                {
                    item = response.Item;
                }
            }
            catch (Exception e)
            {
                //Function.info.Log("Failed to GetEntry + " + primaryKey + " from " + tableName + ": " + e);
            }

            return item;
        }

        /// Returns a list of the entries that match the specified Global Secondary Index (GSI)
        /// @param secondaryIndexName - the name of the Global Secondary Index
        /// @param attributeKey the key of the column in the Database used as the GSI
        /// @param attributeValue the value to lookup on
        protected async Task<List<DatabaseItem>> GetEntriesBySecondaryIndex(string secondaryIndexName, string attributeKey, string attributeValue)
        {
            QueryRequest queryRequest = new QueryRequest
            {
                TableName = tableName,
                IndexName = secondaryIndexName,
                KeyConditionExpression = "#attrKey = :v_value",
                ExpressionAttributeNames = new Dictionary<String, String> {
                        {"#attrKey", attributeKey}
                    },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                        {":v_value", new AttributeValue { S =  attributeValue }},
                    },
                ScanIndexForward = true
            };

            try
            {
                var response = await client.QueryAsync(queryRequest);
                return response.Items;
            }
            catch (Exception e)
            {
                //Function.info.Log("Failed to GetEntriesBySecondaryIndex by " + secondaryIndexName + " from " + tableName + ": " + e);
            }

            return new List<DatabaseItem>();
        }

        /// Returns a list of the entries that match the specified Global Secondary Index (GSI)
        /// @param secondaryIndexName - the name of the Global Secondary Index
        /// @param attributeKey the key of the column in the Database used as the GSI
        /// @param attributeValue the value to lookup on
        protected async Task<List<DatabaseItem>> GetItemsWithQueryRequest(QueryRequest queryRequest)
        {

            try
            {
                var response = await client.QueryAsync(queryRequest);
                return response.Items;
            }
            catch (Exception e)
            {
                //Function.info.Log("Failed to get items by query request: "  + e);
            }

            return new List<DatabaseItem>();
        }

        /// Edits an entry in the database
        /// @param primaryKey - key of entry to edit
        /// @param attributeKey - attribute key that will be changed or added
        /// @param attributeValue - the value for the specified attributeKey
        protected async Task<bool> SetItemsAttribute(AttributeValue primaryKey, string attributeKey, AttributeValue attributeValue)
        {
            var updateRequest = new UpdateItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { primaryPartitionKey, primaryKey }
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":attr", attributeValue },
                },
                UpdateExpression = "SET " + attributeKey + " = :attr"
            };

            try
            {
                await client.UpdateItemAsync(updateRequest);
                return true;
            }
            catch (Exception e)
            {
               // Function.info.Log("Failed to set + " + tableName + "." + attributeKey + ": " + e);
                return false;
            }
        }

        /// Updates an entry in the database
        /// NOTE: This function deletes all existing attributes for the specified key and inserts the new attributes
        /// @param primaryKey - key of entry to edit
        /// @param attributes - a dictionary containing all of the attributes to set for the specified entry
        protected async Task<bool> PutAttributes(string primaryKey, Dictionary<string, AttributeValue> attributes)
        {
            // The primary key is required to determine which database entry to update
            attributes.Add(primaryPartitionKey, new AttributeValue { S = primaryKey });

            var putRequest = new PutItemRequest
            {
                TableName = tableName,
                Item = attributes
            };

            try
            {
                await client.PutItemAsync(putRequest);
                return true;
            }
            catch (Exception e)
            {
               // Function.info.Log("Failed to put item for key " + primaryKey + " in table " + tableName + ": " + e);
                return false;
            }
        }
    }
}

