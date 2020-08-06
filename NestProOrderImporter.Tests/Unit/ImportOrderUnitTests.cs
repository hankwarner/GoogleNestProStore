using System;
using Xunit;
using NestProOrderImporter.Controllers;
using NestProModels;
using NestProHelpers;
using System.Collections.Generic;
using RestSharp;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace NestProOrderImporter.Tests
{
    public class ImportOrderUnitTests
    {
        private static int bigCommerceOrderId = 130;

        [Fact]
        public void WillGetNetSuiteItemIdBySku()
        {
            string sku = "69000";
            string netsuiteItemId = "10268";
            string result = NetSuiteController.GetNetSuiteItemIdBySku(sku);

            Assert.Equal(netsuiteItemId, result);
        }


        [Fact]
        public void WillGetCustomerNetSuiteId()
        {
            OrderToImport customerRequest = new OrderToImport();
            customerRequest.Email = "BarryBlock@GeneCousineauActingStudio.com";
            int expectedCustomerId = 17494445;

            string netsuiteCustomerId = NetSuiteController.GetNetSuiteCustomerId(customerRequest);

            Assert.Equal(expectedCustomerId, Convert.ToInt32(netsuiteCustomerId));
        }

        [Fact]
        public void WillGetOrdersAwaitingFulfillment()
        {
            JArray ordersAwaitingFulfillment = BigCommerceController.GetOrdersAwaitingFulfillment();

            foreach (var order in ordersAwaitingFulfillment)
            {
                Order parsedOrder = JsonConvert.DeserializeObject<Order>(order.ToString());
                Assert.Equal("Awaiting Fulfillment", parsedOrder.status);
            }
        }

        [Fact]
        public void WillGetProductsOnOrder()
        {
            string productsApiUrl = $"{BigCommerceHelper.baseUrl}/{bigCommerceOrderId}/products";
            int expectedProductCount = 3;

            List<Product> productsOnOrder = BigCommerceController.GetProductsOnOrder(productsApiUrl);

            Assert.Equal(expectedProductCount, productsOnOrder.Count);
        }


        [Fact]
        public void WillGetTaxStatusOfExemptCustomer()
        {
            BigCommerceController.customerId = 2;
            string expectedTaxExemptionCategory = "G";

            var customerData = BigCommerceController.GetCustomerData();

            Assert.Equal(expectedTaxExemptionCategory, customerData.tax_exempt_category);
        }


        [Fact]
        public void WillGetTaxStatusOfNonExemptCustomer()
        {
            BigCommerceController.customerId = 33;
            string expectedTaxExemptionCategory = "";

            var customerData = BigCommerceController.GetCustomerData();

            Assert.Equal(expectedTaxExemptionCategory, customerData.tax_exempt_category);
        }


        [Fact]
        public void WillSetTaxableStatus()
        {
            BigCommerceCustomer customer = new BigCommerceCustomer();
            customer.tax_exempt_category = "G";

            OrderToImport netsuiteRequest = new OrderToImport();
            bool expectedTaxableStatus = false;

            NetSuiteController.SetTaxableStatus(customer, netsuiteRequest);

            Assert.Equal(expectedTaxableStatus, netsuiteRequest.Taxable);
        }


        [Fact]
        public void WillGetCustomerName()
        {
            BigCommerceController.customerId = 2;

            var customerData = BigCommerceController.GetCustomerData();

            Assert.Equal("Michael", customerData.first_name);
            Assert.Equal("Owens", customerData.last_name);
        }


        [Fact]
        public void WillSetCustomerName()
        {
            string firstName = "Morty";
            string lastName = "Smith";
            BigCommerceCustomer customer = new BigCommerceCustomer();
            customer.first_name = firstName;
            customer.last_name = lastName;

            OrderToImport netsuiteRequest = new OrderToImport();

            NetSuiteController.SetCustomerName(customer, netsuiteRequest);

            Assert.Equal(firstName, netsuiteRequest.CustomerFirstName);
            Assert.Equal(lastName, netsuiteRequest.CustomerLastName);
            Assert.Equal(firstName + " " + lastName, netsuiteRequest.CustomerName);
        }


        [Fact]
        public void WillGetNestProId()
        {
            BigCommerceController.customerId = 2;
            string expectedNestProId = "BB1-866";

            string nestProId = BigCommerceController.GetNestProId();

            Assert.Equal(expectedNestProId, nestProId);
        }


        //[Fact]
        //public void WillSetNetSuiteItemIdAndPersonalItemFlag()
        //{
        //    ProductOptions optionsOne = new ProductOptions();
        //    optionsOne.display_name = "Material";
        //    optionsOne.display_value = "Brass";

        //    ProductOptions optionsTwo = new ProductOptions();
        //    optionsTwo.display_name = "Value Pack Options";
        //    optionsTwo.display_value = "4 Pack - $215.00 / Each - Best Value";

        //    List<ProductOptions> productOptions = new List<ProductOptions>() { optionsOne, optionsTwo };

        //    Product product = new Product();
        //    product.ProductOptions = productOptions;
        //    product.Sku = "T3032US";

        //    List<Product> productsOnOrder = new List<Product>() { product };

        //    NetSuiteController.SetNetSuiteItemIdAndPersonalItemFlag(productsOnOrder);

        //    Assert.Equal("3022285", productsOnOrder[0].ItemId);
        //    Assert.False(productsOnOrder[0].PersonalItem);
        //}


        [Fact]
        public void WillCreateANewCustomerInNetsuite()
        {
            var createCustomerRequest = new OrderToImport();

            // General information 
            createCustomerRequest.Email = $"{Faker.Name.First()}{Faker.Name.Last()}@gmail.com";
            createCustomerRequest.PhoneNumber = Faker.Phone.Number();
            createCustomerRequest.Department = NetSuiteController.b2bDepartmentId;
            createCustomerRequest.UserTypeId = NetSuiteController.generalContractor;

            // Billing address
            createCustomerRequest.BillingFirstName = Faker.Name.First();
            createCustomerRequest.BillingLastName = Faker.Name.Last();
            createCustomerRequest.BillingLine1 = Faker.Address.StreetAddress();
            createCustomerRequest.BillingLine2 = "Apt B";
            createCustomerRequest.BillingCity = Faker.Address.City();
            createCustomerRequest.BillingState = Faker.Address.UsState();
            createCustomerRequest.BillingZip = Faker.Address.ZipCode();
            createCustomerRequest.BillingCountry = "US";

            // Shipping Address
            createCustomerRequest.ShippingFirstName = Faker.Name.First();
            createCustomerRequest.ShippingLastName = Faker.Name.Last();
            createCustomerRequest.ShippingLine1 = Faker.Address.StreetAddress();
            createCustomerRequest.ShippingLine2 = "Room 104";
            createCustomerRequest.ShippingCity = Faker.Address.City();
            createCustomerRequest.ShippingState = Faker.Address.UsState();
            createCustomerRequest.ShippingZip = Faker.Address.ZipCode();
            createCustomerRequest.ShippingCountry = "US";

            string customerId = NetSuiteController.GetNetSuiteCustomerId(createCustomerRequest);

            Assert.NotNull(customerId);
        }

        [Fact]
        public void WillGetCustomerShippingAddress()
        {
            string shippingAddressUrl = $"{BigCommerceHelper.baseUrl}/{bigCommerceOrderId}/shippingaddresses";

            ShippingAddress customerShippingAddress = BigCommerceController.GetCustomerShippingAddress(shippingAddressUrl);

            Assert.Equal(31, customerShippingAddress.id);
            Assert.Equal(bigCommerceOrderId, customerShippingAddress.order_id);
            Assert.Equal("Barry", customerShippingAddress.first_name);
            Assert.Equal("Block", customerShippingAddress.last_name);
            Assert.Equal("998 Hollywood Blvd", customerShippingAddress.street_1);
            Assert.Equal("Apt D", customerShippingAddress.street_2);
            Assert.Equal("Beverly Hills", customerShippingAddress.city);
            Assert.Equal("California", customerShippingAddress.state);
            Assert.Equal("90210", customerShippingAddress.zip);
        }

        [Fact]
        public void WillGetBasePriceAndQuantityOfProductsOnOrder()
        {
            string productsUrl = $"{BigCommerceHelper.baseUrl}/{bigCommerceOrderId}/products";

            List<Product> productsOnOrder = BigCommerceController.GetProductsOnOrder(productsUrl);

            foreach(var product in productsOnOrder)
            {
                if(product.id < 36 && product.id > 38)
                {
                    throw new Exception($"Unexpected product ID {product.id}");
                }
                else if (product.id == 36)
                {
                    Assert.Equal("149.0000", product.BasePrice);
                    Assert.Equal("149.0000", product.BaseTotal);
                    Assert.Equal(1, product.Quantity);
                    Assert.Equal("T3019US", product.Sku);
                }
                else if(product.id == 37)
                {
                    Assert.Equal("860.0000", product.BasePrice);
                    Assert.Equal("2580.0000", product.BaseTotal);
                    Assert.Equal(3, product.Quantity);
                    Assert.Equal("T3032US", product.Sku);
                }
                else if (product.id == 38)
                {
                    Assert.Equal("221.4500", product.BasePrice);
                    Assert.Equal("1107.2500", product.BaseTotal);
                    Assert.Equal(5, product.Quantity);
                    Assert.Equal("T3016US", product.Sku);
                }
            }
        }

        [Fact]
        public void WillSetOrderStatusToAwaitingShipmentAndNetSuiteOrderId()
        {
            string netsuiteOrderId = "70169180";

            var exception = Record.Exception(() => BigCommerceController.SetOrderStatus(bigCommerceOrderId, netsuiteOrderId));
            string[] results = GetOrderStatusAndNetSuiteOrderId(bigCommerceOrderId);

            Assert.Null(exception);
            Assert.Equal(netsuiteOrderId, results[0]);
            Assert.Equal("Awaiting Shipment", results[1]);

            ResetOrderStatusAndNetSuiteOrderId(bigCommerceOrderId, netsuiteOrderId);
        }

        private static string[] GetOrderStatusAndNetSuiteOrderId(int bigCommerceOrderId)
        {
            JArray ordersAwaitingShipment = GetOrdersAwaitingShipment();
            string[] results = new string[2];

            foreach (var order in ordersAwaitingShipment)
            {
                Order parsedOrder = JsonConvert.DeserializeObject<Order>(order.ToString());

                if(parsedOrder.id != bigCommerceOrderId)
                {
                    continue;
                }
                else
                {
                    string netsuiteOrderId = parsedOrder.staff_notes;
                    string orderStatus = parsedOrder.status;
                    results[0] = netsuiteOrderId;
                    results[1] = orderStatus;
                }
            }
            return results;
        }

        private static JArray GetOrdersAwaitingShipment()
        {
            RestClient client = new RestClient($"{BigCommerceHelper.baseUrl}?status_id=9");
            RestRequest request = BigCommerceHelper.CreateNewGetRequest();

            IRestResponse jsonResponse = client.Execute(request);
            JArray parsedResponse = BigCommerceHelper.ParseApiResponse(jsonResponse.Content);

            return parsedResponse;
        }

        private static void ResetOrderStatusAndNetSuiteOrderId(int bigCommerceOrderId, string netsuiteOrderId)
        {
            string jsonRequest = "{\"status_id\": 11, \"staff_notes\": \"\"}";
;
            RestClient client = new RestClient(BigCommerceHelper.baseUrl);
            RestRequest request = BigCommerceHelper.CreateNewPutRequest(bigCommerceOrderId, jsonRequest);

            IRestResponse ordersApiResponse = client.Execute(request);
        }
    }
}
