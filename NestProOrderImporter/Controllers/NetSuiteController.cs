using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NestProModels;
using RestSharp;
using System.Data.SqlClient;
using Dapper;
using Serilog;
using NestProHelpers;

namespace NestProOrderImporter.Controllers
{
    public class NetSuiteController
    {
        private static string createCustomerSuiteletUrl = Environment.GetEnvironmentVariable("SUITELET_URL");
        private static string orderImporterRestletUrl = Environment.GetEnvironmentVariable("RESTLET_URL");
        private static string dbName = Environment.GetEnvironmentVariable("DB_HMW");

        public static int b2bDepartmentId = 23;
        public static int micrositeId = 31;
        public static int registered = 4;
        public static int generalContractor = 4;
        public static int creditCard = 1;
        public static int sameDayFullyCommittedOnly = 2;
        
        private static string netsuiteItemId { get; set; }


        public static string GetNetSuiteCustomerId(OrderToImport createCustomerRequest)
        {
            var client = new RestClient(createCustomerSuiteletUrl);
            var jsonRequest = JsonConvert.SerializeObject(createCustomerRequest);

            var request = new RestRequest(Method.GET);

            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("jsonRequest", jsonRequest, ParameterType.GetOrPost);

            var response = client.Execute(request);
            
            if (response.Content.Contains("Error"))
            {
                var errorMessage = $"CreateCustomerSuitelet return an Error: {response.Content}";
                Log.Error(errorMessage);
                throw new Exception(errorMessage);
            }

            var customerId = response.Content;

            Log.Information($"customerId {customerId}");
            return customerId;
        }

        public static OrderToImport CreateNetSuiteRequest(Order parsedOrder, ShippingAddress customerShippingAddress)
        {
            OrderToImport netsuiteRequest = new OrderToImport();
            BigCommerceCustomer customerData = BigCommerceController.GetCustomerData();

            SetTaxableStatus(customerData, netsuiteRequest);

            SetCustomerName(customerData, netsuiteRequest);

            SetBillingInformation(parsedOrder, netsuiteRequest);

            SetShippingInformation(customerShippingAddress, netsuiteRequest);

            SetOrderDetails(parsedOrder, netsuiteRequest);

            netsuiteRequest.NestProId = BigCommerceController.GetNestProId();

            Log.Information("netsuiteRequest {@netsuiteRequest}", netsuiteRequest);

            return netsuiteRequest;
        }


        public static void SetBillingInformation(Order order, OrderToImport request)
        {
            int googleParentAccountId = 18054032;

            request.ParentAccountId = googleParentAccountId;
            request.Email = order.billing_address.email;
            request.PhoneNumber = order.billing_address.phone;
            request.Department = b2bDepartmentId;
            request.IPAddress = order.ip_address;
            request.UserTypeId = generalContractor;
            request.BillingFirstName = order.billing_address.first_name;
            request.BillingLastName = order.billing_address.last_name;
            request.BillingLine1 = order.billing_address.street_1;
            request.BillingLine2 = order.billing_address.street_2;
            request.BillingCity = order.billing_address.city;
            request.BillingState = NetSuiteHelper.GetStateByName(order.billing_address.state);
            request.BillingZip = order.billing_address.zip;
            request.BillingCountry = order.billing_address.country_iso2;
            request.Company = order.billing_address.company;
        }


        public static void SetShippingInformation(ShippingAddress shippingAddress, OrderToImport request)
        {
            request.ShippingFirstName = shippingAddress.first_name;
            request.ShippingLastName = shippingAddress.last_name;
            request.ShippingCompany = shippingAddress.company;
            request.ShippingLine1 = shippingAddress.street_1;
            request.ShippingLine2 = shippingAddress.street_2;
            request.ShippingCity = shippingAddress.city;
            request.ShippingState = NetSuiteHelper.GetStateByName(shippingAddress.state);
            request.ShippingZip = shippingAddress.zip;
            request.ShippingCountry = shippingAddress.country_iso2;
            request.ShippingPhone = shippingAddress.phone;
            request.ShippingMethodName = BigCommerceHelper.GetShippingMethodName(shippingAddress.shipping_method);
        }


        public static void SetOrderDetails(Order order, OrderToImport request)
        {
            request.SiteOrderNumber = $"NP{Convert.ToInt32(order.id)}";
            request.AltOrderNumber = order.payment_provider_id; // Processing Gateway ID
            request.Note = order.customer_message;
            request.Microsite = micrositeId;
            request.CheckoutTypeId = registered;
            request.PaymentMethodId = creditCard;
            request.SameDayShipping = sameDayFullyCommittedOnly;
            request.SH = Convert.ToDouble(order.base_shipping_cost);
        }


        public static void SetCustomerName(BigCommerceCustomer customer, OrderToImport request)
        {
            request.CustomerFirstName = customer.first_name;
            request.CustomerLastName = customer.last_name;
        }


        public static void SetTaxableStatus(BigCommerceCustomer customer, OrderToImport request)
        {
            // If there is any value in the tax_exemption_category, they are tax exempt
            request.Taxable = customer.tax_exempt_category == "";

            // If customer is taxable, set tax vendor to Avatax
            request.TaxVendor = request.Taxable ? "1990053" : " ";
        }


        public static string ImportOrderToNetSuite(OrderToImport importOrderRequest)
        {
            var client = new RestClient(orderImporterRestletUrl);
            var jsonRequest = JsonConvert.SerializeObject(importOrderRequest);
            var request = NetSuiteHelper.CreateNewRestletRequest(jsonRequest, orderImporterRestletUrl, "OrderImporter");

            var orderImporterRestletResponse = client.Execute(request);
            var parsedNetSuiteResponse = JsonConvert.DeserializeObject<ImportOrderResponse>(orderImporterRestletResponse.Content);
            Log.Information("Netsuite Order Importer response {@parsedNetSuiteResponse}", parsedNetSuiteResponse);

            if (parsedNetSuiteResponse.error != null)
            {
                throw new Exception($"Error: {parsedNetSuiteResponse.error}");
            }

            return parsedNetSuiteResponse.salesOrderRecordId;
        }

        public static void SetNetSuiteItemIdAndPersonalItemFlag(OrderToImport netsuiteRequest)
        {
            var productsOnOrder = netsuiteRequest.Items;

            foreach (Product product in productsOnOrder)
            {

                foreach (ProductOptions option in product.ProductOptions)
                {
                    String[] seperators = { " ", "-" };
                    String[] displayValue = option.display_value.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
                    Log.Information("display value {@displayValue}", displayValue);
                    Log.Information($"display name {option.display_name}");

                    if (option.display_name.ToLower().Contains("pack"))
                    {
                        int packQuantity = 0;
                        // Look for a pack quantity in the first character of display value. If one is found, append it to the end of the sku
                        if (int.TryParse(displayValue[0], out packQuantity))
                        {
                            if(packQuantity > 1)
                            {
                                product.Sku = $"{product.Sku}_{packQuantity}";
                            }
                        }

                    }
                    // Set the personal item flag
                    if (option.display_value.ToLower().Contains("personal"))
                    {
                        Log.Information($"PersonalItem is true");
                        product.PersonalItem = true;
                    }
                }

                Log.Information($"Parsed sku {product.Sku}");
                product.ItemId = GetNetSuiteItemIdBySku(product.Sku);

                // Set the Shipping Method Name on the item line
                if (netsuiteRequest.ShippingMethodName.Contains("Priority"))
                {
                    product.ShippingMethodName = "Priority";
                }
                else
                {
                    product.ShippingMethodName = "Standard";
                }
            }
        }

        public static string GetNetSuiteItemIdBySku(string sku)
        {
            using (SqlConnection connection = new SqlConnection(NetSuiteHelper.GetAthenaDBConnectionString(dbName)))
            {
                try
                {
                    connection.Open();
                    string query = $"SELECT [ITEM_ID] FROM [HMWallaceDATA].[dbo].[NETSUITEITEMS] WHERE [FULL_NAME] = '{sku}'";

                    netsuiteItemId = connection.QuerySingle<String>(query);

                    connection.Close();
                }
                catch (Exception ex)
                {
                    var itemIdErrormessage = $"Error in GetNetSuiteItemIdBySku. Item SKU {sku} was not found. Error: {ex}";
                    Log.Error(itemIdErrormessage);
                    throw new Exception(itemIdErrormessage);
                }

                return netsuiteItemId;
            }
        }
    }
}
