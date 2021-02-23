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
    public class ProductInventory
    {
        private SkillRequest input;
        private InSkillProductsClient client;
        private InSkillProductsResponse productsResponse;

        // Purchasable Content Reference Name
        public static string WordFamily { get { return "word_family"; } }
        public string ShortVowel { get { return "short_vowel"; } }        

        public ProductInventory(SkillRequest input)
        {
            this.input = input;
        }

        public async Task GetAvailableProducts()
        {
            LOGGER.log.INFO("ProductInventory", "UpdateProductInformation");
            this.client = new InSkillProductsClient(this.input);

            try
            {
                this.productsResponse = await client.GetProducts();
            }
            catch (Exception e)
            {
                LOGGER.log.WARN("ProductInventory", "UpdateProductInformation", "EXCEPTION: " + e.Message);
            }
            
        }

        /// <summary>
        /// Determines if the current schedule has been paid for. If active entitlements are greater
        /// than zero, than the product has been purchased. If active entitlements are 0, than the user
        /// still need to purchase item.
        /// </summary>
        /// <param name="productName">The name of the product from scope and sequence</param>
        public bool IsUnpaid(string productName)
        {
            var item = productsResponse.Products.FirstOrDefault(o => o.ReferenceName == productName);

            return (item != null && item.ActiveEntitlementCount == 0);
        }

        public string GetProductId(string productName)
        {
            var item = productsResponse.Products.FirstOrDefault(o => o.ReferenceName == productName);

            return item.ProductId;
        }
    }
}
