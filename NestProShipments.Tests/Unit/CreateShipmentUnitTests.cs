using Xunit;
using System.Collections.Generic;
using NestProHelpers;
using NestProShipments.Controllers;
using NestProModels;
using RestSharp;
using System.Linq;

namespace NestProShipments.Tests
{
    public class CreateShipmentUnitTests
    {
        private const string itemFulfillmentId = "71520176";
        private static int bigCommerceOrderId = 130;
        private static int orderAddressId = 31;

        private static string expectedCarrierOne = "ups";
        private static string expectedCarrierTwo = "fedex";
        private static string expectedCarrierThree = "usps";

        [Fact]
        public void WillGetImportedItemFulfillmentIds()
        {
            int orderId = 126;
            int shipmentId = 10;

            SetItemFulfillmentIdOnShipment(orderId, shipmentId);

            BigCommerceController.bigCommerceOrderId = orderId;
            List<string> importedItemFulfillmentIds = BigCommerceController.GetImportedItemFulfillments();

            Assert.Equal(itemFulfillmentId, importedItemFulfillmentIds[0]);
            Assert.Equal("Janes Order", importedItemFulfillmentIds[1]);

            RemoveItemFulfillmentId(orderId, shipmentId);
        }


        [Fact]
        public void WillGetOrderAddressId()
        {
            int getOrderAddressIdResult = BigCommerceController.GetOrderAddressId(bigCommerceOrderId);

            Assert.Equal(orderAddressId, getOrderAddressIdResult);
        }


        [Fact]
        public void WillParsePersonalItemValueForPersonalItem()
        {
            ProductOptions productOption = new ProductOptions();
            productOption.display_value = "1 Item - Personal Use - $129 / Each";
            List<ProductOptions> productOptions = new List<ProductOptions>() { productOption };

            bool isPersonal = BigCommerceController.isItemPersonal(productOptions);

            Assert.True(isPersonal);
        }


        [Fact]
        public void WillGetSKUOfParentKit()
        {
            int? parentKitId = 3854678;

            var sku = NetSuiteController.GetSKUOfParentKit(parentKitId);

            Assert.Equal("GoogleWifiRouter2Pointsw/CharcoalHub", sku);
        }


        [Fact]
        public void WillParsePersonalItemValueForNonPersonalItem()
        {
            ProductOptions productOptionOne = new ProductOptions();
            productOptionOne.display_value = "Stainless Steel";

            ProductOptions productOptionTwo = new ProductOptions();
            productOptionTwo.display_value = "4 Pack - $215.00 / Each - Best Value";

            List<ProductOptions> productOptions = new List<ProductOptions>();

            bool isPersonal = BigCommerceController.isItemPersonal(productOptions);

            Assert.False(isPersonal);
        }


        [Fact]
        public void WillParsePackQuantity()
        {
            ProductOptions productOptionOne = new ProductOptions();
            productOptionOne.display_name = "Material";
            productOptionOne.display_value = "Brass";

            ProductOptions productOptionTwo = new ProductOptions();
            productOptionTwo.display_name = "Value Pack Options";
            productOptionTwo.display_value = "4 Pack - $215.00 / Each - Best Value";

            List<ProductOptions> productOptions = new List<ProductOptions>() { productOptionOne, productOptionTwo };

            Product product = new Product();
            product.ProductOptions = productOptions;

            int packQuantity = BigCommerceController.ParsePackQuantity(product);
            int expectedPackQuantity = 4;

            Assert.Equal(expectedPackQuantity, packQuantity);
        }


        [Fact]
        public void WillGetKitQuantity()
        {
            string itemSku = "T3032US";
            BigCommerceController.bigCommerceOrderId = 130;

            int kitQuantity = BigCommerceController.GetKitQuantity(itemSku);
            int expectedKitQuantity = 4;

            Assert.Equal(expectedKitQuantity, kitQuantity);
        }

        [Fact]
        public void WillGetItemQuantity()
        {
            BigCommerceController.bigCommerceOrderId = 130;
            var itemFulfillment = new ItemFulfillmentLine();
            itemFulfillment.KitId = 3022285;
            itemFulfillment.Quantity = 12;
            int expectedItemQuantity = 3;

            int itemQuantity = BigCommerceController.GetItemQuantity(itemFulfillment);

            Assert.Equal(expectedItemQuantity, itemQuantity);
        }


        [Fact]
        public void WillSetMultipleTrackingNumbers()
        {
            string itemFulfillmentId = "71973675";
            string expectedTrackingNumber = "1Z7849523049857 1Z3897341903472";

            string trackingNumber = BigCommerceController.GetTrackingNumbers(itemFulfillmentId);

            Assert.Equal(expectedTrackingNumber, trackingNumber);
        }


        [Fact]
        public void WillGetTrackingNumbersByItemFulfillment()
        {
            string itemFulfillmentId = "71973675";

            List<string> trackingNumbers = NetSuiteController.GetTrackingNumbersByItemFulfillment(itemFulfillmentId);

            Assert.Equal("1Z7849523049857", trackingNumbers[0]);
            Assert.Equal("1Z3897341903472", trackingNumbers[1]);
        }


        [Fact]
        public void WillGetOrderProductIdOfPersonalItem()
        {
            string itemSku = "T4001ES";
            int orderId = 1086;
            bool isKit = false;
            bool isPersonal = true;
            int orderProductId = BigCommerceController.GetOrderProductId(itemSku, isKit, isPersonal, orderId);

            Assert.Equal(204, orderProductId);
        }


        [Fact]
        public void WillGetOrderProductIdOfNonPersonalItem()
        {
            string itemSku = "T4001ES";
            int orderId = 1086;
            bool isKit = false;
            bool isPersonal = false;
            int orderProductId = BigCommerceController.GetOrderProductId(itemSku, isKit, isPersonal, orderId);

            Assert.Equal(205, orderProductId);
        }


        [Fact]
        public void WillGetOrderProductIdOfSingleItem()
        {
            string itemSku = "NC5100US";
            int orderId = 1021;
            bool isKit = false;
            bool isPersonal = true;
            int orderProductId = BigCommerceController.GetOrderProductId(itemSku, isKit, isPersonal, orderId);

            Assert.Equal(100, orderProductId);
        }


        [Fact]
        public void WillGetOrderProductIdOfKit()
        {
            string itemSku = "NC5100US";
            int orderId = 1021;
            bool isKit = true;
            bool isPersonal = false;
            int orderProductId = BigCommerceController.GetOrderProductId(itemSku, isKit, isPersonal, orderId);

            Assert.Equal(99, orderProductId);
        }


        [Fact]
        public void WillGetItemIsPersonalValue()
        {
            string netsuiteSalesOrderId = "72290158";
            List<string> itemFulfillmentIdsImported = new List<string>();

            var itemFulfillmentGroupsToImport = NetSuiteController.GetItemFulfillmentsNotImported(netsuiteSalesOrderId, itemFulfillmentIdsImported);

            foreach(var itemGroup in itemFulfillmentGroupsToImport)
            {
                foreach (var line in itemGroup)
                {
                    Assert.NotNull(line.IsPersonal);
                }
            }
        }


        [Fact]
        public void WillGetShippingProviderFromCarrierName()
        {
            string carrier = "UPS GROUND";
            string shippingProvider = BigCommerceController.GetShippingProviderFromCarrierName(carrier);
            Assert.Equal(expectedCarrierOne, shippingProvider);

            carrier = "FedEx 2-Day Priority";
            shippingProvider = BigCommerceController.GetShippingProviderFromCarrierName(carrier);
            Assert.Equal(expectedCarrierTwo, shippingProvider);

            carrier = "USPS Standard";
            shippingProvider = BigCommerceController.GetShippingProviderFromCarrierName(carrier);
            Assert.Equal(expectedCarrierThree, shippingProvider);
        }

        [Fact]
        public void WillSetBigCommerceHeaders()
        {
            RestRequest request = new RestRequest(Method.GET);

            BigCommerceHelper.SetBigCommerceHeaders(request);

            Assert.Equal(4, request.Parameters.Count);
        }


        //[Fact]
        //public void WillGetItemsOnShipment()
        //{
        //    var netsuiteSalesOrderId = "71972697";
        //    Shipment shipment = new Shipment("ups", 31, "71973675", "1Z3459875345");
        //    List<string> importedItemFulfillmentIds = new List<string>();
        //    var itemFulfillmentGroupsToImport = NetSuiteController.GetItemFulfillmentsNotImported(netsuiteSalesOrderId, importedItemFulfillmentIds);

        //    foreach(var itemFulfillment in itemFulfillmentGroupsToImport)
        //    {
        //        BigCommerceController.AddItemsToShipment(shipment, itemFulfillment);
        //    }
        //}

        private static void SetItemFulfillmentIdOnShipment(int orderId, int shipmentId)
        {
            RestClient client = new RestClient($"{BigCommerceHelper.baseUrl}{orderId}/shipments");

            string jsonRequest = $"{{\"comments\": \"{itemFulfillmentId}\"}}";
            RestRequest request = BigCommerceHelper.CreateNewPutRequest(shipmentId, jsonRequest);

            client.Execute(request);
        }


        private static void RemoveItemFulfillmentId(int orderId, int shipmentId)
        {
            RestClient client = new RestClient($"{BigCommerceHelper.baseUrl}{orderId}/shipments");

            string jsonRequest = "{\"comments\": \"\"}";
            RestRequest request = BigCommerceHelper.CreateNewPutRequest(shipmentId, jsonRequest);

            client.Execute(request);
        }

    }
}
