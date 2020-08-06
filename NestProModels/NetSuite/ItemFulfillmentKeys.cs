
namespace NestProModels
{
    public class ItemFulfillmentKeys
    {
        public string ItemFulfillmentId { get; set; }

        public string TrackingNumber { get; set; }


        /* The code below overrides the 'Equals' and 'GetHashCode' default equality comparer methods used by Linq so that
        *  we can use the GroupBy function with multiple keys.
        */
        public override int GetHashCode()
        {
            unchecked
            {
                return ((ItemFulfillmentId != null ? ItemFulfillmentId.GetHashCode() : 0) * 397) ^ (TrackingNumber != null ? TrackingNumber.GetHashCode() : 0);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;

            return Equals((ItemFulfillmentKeys)obj);
        }

        public bool Equals(ItemFulfillmentKeys other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return string.Equals(ItemFulfillmentId, other.ItemFulfillmentId)
                   && string.Equals(TrackingNumber, other.TrackingNumber);
        }
    }
}
