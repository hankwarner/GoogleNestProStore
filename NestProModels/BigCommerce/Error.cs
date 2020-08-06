using Newtonsoft.Json;

namespace NestProModels
{
    public class Error
    {
        [JsonProperty("invalid_reason")]
        public int InvalidReason { get; set; }

        [JsonProperty("available_quantity")]
        public string AvailableQuantity { get; set; }

        [JsonProperty("order_product_id")]
        public string OrderProductId { get; set; }
    }
}
