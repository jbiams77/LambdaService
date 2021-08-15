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
using Infrastructure.DynamoDB;
using Infrastructure.GlobalConstants;
using Infrastructure.CognitoPool;
using Infrastructure.Logger;
using Alexa.NET.InSkillPricing;
using Alexa.NET.InSkillPricing.Directives;

namespace Infrastructure.Alexa
{
    public class ProductInventory
    {
        private SkillRequest input;
        private InSkillProductsClient client;
        private InSkillProductsResponse productsResponse;
        public bool Purchasable { get; set; } = false;
        public bool Purchased { get; set; } = false;
        public string ProductId { get; set; } = "";

        public ProductInventory(SkillRequest input)
        {
            this.input = input;
        }

        public async Task UpdateProductInfo(string productName)
        {
            this.client = new InSkillProductsClient(this.input);
            this.productsResponse = await client.GetProducts();
            var item = productsResponse.Products.FirstOrDefault(o => o.ReferenceName == productName);
            Purchasable = (item != null && item.Purchasable.Equals(PurchaseState.Purchasable));
            Purchased = (item != null && item.ActiveEntitlementCount > 0);
            ProductId = item.ProductId;
        }

    }
}
