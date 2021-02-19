using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Alexa.NET;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using AWSInfrastructure.DynamoDB;
using AWSInfrastructure.GlobalConstants;
using AWSInfrastructure.CognitoPool;
using AWSInfrastructure.Logger;
using Alexa.NET.InSkillPricing;
using Alexa.NET.InSkillPricing.Directives;

namespace FlashCardService
{
    public class InSkillPurchase
    {
        private SkillRequest input;
        private InSkillProductsClient client;
        private InSkillProductsResponse productsResponse;
        private Dictionary<string, string> availableProductsForPurchase;

        // Purchasable Content Reference Name
        public static string WordFamily { get { return "word_family"; } }
        public string ShortVowel { get { return "short_vowel"; } }

        public InSkillPurchase(SkillRequest input)
        {
            this.input = input;
            this.availableProductsForPurchase = new Dictionary<string, string>();
        }

        public async Task GetAvailableProducts()
        {
            Function.log.INFO("InSkillPUrchase", "GetAvailableProducts");
            this.client = new InSkillProductsClient(this.input);

            try
            {
                this.productsResponse = await client.GetProducts();

                foreach (InSkillProduct product in this.productsResponse.Products)
                {
                    this.availableProductsForPurchase.Add(product.ReferenceName, product.ProductId);
                }
            }
            catch (Exception e)
            {
                Function.log.WARN("InSkillPurchase", "GetAvailableProducts", "EXCEPTION: " + e.Message);
            }
            
        }

        public string GetProductId(string referenceName)
        {
            if (availableProductsForPurchase.TryGetValue(referenceName, out string productID))
            {
                return productID;
            }
            else
            {
                return "NONE";
            }
        }
    }
}
