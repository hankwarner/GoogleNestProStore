using System;
using Xunit;
using System.Linq;
using System.Collections.Generic;
using NestProModels;
using NestProHelpers;
using NestProShipments.Controllers;
using RestSharp;
using Newtonsoft.Json;

namespace NestProShipments.Tests
{
    public class CreateShipmentIntegrationTests
    {
        private static int bigCommerceOrderId = 130;
        private static int orderAddressId = 31;
        private static string netsuiteSalesOrderId = "68854311";

        //private static string itemFulfillmentIdOne = "68893604";
        private static string itemFulfillmentIdOne = "74263389";
        private static string itemFulfillmentIdTwo = "68880501";

        private static string trackingNumberOne = "130833846";
        private static string trackingNumberTwo = "1Z7F45F30314255117";

        private static string carrierOne = "UPS GROUND";
        private static string carrierTwo = "FedEx Priority";

        private static int itemQuantityOne = 1;
        private static int itemQuantityTwo = 3;
        private static int itemQuantityThree = 5;

        private static string itemSkuOne = "GA00822-US";
        private static string itemSkuTwo = "GA00516-US";
        //private static string itemSkuOne = "T3019US";
        //private static string itemSkuTwo = "T3032US_4";
        private static string itemSkuThree = "T3016US";

        private static int expectedOrderProductIdOne = 36;
        private static int expectedOrderProductIdTwo = 37;
        private static int expectedOrderProductIdThree = 38;

        private static string expectedCarrierOne = "ups";
        private static string expectedCarrierTwo = "fedex";


        [Fact]
        public void WillGetItemFulfillmentsNotImportedToNetSuite()
        {
            List<string> itemFulfillmentIdsImported = new List<string>();

            var itemFulfillmentGroupsToImport = NetSuiteController.GetItemFulfillmentsNotImported(netsuiteSalesOrderId, itemFulfillmentIdsImported);

            foreach (var itemFulfillmentGroup in itemFulfillmentGroupsToImport)
            {
                foreach (var itemFulfillment in itemFulfillmentGroup)
                {
                    //Assert.Equal(itemFulfillmentIdOne, itemFulfillment.ItemFulfillmentId);
                    if (itemFulfillment.ItemFulfillmentId == itemFulfillmentIdOne)
                    {
                        Assert.Equal("R&L Carriers", itemFulfillment.Carrier);
                        Assert.Equal(trackingNumberOne, itemFulfillment.TrackingNumber);

                        if (itemFulfillment.SKU == "559LF-BLMPU")
                        {
                            Assert.Equal(1, itemFulfillment.Quantity);
                        }
                        else if (itemFulfillment.SKU == "SS114#01" || itemFulfillment.SKU == "C244EF#01" || itemFulfillment.SKU == "ST243E#01")
                        {
                            Assert.Equal(3, itemFulfillment.Quantity);
                        }
                    }
                    else if (itemFulfillment.ItemFulfillmentId == itemFulfillmentIdTwo)
                    {
                        Assert.Equal("UPS COLLECT", itemFulfillment.Carrier);
                        Assert.Equal(trackingNumberTwo, itemFulfillment.TrackingNumber);

                        if (itemFulfillment.SKU == "T14459-BL")
                        {
                            Assert.Equal(1, itemFulfillment.Quantity);
                        }
                    }

                }
            }
        }


        [Fact]
        public void WillCreateAShipmentRequest()
        {
            BigCommerceController.bigCommerceOrderId = 130;

            ItemFulfillmentLine itemFulfillmentLineOne = CreateItemFulfillmentLine(itemFulfillmentIdOne, carrierOne, itemSkuOne, trackingNumberOne, itemQuantityOne, null);
            ItemFulfillmentLine itemFulfillmentLineTwo = CreateItemFulfillmentLine(itemFulfillmentIdTwo, carrierTwo, itemSkuTwo, trackingNumberTwo, itemQuantityTwo, null);
            ItemFulfillmentLine itemFulfillmentLineThree = CreateItemFulfillmentLine(itemFulfillmentIdTwo, carrierTwo, itemSkuThree, trackingNumberTwo, itemQuantityThree, null);
            List<ItemFulfillmentLine> itemFulfillments = new List<ItemFulfillmentLine>() { itemFulfillmentLineOne, itemFulfillmentLineTwo, itemFulfillmentLineThree };
            

            var itemFulfillmentGroups = itemFulfillments.GroupBy(itfil => itfil.ItemFulfillmentId);

            foreach (var itemFulfillmentGroup in itemFulfillmentGroups)
            {
                BigCommerceController.currentItemFulfillment = itemFulfillmentGroup;

                Shipment shipmentToCreate = BigCommerceController.CreateShipmentRequest(itemFulfillmentGroup);

                Assert.Equal(orderAddressId, shipmentToCreate.OrderAddressId);

                if (shipmentToCreate.NetSuiteItemFulfillmentId == itemFulfillmentIdOne)
                {
                    Assert.Equal(trackingNumberOne, shipmentToCreate.TrackingNumber);
                    Assert.Equal(expectedCarrierOne, shipmentToCreate.ShippingProvider);

                    // Items
                    foreach (var item in shipmentToCreate.Items)
                    {
                        Assert.Equal(expectedOrderProductIdOne, item.order_product_id);
                        Assert.Equal(itemQuantityOne, item.quantity);
                    }
                }
                else if (shipmentToCreate.NetSuiteItemFulfillmentId == itemFulfillmentIdTwo)
                {
                    Assert.Equal(trackingNumberTwo, shipmentToCreate.TrackingNumber);
                    Assert.Equal(expectedCarrierTwo, shipmentToCreate.ShippingProvider);

                    // Items
                    foreach (var item in shipmentToCreate.Items)
                    {
                        if (item.order_product_id == expectedOrderProductIdTwo)
                        {
                            Assert.Equal(expectedOrderProductIdTwo, item.order_product_id);
                            Assert.Equal(itemQuantityTwo, item.quantity);
                        }
                        else if (item.order_product_id == expectedOrderProductIdThree)
                        {
                            Assert.Equal(expectedOrderProductIdThree, item.order_product_id);
                            Assert.Equal(itemQuantityThree, item.quantity);
                        }
                        else
                        {
                            throw new Exception($"Unexpected order product id {item.order_product_id}");
                        }
                    }
                }
                else
                {
                    throw new Exception($"Unexpected item fulfillment id {shipmentToCreate.NetSuiteItemFulfillmentId}");
                }
            }
        }


        [Fact]
        public void WillCreateAShipmentRequestWithPromoBundle()
        {
            BigCommerceController.bigCommerceOrderId = 2244;

            var kitId = 3854675;
            ItemFulfillmentLine itemFulfillmentLineOne = CreateItemFulfillmentLine(itemFulfillmentIdOne, carrierOne, itemSkuOne, trackingNumberOne, itemQuantityOne, kitId);
            ItemFulfillmentLine itemFulfillmentLineTwo = CreateItemFulfillmentLine(itemFulfillmentIdOne, carrierOne, itemSkuTwo, trackingNumberOne, itemQuantityOne, kitId);
            List<ItemFulfillmentLine> itemFulfillments = new List<ItemFulfillmentLine>() { itemFulfillmentLineOne, itemFulfillmentLineTwo };

            var itemFulfillmentGroups = itemFulfillments.GroupBy(itfil => itfil.ItemFulfillmentId);

            foreach (var itemFulfillmentGroup in itemFulfillmentGroups)
            {
                BigCommerceController.currentItemFulfillment = itemFulfillmentGroup;

                Shipment shipmentToCreate = BigCommerceController.CreateShipmentRequest(itemFulfillmentGroup);

                Assert.Single(shipmentToCreate.Items);
                Assert.Equal(2287, shipmentToCreate.Items[0].order_product_id);
                Assert.Equal(1, shipmentToCreate.Items[0].quantity);
            }
        }


        [Fact]
        public void WillPostShipmentToBigCommerce()
        {
            BigCommerceController.bigCommerceOrderId = bigCommerceOrderId;
            Shipment shipmentToCreate = new Shipment(expectedCarrierOne, orderAddressId, itemFulfillmentIdOne, trackingNumberOne);
            Item itemOne = new Item(expectedOrderProductIdOne, itemQuantityOne);
            Item itemTwo = new Item(expectedOrderProductIdTwo, itemQuantityTwo);
            Item itemThree = new Item(expectedOrderProductIdThree, itemQuantityThree);
            shipmentToCreate.Items = new List<Item>() { itemOne, itemTwo, itemThree };

            Shipment shipmentCreated = BigCommerceController.PostShipmentToBigCommerce(shipmentToCreate);

            Assert.Null(shipmentCreated.Status);
            Assert.Equal(itemFulfillmentIdOne, shipmentCreated.NetSuiteItemFulfillmentId);
            Assert.Equal(trackingNumberOne, shipmentCreated.TrackingNumber);
            Assert.Equal(expectedCarrierOne, shipmentCreated.ShippingProvider);
            Assert.Equal(orderAddressId, shipmentCreated.OrderAddressId);

            foreach (var item in shipmentCreated.Items)
            {
                int orderProductId = item.order_product_id;

                if (orderProductId != expectedOrderProductIdOne && orderProductId != expectedOrderProductIdTwo && orderProductId != expectedOrderProductIdThree)
                {
                    throw new Exception($"Unexpected order product id {orderProductId}");
                }
                else if (orderProductId == expectedOrderProductIdOne)
                {
                    Assert.Equal(itemQuantityOne, item.quantity);
                }
                else if (orderProductId == expectedOrderProductIdTwo)
                {
                    Assert.Equal(itemQuantityTwo, item.quantity);
                }
                else if (orderProductId == expectedOrderProductIdThree)
                {
                    Assert.Equal(itemQuantityThree, item.quantity);
                }
            }

            // Get the status and assert that it is marked as shipped
            string orderStatus = GetOrderStatus();
            Assert.Equal("Shipped", orderStatus);

            DeleteShipment(shipmentCreated.ShipmentId);
        }


        private static string GetOrderStatus()
        {
            var client = new RestClient($"{BigCommerceHelper.baseUrl}{bigCommerceOrderId}");
            var request = BigCommerceHelper.CreateNewGetRequest();

            var jsonResponse = client.Execute(request);
            Order parsedOrder = JsonConvert.DeserializeObject<Order>(jsonResponse.Content);

            string orderStatus = parsedOrder.status;

            return orderStatus;
        }


        private static ItemFulfillmentLine CreateItemFulfillmentLine(string itemFulfillmentId, string carrier, string sku, string trackingNumber, int quantity, int? kitId)
        {
            ItemFulfillmentLine itemFulfillmentLine = new ItemFulfillmentLine();

            itemFulfillmentLine.ItemFulfillmentId = itemFulfillmentId;
            itemFulfillmentLine.Carrier = carrier;
            itemFulfillmentLine.SKU = sku;
            itemFulfillmentLine.TrackingNumber = trackingNumber;
            itemFulfillmentLine.Quantity = quantity;
            itemFulfillmentLine.KitId = kitId;

            return itemFulfillmentLine;
        }


        private static void DeleteShipment(string shipmentId)
        {
            var client = new RestClient($"{BigCommerceHelper.baseUrl}{bigCommerceOrderId}/shipments/{shipmentId}");
            var request = BigCommerceHelper.CreateNewDeleteRequest();
            client.Execute(request);
        }
    }
}
