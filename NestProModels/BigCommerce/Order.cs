using System.Collections.Generic;

namespace NestProModels
{
    public class Order
    {
        public int id { get; set; }
        public int customer_id { get; set; }
        public string date_created { get; set; }
        public string date_modified { get; set; }
        public string date_shipped { get; set; }
        public int status_id { get; set; }
        public string status { get; set; }
        public string subtotal_ex_tax { get; set; }
        public string subtotal_inc_tax { get; set; }
        public string subtotal_tax { get; set; }
        public string base_shipping_cost { get; set; }
        public string shipping_cost_ex_tax { get; set; }
        public string shipping_cost_inc_tax { get; set; }
        public string shipping_cost_tax { get; set; }
        public int shipping_cost_tax_class_id { get; set; }
        public string base_handling_cost { get; set; }
        public string handling_cost_ex_tax { get; set; }
        public string handling_cost_inc_tax { get; set; }
        public string handling_cost_tax { get; set; }
        public int handling_cost_tax_class_id { get; set; }
        public string base_wrapping_cost { get; set; }
        public string wrapping_cost_ex_tax { get; set; }
        public string wrapping_cost_inc_tax { get; set; }
        public string wrapping_cost_tax { get; set; }
        public int wrapping_cost_tax_class_id { get; set; }
        public string total_ex_tax { get; set; }
        public string total_inc_tax { get; set; }
        public string total_tax { get; set; }
        public int items_total { get; set; }
        public int items_shipped { get; set; }
        public string payment_method { get; set; }
        public string payment_provider_id { get; set; }
        public string payment_status { get; set; }
        public string refunded_amount { get; set; }
        public bool order_is_digital { get; set; }
        public string store_credit_amount { get; set; }
        public string gift_certificate_amount { get; set; }
        public string ip_address { get; set; }
        public string geoip_country { get; set; }
        public string geoip_country_iso2 { get; set; }
        public int currency_id { get; set; }
        public string currency_code { get; set; }
        public string currency_exchange_rate { get; set; }
        public int default_currency_id { get; set; }
        public string default_currency_code { get; set; }
        public string staff_notes { get; set; }
        public string customer_message { get; set; }
        public string discount_amount { get; set; }
        public string coupon_discount { get; set; }
        public int shipping_address_count { get; set; }
        public bool is_deleted { get; set; }
        public string ebay_order_id { get; set; }
        public string cart_id { get; set; }
        public BillingAddress billing_address { get; set; }
        public bool is_email_opt_in { get; set; }
        public string credit_card_type { get; set; }
        public string order_source { get; set; }
        public int channel_id { get; set; }
        public string external_source { get; set; }
        public Products products { get; set; }
        public ShippingAddresses shipping_addresses { get; set; }
        public object external_id { get; set; }
        public object external_merchant_id { get; set; }
        public string tax_provider_id { get; set; }
        public string store_default_currency_code { get; set; }
        public string store_default_to_transactional_exchange_rate { get; set; }
        public string custom_status { get; set; }
    }

    public class BillingAddress
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string street_1 { get; set; }
        public string street_2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string country { get; set; }
        public string country_iso2 { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public List<object> form_fields { get; set; }
    }

    public class Products
    {
        public string url { get; set; }
        public string resource { get; set; }
    }

    public class ShippingAddresses
    {
        public string url { get; set; }
        public string resource { get; set; }
    }
}
