using System.Collections.Generic;
using Newtonsoft.Json;

namespace NestProModels
{
    public class Shipment
    {
        public Shipment(string carrier, int orderAddressId, string itemFulfillmentRecordId, string trackingNumber)
        {
            ShippingProvider = carrier;
            OrderAddressId = orderAddressId;
            NetSuiteItemFulfillmentId = itemFulfillmentRecordId;
            TrackingNumber = trackingNumber;
        }

        [JsonProperty("comments")]
        public string NetSuiteItemFulfillmentId { get; set; }

        [JsonProperty("id")]
        public string ShipmentId { get; set; }

        [JsonProperty("order_address_id")]
        public int OrderAddressId { get; set; }

        [JsonProperty("tracking_number")]
        public string TrackingNumber { get; set; }

        [JsonProperty("shipping_provider")]
        public string ShippingProvider { get; set; }

        [JsonProperty("items")]
        public List<Item> Items { get; set; } = new List<Item>();

        [JsonProperty("status")]
        public int? Status { get; set; } = null;

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("details")]
        public Error Details { get; set; }


        // These properties will be ignored during serialization but not deserialization
        public bool ShouldSerializeShipmentId()
        {
            return false;
        }

        public bool ShouldSerializeStatus()
        {
            return false;
        }

        public bool ShouldSerializeMessage()
        {
            return false;
        }

        public bool ShouldSerializeDetails()
        {
            return false;
        }
    }
}
