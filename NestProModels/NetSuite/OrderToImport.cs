using System.Collections.Generic;

namespace NestProModels
{
    public class OrderToImport
    {
        public string CustomerName
        {
            get { return CustomerFirstName + " " + CustomerLastName; }
        }
        
        public string SiteOrderNumber { get; set; }
        public string Company { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string BillingFirstName { get; set; }
        public string BillingLastName { get; set; }
        public string Email { get; set; }
        public string NestProId { get; set; }
        public bool Taxable { get; set; }
        public string TaxVendor { get; set; }
        public int ParentAccountId { get; set; }
        public string PhoneNumber { get; set; }
        public string BillingAddressee { get; set; }
        public string BillingLine1 { get; set; }
        public string BillingLine2 { get; set; }
        public string BillingCity { get; set; }
        public string BillingState { get; set; }
        public string BillingZip { get; set; }
        public string BillingCountry { get; set; }
        public string ShippingFirstName { get; set; }
        public string ShippingLastName { get; set; }
        public string ShippingAddressee { get; set; }
        public string ShippingCompany { get; set; }
        public string ShippingLine1 { get; set; }
        public string ShippingLine2 { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingZip { get; set; }
        public string ShippingCountry { get; set; }
        public string ShippingPhone { get; set; }
        public string Note { get; set; }
        public double SH { get; set; }
        public string ShippingMethodName { get; set; }
        public string JobName { get; set; }
        public object CustomNetSuiteID { get; set; }
        public object DiscountNames { get; set; }
        public string AltOrderNumber { get; set; }
        public string IPAddress { get; set; }
        public int Microsite { get; set; }
        public int Department { get; set; }
        public int UserTypeId { get; set; }
        public int CheckoutTypeId { get; set; }
        public int PaymentMethodId { get; set; }
        public int CustomerId { get; set; }
        public int SameDayShipping { get; set; }
        public List<Product> Items { get; set; }
        public bool SourceComplete { get; set; } = true;
        public bool SourceKitsComplete { get; set; } = true;
        public bool FulfillComplete { get; set; } = true;
    }
}
