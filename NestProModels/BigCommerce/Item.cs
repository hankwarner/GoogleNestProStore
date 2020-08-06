
namespace NestProModels
{
    public class Item
    {
        public Item(int productId, int itemQuantity)
        {
            order_product_id = productId;
            quantity = itemQuantity;
        }

        public int order_product_id { get; set; }
        public int quantity { get; set; }

    }
}
