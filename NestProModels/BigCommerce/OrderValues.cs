
namespace NestProModels
{
    public class OrderValues
    {
        public OrderValues(int statusId, string netsuiteOrderId)
        {
            status_id = statusId;
            staff_notes = netsuiteOrderId;
        }
        public int status_id { get; set; }
        public string staff_notes { get; set; }
    }
}