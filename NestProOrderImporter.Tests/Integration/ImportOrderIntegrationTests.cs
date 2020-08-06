using System;
using Xunit;
using NestProOrderImporter.Controllers;
using NestProModels;
using Faker;
using NestProHelpers;
using System.Collections.Generic;
using RestSharp;
using Newtonsoft.Json;

namespace NestProOrderImporter.Tests
{
    public class ImportOrderIntegrationTests
    {
        private static string orderImporterSpecControllerUrl = "https://634494-sb1.extforms.netsuite.com/app/site/hosting/scriptlet.nl?script=1779&deploy=1&compid=634494_SB1&h=e2c8c227c3eb3b838b7a";

        [Fact]
        public void WillCreateNetSuiteRequest()
        {
            BigCommerceController.customerId = 2;
            Order order = CreateFakeBigCommerceOrder();
            ShippingAddress shippingAddress = CreateFakeShippingAddress();

            OrderToImport netsuiteRequest = NetSuiteController.CreateNetSuiteRequest(order, shippingAddress);

            Assert.Equal(order.billing_address.email, netsuiteRequest.Email);
            Assert.Equal(order.billing_address.phone, netsuiteRequest.PhoneNumber);
            Assert.Equal(NetSuiteController.b2bDepartmentId, netsuiteRequest.Department);
            Assert.Equal(order.ip_address, netsuiteRequest.IPAddress);
            Assert.Equal("BB1-866", netsuiteRequest.NestProId);

            Assert.Equal($"NP{order.id}", netsuiteRequest.SiteOrderNumber);
            Assert.Equal(order.payment_provider_id, netsuiteRequest.AltOrderNumber);
            Assert.Equal(NetSuiteController.micrositeId, netsuiteRequest.Microsite);
            Assert.Equal(NetSuiteController.registered, netsuiteRequest.CheckoutTypeId);
            Assert.Equal(Convert.ToDouble(order.base_shipping_cost), netsuiteRequest.SH);

            Assert.Equal(order.billing_address.first_name, netsuiteRequest.BillingFirstName);
            Assert.Equal(order.billing_address.last_name, netsuiteRequest.BillingLastName);
            Assert.Equal(order.billing_address.street_1, netsuiteRequest.BillingLine1);
            Assert.Equal(order.billing_address.street_2, netsuiteRequest.BillingLine2);
            Assert.Equal(order.billing_address.city, netsuiteRequest.BillingCity);
            Assert.Equal(NetSuiteHelper.GetStateByName(order.billing_address.state), netsuiteRequest.BillingState);
            Assert.Equal(order.billing_address.zip, netsuiteRequest.BillingZip);
            Assert.Equal(order.billing_address.country_iso2, netsuiteRequest.BillingCountry);
            Assert.Equal(NetSuiteController.generalContractor, netsuiteRequest.UserTypeId);

            Assert.Equal(shippingAddress.first_name, netsuiteRequest.ShippingFirstName);
            Assert.Equal(shippingAddress.last_name, netsuiteRequest.ShippingLastName);
            Assert.Equal(shippingAddress.street_1, netsuiteRequest.ShippingLine1);
            Assert.Equal(shippingAddress.street_2, netsuiteRequest.ShippingLine2);
            Assert.Equal(shippingAddress.city, netsuiteRequest.ShippingCity);
            Assert.Equal(NetSuiteHelper.GetStateByName(shippingAddress.state), netsuiteRequest.ShippingState);
            Assert.Equal(shippingAddress.zip, netsuiteRequest.ShippingZip);
            Assert.Equal(shippingAddress.country_iso2, netsuiteRequest.ShippingCountry);
            Assert.Equal(BigCommerceHelper.GetShippingMethodName(shippingAddress.shipping_method), netsuiteRequest.ShippingMethodName);
        }


        //[Fact]
        //public void WillSetNetSuiteItemIdAndPersonalItemFlag()
        //{
        //    ProductOptions productOneOptions = new ProductOptions();
        //    productOneOptions.display_name = "Value Pack Quantity";
        //    productOneOptions.display_value = "1 item / Personal Use";

        //    ProductOptions productTwoOptions = new ProductOptions();
        //    productTwoOptions.display_name = "Value Pack Quantity";
        //    productTwoOptions.display_value = "1 item";

        //    Product productOne = new Product();
        //    productOne.Sku = "DUMMYITEM";
        //    productOne.id = 1;
        //    productOne.ProductOptions = new List<ProductOptions>() { productOneOptions };

        //    Product productTwo = new Product();
        //    productTwo.Sku = "DUMMYITEM2";
        //    productTwo.id = 2;
        //    productTwo.ProductOptions = new List<ProductOptions>() { productTwoOptions };

        //    List<Product> productsOnOrder = new List<Product>() { productOne, productTwo };

        //    NetSuiteController.SetNetSuiteItemIdAndPersonalItemFlag(productsOnOrder);

        //    foreach(var item in productsOnOrder)
        //    {
        //        if(item.id == 1)
        //        {
        //            Assert.True(item.PersonalItem);
        //            Assert.Equal("484204", item.ItemId);
        //        }
        //        else if(item.id == 2)
        //        {
        //            Assert.False(item.PersonalItem);
        //            Assert.Equal("500338", item.ItemId);
        //        }
        //    }
        //}


        [Fact]
        public void WillImportOrderToNetSuite()
        {
            OrderToImport importOrderRequest = CreateFakeOrderToImport();

            string salesOrderId = NetSuiteController.ImportOrderToNetSuite(importOrderRequest);

            OrderToImport netsuiteResponse = GetSalesOrderValues(salesOrderId);

            // Billing address
            Assert.Equal($"{importOrderRequest.BillingFirstName} {importOrderRequest.BillingLastName}", netsuiteResponse.BillingAddressee);
            Assert.Equal(importOrderRequest.BillingLine1, netsuiteResponse.BillingLine1);
            Assert.Equal(importOrderRequest.BillingLine2, netsuiteResponse.BillingLine2);
            Assert.Equal(importOrderRequest.BillingCity, netsuiteResponse.BillingCity);
            Assert.Equal(importOrderRequest.BillingState, netsuiteResponse.BillingState);
            Assert.Equal(importOrderRequest.BillingZip, netsuiteResponse.BillingZip);

            // Shipping address
            Assert.Equal($"{importOrderRequest.ShippingFirstName} {importOrderRequest.ShippingLastName}", netsuiteResponse.ShippingAddressee);
            Assert.Equal(importOrderRequest.ShippingLine1, netsuiteResponse.ShippingLine1);
            Assert.Equal(importOrderRequest.ShippingLine2, netsuiteResponse.ShippingLine2);
            Assert.Equal(importOrderRequest.ShippingCity, netsuiteResponse.ShippingCity);
            Assert.Equal(importOrderRequest.ShippingState, netsuiteResponse.ShippingState);
            Assert.Equal(importOrderRequest.ShippingZip, netsuiteResponse.ShippingZip);

            Assert.Equal(importOrderRequest.CheckoutTypeId, netsuiteResponse.CheckoutTypeId);
            Assert.Equal(importOrderRequest.CustomerId, netsuiteResponse.CustomerId);
            Assert.Equal(importOrderRequest.Department, netsuiteResponse.Department);
            Assert.Equal(importOrderRequest.IPAddress, netsuiteResponse.IPAddress);
            Assert.Equal(importOrderRequest.Microsite, netsuiteResponse.Microsite);
            Assert.Equal(importOrderRequest.Note, netsuiteResponse.Note);
            Assert.Equal(importOrderRequest.SiteOrderNumber, netsuiteResponse.SiteOrderNumber);
            Assert.Equal(importOrderRequest.PaymentMethodId, netsuiteResponse.PaymentMethodId);

            // Items
            foreach (var item in netsuiteResponse.Items)
            {
                if(item.ItemId != "484204" && item.ItemId != "348230")
                {
                    throw new Exception($"Unexpected item id {item.ItemId}");
                }
                else if (item.ItemId == "484204")
                {
                    Assert.Equal(importOrderRequest.Items[0].Quantity, item.Quantity);
                    Assert.Equal(importOrderRequest.Items[0].Rate, item.Rate);
                    Assert.Equal(importOrderRequest.Items[0].Amount, item.Amount);
                    Assert.Equal(importOrderRequest.Items[0].PersonalItem, item.PersonalItem);
                }
                else
                {
                    Assert.Equal(importOrderRequest.Items[1].Quantity, item.Quantity);
                    Assert.Equal(importOrderRequest.Items[1].Rate, item.Rate);
                    Assert.Equal(importOrderRequest.Items[1].Amount, item.Amount);
                    Assert.Equal(importOrderRequest.Items[1].PersonalItem, item.PersonalItem);
                }
            }
        }


        private static OrderToImport GetSalesOrderValues(string salesOrderId)
        {
            RestClient client = new RestClient(orderImporterSpecControllerUrl);
            RestRequest request = BigCommerceHelper.CreateNewGetRequest();
            NetSuiteHelper.SetNetSuiteTestParameters(request, salesOrderId);

            IRestResponse response = client.Execute(request);
            OrderToImport parsedResponse = JsonConvert.DeserializeObject<OrderToImport>(response.Content);

            return parsedResponse;
        }


        private static Order CreateFakeBigCommerceOrder()
        {
            BillingAddress billingAddress = new BillingAddress();
            billingAddress.first_name = Faker.Name.First();
            billingAddress.last_name = Faker.Name.Last();
            billingAddress.street_1 = Faker.Address.StreetAddress();
            billingAddress.street_2 = "Apt B";
            billingAddress.city = Faker.Address.City();
            billingAddress.state = Faker.Address.UsState();
            billingAddress.zip = Faker.Address.ZipCode();
            billingAddress.country_iso2 = "US";
            billingAddress.email = $"{Faker.Name.First()}{Faker.Name.Last()}@gmail.com";

            Order order = new Order();
            order.billing_address = billingAddress;
            order.id = Faker.RandomNumber.Next();
            order.ip_address = Faker.RandomNumber.Next().ToString();
            order.payment_provider_id = Faker.RandomNumber.Next().ToString();
            order.base_shipping_cost = "50.0000";

            return order;
        }


        private static ShippingAddress CreateFakeShippingAddress()
        {
            ShippingAddress shippingAddress = new ShippingAddress();

            shippingAddress.first_name = Faker.Name.First();
            shippingAddress.last_name = Faker.Name.Last();
            shippingAddress.street_1 = Faker.Address.StreetAddress();
            shippingAddress.street_2 = "Room 104";
            shippingAddress.city = Faker.Address.City();
            shippingAddress.state = Faker.Address.UsState();
            shippingAddress.zip = Faker.Address.ZipCode();
            shippingAddress.country_iso2 = "US";
            shippingAddress.phone = Faker.Phone.Number();
            shippingAddress.shipping_method = "Priority";

            return shippingAddress;
        }


        private static OrderToImport CreateFakeOrderToImport()
        {
            var importOrderRequest = new OrderToImport();

            // Billing address
            importOrderRequest.BillingFirstName = Name.First();
            importOrderRequest.BillingLastName = Name.Last();
            importOrderRequest.BillingLine1 = Address.StreetAddress();
            importOrderRequest.BillingLine2 = "Apt B";
            importOrderRequest.BillingCity = Address.City();
            importOrderRequest.BillingState = "GA";
            importOrderRequest.BillingZip = "30316";
            importOrderRequest.BillingCountry = "US";

            // Shipping Address
            importOrderRequest.ShippingFirstName = Name.First();
            importOrderRequest.ShippingLastName = Name.Last();
            importOrderRequest.ShippingLine1 = Address.StreetAddress();
            importOrderRequest.ShippingLine2 = "Room 104";
            importOrderRequest.ShippingCity = Address.City();
            importOrderRequest.ShippingState = "CA";
            importOrderRequest.ShippingZip = "90292";
            importOrderRequest.ShippingCountry = "US";
            importOrderRequest.ShippingMethodName = "UPS Ground";

            importOrderRequest.AltOrderNumber = $"{RandomNumber.Next()}";
            importOrderRequest.CheckoutTypeId = 4;
            importOrderRequest.CustomerId = 17494445;
            importOrderRequest.PhoneNumber = Phone.Number();
            importOrderRequest.Department = NetSuiteController.b2bDepartmentId;
            importOrderRequest.Email = $"{Name.First()}{Name.Last()}@gmail.com";
            importOrderRequest.IPAddress = "1.1.125.45845";
            importOrderRequest.PaymentMethodId = 1;
            importOrderRequest.Microsite = 31;
            importOrderRequest.Note = "There is no spoon";
            importOrderRequest.SiteOrderNumber = $"NP{RandomNumber.Next()}";
            importOrderRequest.Items = CreateFakeItems();

            return importOrderRequest;
        }

        private static List<Product> CreateFakeItems()
        {
            Product itemOne = new Product();
            itemOne.ItemId = "484204";
            itemOne.Rate = 584.00;
            itemOne.Amount = 1168.00;
            itemOne.PersonalItem = false;
            itemOne.Quantity = 2;

            Product itemTwo = new Product();
            itemTwo.ItemId = "348230";
            itemTwo.Rate = 380.00;
            itemTwo.Amount = 1520.00;
            itemTwo.PersonalItem = true;
            itemTwo.Quantity = 4;

            List<Product> productsOnOrder = new List<Product>() { itemOne, itemTwo };

            return productsOnOrder;
        }
    }
}
