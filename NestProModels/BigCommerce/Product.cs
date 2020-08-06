using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NestProModels
{
    public class Product
    {
        // Values from Big Commerce API
        [JsonProperty("base_price")]
        public string BasePrice { get; set; }

        [JsonProperty("base_total")]
        public string BaseTotal { get; set; }

        [JsonProperty("quantity_shipped")]
        public int QuantityShipped { get; set; }

        public int Quantity { get; set; }

        public string Sku { get; set; }

        public int id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("product_options")]
        public List<ProductOptions> ProductOptions { get; set; }

        // NetSuite expected values
        public string ItemId { get; set; }

        public double Rate { get; set; }

        public double Amount { get; set; }

        public Boolean PersonalItem { get; set; }

        public string ShippingMethodName { get; set; }



    }
}
