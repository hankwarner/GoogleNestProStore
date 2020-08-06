using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using RestSharp;
using NestProModels;
using NestProHelpers;
using Newtonsoft.Json;
using Serilog;
using Polly;
using System.Data.SqlClient;

namespace NestProShipments.Controllers
{
    public class BigCommerceController
    {
        public static int bigCommerceOrderId { get; set; }
        private static List<int?> promoBundleKitIds = new List<int?>() 
        {
            3854675, 3854676, 3854677, 3854678, 3898517,
            3898516, 3898518, 3898519, 3898520, 3898521, 3898522, 3898523,
            3898524, 3898525, 3898526, 3898527, 3898528, 3898529, 3898530,
            3898531, 3898532, 3898533, 4141215
        };
        public static IGrouping<string, ItemFulfillmentLine> currentItemFulfillment { get; set; }
        private static string getOrdersByStatusBaseUrl = $"{BigCommerceHelper.baseUrl}?status_id=";

        public static JArray GetOrdersByStatus(int statusId)
        {
            RestClient client = new RestClient($"{getOrdersByStatusBaseUrl}{statusId}");
            RestRequest request = BigCommerceHelper.CreateNewGetRequest();

            string jsonResponse = client.Execute(request).Content;

            // Handles if there are no orders for a given status
            if (jsonResponse == "") return new JArray();

            return BigCommerceHelper.ParseApiResponse(jsonResponse);
        }


        public static List<Product> GetProductsOnOrder(string productsUrl)
        {
            var getProductsOnOrderRetryPolicy = Policy.Handle<SqlException>()
                .WaitAndRetry(4, _ => TimeSpan.FromSeconds(30), (ex, ts, count, context) =>
                {
                    string errorMessage = "Error in GetProductsOnOrder";
                    Log.Warning(ex, $"{errorMessage} . Retrying...");

                    if (count == 4)
                    {
                        Log.Error(ex, errorMessage);
                    }
                });

            return getProductsOnOrderRetryPolicy.Execute(() =>
            {
                RestClient client = new RestClient(productsUrl);
                RestRequest request = BigCommerceHelper.CreateNewGetRequest();

                IRestResponse productsApiResponse = client.Execute(request);
                JArray parsedProductsApiResponse = BigCommerceHelper.ParseApiResponse(productsApiResponse.Content);
                List<Product> productsOnOrder = new List<Product>();

                foreach (var product in parsedProductsApiResponse)
                {
                    Product parsedProduct = JsonConvert.DeserializeObject<Product>(product.ToString());
                    productsOnOrder.Add(parsedProduct);
                }

                Log.Information("productsOnOrder {@productsOnOrder}", productsOnOrder);

                return productsOnOrder;
            });
        }


        public static List<string> GetImportedItemFulfillments()
        {
            var getImportedItemFulfillmentsRetryPolicy = Policy.Handle<SqlException>()
                .WaitAndRetry(4, _ => TimeSpan.FromSeconds(30), (ex, ts, count, context) =>
                {
                    string errorMessage = "Error in GetImportedItemFulfillments";
                    Log.Warning(ex, $"{errorMessage} . Retrying...");

                    if (count == 4)
                    {
                        Log.Error(ex, errorMessage);
                    }
                });

            return getImportedItemFulfillmentsRetryPolicy.Execute(() =>
            {
                RestClient client = new RestClient($"{BigCommerceHelper.baseUrl}/{bigCommerceOrderId}/shipments");
                RestRequest request = BigCommerceHelper.CreateNewGetRequest();

                IRestResponse shipmentsApiResponse = client.Execute(request);
                JArray parsedShipmentsApiResponse = BigCommerceHelper.ParseApiResponse(shipmentsApiResponse.Content);
                List<string> shipmentsOnOrder = new List<string>();

                foreach (var shipment in parsedShipmentsApiResponse)
                {
                    Shipment parsedShipment = JsonConvert.DeserializeObject<Shipment>(shipment.ToString());
                    string netsuiteItemFulfillmentId = parsedShipment.NetSuiteItemFulfillmentId;
                    shipmentsOnOrder.Add(netsuiteItemFulfillmentId);
                }

                Log.Information("shipmentsOnOrder {@shipmentsOnOrder}", shipmentsOnOrder);

                return shipmentsOnOrder;
            });
        }


        public static Shipment CreateShipmentRequest(IGrouping<string, ItemFulfillmentLine> itemFulfillmentGroupToImport)
        {
            // Get the item fulfillment id, tracking # and carrier from the first item since they are all the same
            string itemFulfillmentId = itemFulfillmentGroupToImport.First().ItemFulfillmentId;
            string trackingNumber = GetTrackingNumbers(itemFulfillmentId);
            string carrier = GetShippingProviderFromCarrierName(itemFulfillmentGroupToImport.First().Carrier);

            // Get the id of the shipping address (there will only be one per order)
            int orderAddressId = GetOrderAddressId(bigCommerceOrderId);

            // Create a new Shipment, then add the items
            var shipmentToCreate = new Shipment(carrier, orderAddressId, itemFulfillmentId, trackingNumber);

            AddItemsToShipment(shipmentToCreate, itemFulfillmentGroupToImport);

            return shipmentToCreate;
        }


        public static void AddItemsToShipment(Shipment shipmentToCreate, IGrouping<string, ItemFulfillmentLine> itemFulfillmentGroupToImport)
        {
            foreach (var itemFulfillmentToImport in itemFulfillmentGroupToImport)
            {
                // Skip the line if it has already been added via a kit bundle
                if (itemFulfillmentToImport.AddedToKit) continue;

                bool isKit = itemFulfillmentToImport.KitId == null ? false : true;
                
                string sku;

                // For kits with different types of items, such as promo kits, the skus can't be parsed
                if (promoBundleKitIds.Contains(itemFulfillmentToImport.KitId))
                {
                    sku = NetSuiteController.GetSKUOfParentKit(itemFulfillmentToImport.KitId);
                }
                else
                {
                    sku = itemFulfillmentToImport.SKU.Split('_')[0];
                }

                bool isPersonal = itemFulfillmentToImport.IsPersonal;

                int orderProductId = GetOrderProductId(sku, isKit, isPersonal, bigCommerceOrderId);
                int itemQuantity = GetItemQuantity(itemFulfillmentToImport);

                Item item = new Item(orderProductId, itemQuantity);
                shipmentToCreate.Items.Add(item);
            }
        }


        public static Shipment PostShipmentToBigCommerce(Shipment shipmentToCreate)
        {
            RestClient client = new RestClient($"{BigCommerceHelper.baseUrl}/{bigCommerceOrderId}/shipments");

            string jsonRequest = JsonConvert.SerializeObject(shipmentToCreate);
            RestRequest request = BigCommerceHelper.CreateNewPostRequest(jsonRequest);

            IRestResponse shipmentApiResponse = client.Execute(request);
            
            // If there is an error, the API will send the response as a JArray instead of JObject
            try
            {
                Shipment shipmentCreated = JsonConvert.DeserializeObject<Shipment>(shipmentApiResponse.Content);
                Log.Information($"Shipment created in Big Commerce. Shipment id: {shipmentCreated.ShipmentId}, Big Commerce order id: {bigCommerceOrderId}");

                return shipmentCreated;
            }
            catch (Exception ex)
            {
                JArray bigCommerceErrorResponse = BigCommerceHelper.ParseApiResponse(shipmentApiResponse.Content);

                string errorMessage = $"Invalid shipment request. Error: {bigCommerceErrorResponse}";
                Log.Error($"Error in PostShipmentToBigCommerce. {errorMessage}");

                throw new Exception(errorMessage);
            }
        }


        public static string GetTrackingNumbers(string itemFulfillmentId)
        {
            List<string> trackingNumbers = NetSuiteController.GetTrackingNumbersByItemFulfillment(itemFulfillmentId);
            string trackingNumber = "";

            if (trackingNumbers.Count() == 1)
            {
                trackingNumber = trackingNumbers[0];
            }
            // Handles if an item fulfillment has multiple tracking numbers
            else if (trackingNumbers.Count() > 1)
            {
                // Big Commerce has a character limit on tracking numbers, so we can only send the first two
                trackingNumber = trackingNumbers[0] + " " + trackingNumbers[1];
            }

            return trackingNumber;
        }


        public static int GetItemQuantity(ItemFulfillmentLine itemFulfillmentLine)
        {
            /* NetSuite splits kits into individual lines of member items on the item fulfillment,
             * but Big Commerce ships at the kit level. If item is part of a kit, 
            *  use the kit quantity instead of the individual item quantity.
            */
            var kitId = itemFulfillmentLine.KitId;
            
            if (kitId != null)
            {
                var kit = BuildKit(Convert.ToInt32(kitId));

                return  GetKitShippedQuantity(kit);
            }

            return itemFulfillmentLine.Quantity;
        }


        public static int GetOrderAddressId(int bigCommerceOrderId)
        {
            string shippingAddressUrl = $"{BigCommerceHelper.baseUrl}/{bigCommerceOrderId}/shippingaddresses";
            ShippingAddress shippingAddress = GetCustomerShippingAddress(shippingAddressUrl);

            int orderAddressId = shippingAddress.id;

            return orderAddressId;
        }


        public static ShippingAddress GetCustomerShippingAddress(string shippingAddressUrl)
        {
            var getCustomerShippingAddressRetryPolicy = Policy.Handle<SqlException>()
                .WaitAndRetry(4, _ => TimeSpan.FromSeconds(30), (ex, ts, count, context) =>
                {
                    string errorMessage = "Error in GetCustomerShippingAddress";
                    Log.Warning(ex, $"{errorMessage} . Retrying...");

                    if (count == 4)
                    {
                        Log.Error(ex, errorMessage);
                    }
                });

            return getCustomerShippingAddressRetryPolicy.Execute(() =>
            {
                RestClient client = new RestClient(shippingAddressUrl);
                RestRequest request = BigCommerceHelper.CreateNewGetRequest();

                IRestResponse shippingAddressApiResponse = client.Execute(request);
                JArray parsedShippingAddressApiResponse = BigCommerceHelper.ParseApiResponse(shippingAddressApiResponse.Content);

                // There will only be one shipping address, so we get the address at the first index
                ShippingAddress parsedShippingAddress = JsonConvert.DeserializeObject<ShippingAddress>(parsedShippingAddressApiResponse[0].ToString());
                Log.Information("Shipping address id: {@shippingAddressId}", parsedShippingAddress.id);

                return parsedShippingAddress;
            });
        }


        public static int GetOrderProductId(string itemSku, bool isKit, bool isPersonal, int bigCommerceOrderId)
        {
            string productsUrl = $"{BigCommerceHelper.baseUrl}/{bigCommerceOrderId}/Products";
            List<Product> products = GetProductsOnOrder(productsUrl);

            int productId = isKit ? GetKitProductId(products, itemSku) : GetSingleItemProductId(products, itemSku, isPersonal);

            return productId;
        }


        public static int GetKitProductId(List<Product> products, string itemSku)
        {
            foreach (var product in products)
            {
                if (product.Sku == itemSku && (ParsePackQuantity(product) > 1 || product.Name.ToLower().Contains("bundle") || (product.Name.Contains("Google Nest Hello Doorbell with") && product.Name.Contains("Mars"))))
                {
                    return product.id;
                }
            }
            throw new Exception($"Invalid Sku {itemSku}");
        }


        public static int GetSingleItemProductId(List<Product> products, string itemSku, bool isPersonal)
        {
            foreach (var product in products)
            {
                if (product.Sku == itemSku && ParsePackQuantity(product) <= 1)
                {
                    // Check if item is personal
                    if (isPersonal == isItemPersonal(product.ProductOptions))
                    {
                        return product.id;
                    }
                }
            }
            throw new Exception($"Invalid Sku {itemSku}");
        }


        public static int GetKitQuantity(string itemSku)
        {
            string productsUrl = $"{BigCommerceHelper.baseUrl}/{bigCommerceOrderId}/Products";
            List<Product> products = GetProductsOnOrder(productsUrl);

            foreach (var product in products)
            {
                if (product.Sku == itemSku)
                {
                    /* Kits and single items can have the same SKU, so we must also check the pack 
                     * quantity to get the correct kit quantity.
                     */
                    int kitQuantity = ParsePackQuantity(product);

                    if (kitQuantity > 1)
                    {
                        return kitQuantity;
                    }
                }
            }
            throw new Exception($"Invalid Sku {itemSku}");
        }


        public static int ParsePackQuantity(Product product)
        {
            int packQuantity = 0;

            try
            {
                foreach (var option in product.ProductOptions)
                {
                    if (option.display_name.ToLower().Contains("pack"))
                    {
                        // Pack quantity will always be the first charactor of the display value
                        String[] seperators = { " ", "-", "/" };
                        String[] displayValue = option.display_value.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
                        packQuantity = int.Parse(displayValue[0]);
                    }
                }
                return packQuantity;
            }
            catch(Exception ex)
            {
                string errorMessage = $"Error parsing pack quantity for product id {product.id} " +
                                      $"on Big Commerce order id {bigCommerceOrderId}. Error: {ex}";
                Log.Error(errorMessage);
                throw new Exception(errorMessage);
            }
        }


        public static bool isItemPersonal(List<ProductOptions> productOptions)
        {
            bool isPersonal = false;

            try
            {
                foreach (var option in productOptions)
                {
                    if (option.display_value.ToLower().Contains("personal"))
                    {
                        isPersonal = true;
                    }
                }
                return isPersonal;
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error parsing isPersonal value on Big Commerce order id {bigCommerceOrderId}. Error: {ex}";
                Log.Error(errorMessage);
                throw new Exception(errorMessage);
            }
        }


        public static string GetShippingProviderFromCarrierName(string carrier)
        {
            string shippingProvider = "";

            if (carrier.ToLower().Contains("ups"))
            {
                shippingProvider = "ups";
            }
            else if (carrier.ToLower().Contains("usps"))
            {
                shippingProvider = "usps";
            }
            else if (carrier.ToLower().Contains("fedex"))
            {
                shippingProvider = "fedex";
            }

            return shippingProvider;
        }


        public static Kit BuildKit(int kitId)
        {
            var kit = new Kit(Convert.ToInt32(kitId));

            // Look at all items on the fulfillment and get items that are in the kit id
            foreach (var itemFullfillmentLine in currentItemFulfillment)
            {
                if (itemFullfillmentLine.KitId == kitId)
                {
                    int itemQuantity = itemFullfillmentLine.Quantity;

                    // Add item to kit and increase kit member quantity
                    kit.TotalItemQuantity += itemQuantity;

                    // Mark the line as added to a kit
                    itemFullfillmentLine.AddedToKit = true;
                }
            }

            return kit;
        }


        public static int GetKitShippedQuantity(Kit kit)
        {
            int sumOfKitItems = NetSuiteController.GetSumOfKitMembers(kit.ID);

            // Divide the # of items in the shipped kit on the item fulfillment by the # of items that are in that kit
            int kitShippedQuantity = kit.TotalItemQuantity / sumOfKitItems;

            return kitShippedQuantity;
        }
    }
}
