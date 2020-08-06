
namespace NestProModels
{
    public class ItemFulfillmentLine
    {
        public string ItemFulfillmentId { get; set; }

        public int Quantity { get; set; }

        public string SKU { get; set; }

        public string Carrier { get; set; }

        public string TrackingNumber { get; set; }

        public int? KitId { get; set; }

        public bool AddedToKit { get; set; } = false;

        public bool IsPersonal { get; set; }
    }
}
