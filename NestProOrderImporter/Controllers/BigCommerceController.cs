using System;
using System.Collections.Generic;
using NestProModels;
using NestProHelpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Serilog;

namespace NestProOrderImporter.Controllers
{
    public class BigCommerceController
    {
        public static int awaitingShipmentStatusId = 9;
        public static int awaitingFulfillmentStatusId = 11;
        public static int customerId { get; set; }
        public static int orderId { get; set; }

        public static JArray GetOrdersAwaitingFulfillment()
        {
            RestClient client = new RestClient($"{BigCommerceHelper.baseUrl}?status_id={awaitingFulfillmentStatusId}");
            RestRequest request = BigCommerceHelper.CreateNewGetRequest();

            string jsonResponse = client.Execute(request).Content;
            JArray parsedResponse;

            // Handles if there are no orders for a given status
            if (jsonResponse == "")
            {
                parsedResponse = new JArray();
            }
            else
            {
                parsedResponse = BigCommerceHelper.ParseApiResponse(jsonResponse);
            }

            return parsedResponse;
        }

        public static ShippingAddress GetCustomerShippingAddress(string shippingAddressUrl)
        {
            RestClient client = new RestClient(shippingAddressUrl);
            RestRequest request = BigCommerceHelper.CreateNewGetRequest();

            IRestResponse shippingAddressApiResponse = client.Execute(request);
            JArray parsedShippingAddressApiResponse = BigCommerceHelper.ParseApiResponse(shippingAddressApiResponse.Content);

            ShippingAddress parsedShippingAddress = JsonConvert.DeserializeObject<ShippingAddress>(parsedShippingAddressApiResponse[0].ToString());
            Log.Information("parsedShippingAddress {@parsedShippingAddress}", parsedShippingAddress);

            return parsedShippingAddress;
        }

        public static List<Product> GetProductsOnOrder(string productsUrl)
        {
            RestClient client = new RestClient(productsUrl);
            RestRequest request = BigCommerceHelper.CreateNewGetRequest();

            IRestResponse productsApiResponse = client.Execute(request);
            JArray parsedProductsApiResponse = BigCommerceHelper.ParseApiResponse(productsApiResponse.Content);
            List<Product> productsOnOrder = new List<Product>();

            foreach(var product in parsedProductsApiResponse)
            {
                Product parsedProduct = JsonConvert.DeserializeObject<Product>(product.ToString());

                // Map rate and amount values to the NetSuite expected names
                parsedProduct.Rate = Convert.ToDouble(parsedProduct.BasePrice);
                parsedProduct.Amount = Convert.ToDouble(parsedProduct.BaseTotal);

                productsOnOrder.Add(parsedProduct);
            }

            Log.Information("productsOnOrder {@productsOnOrder}", productsOnOrder);

            return productsOnOrder;
        }


        public static BigCommerceCustomer GetCustomerData()
        {
            string customerUrl = $"https://api.bigcommerce.com/stores/v68kp5ifsa/v2/customers/{customerId}";

            RestClient client = new RestClient(customerUrl);
            RestRequest request = BigCommerceHelper.CreateNewGetRequest();

            IRestResponse customerApiResponse = client.Execute(request);
            BigCommerceCustomer parsedCustomer = JsonConvert.DeserializeObject<BigCommerceCustomer>(customerApiResponse.Content);

            return parsedCustomer;
        }


        public static void SetOrderStatus(int bigCommerceOrderId, string netsuiteOrderId)
        {
            OrderValues orderValues = new OrderValues(awaitingShipmentStatusId, netsuiteOrderId);
            string jsonRequest = JsonConvert.SerializeObject(orderValues);

            RestClient client = new RestClient(BigCommerceHelper.baseUrl);
            RestRequest request = BigCommerceHelper.CreateNewPutRequest(bigCommerceOrderId, jsonRequest);

            IRestResponse ordersApiResponse = client.Execute(request);

            if (ordersApiResponse.StatusCode.ToString() != "OK")
            {
                throw new Exception($"Error in setting order status to Awaiting Shipment. Big Commerce order id {bigCommerceOrderId}");
            }
        }


        public static string GetNestProId()
        {
            RestClient client = new RestClient($@"https://api.bigcommerce.com/stores/v68kp5ifsa/v2/customers/{customerId}");
            RestRequest request = BigCommerceHelper.CreateNewGetRequest();

            IRestResponse customerApiResponse = client.Execute(request);
            BigCommerceCustomer parsedCustomer = JsonConvert.DeserializeObject<BigCommerceCustomer>(customerApiResponse.Content);
            string nestProId = "";

            if(parsedCustomer.form_fields == null)
            {
                string errorMessage = $"No Nest Pro Id found for Big Commerce customer {customerId}";
                string title = "Error in ImportOrdersToNetSuite";
                string color = "red";
                TeamsHelper teamsMessage = new TeamsHelper(title, errorMessage, color, Program.errorLogsUrl);
                teamsMessage.LogToMicrosoftTeams(teamsMessage);
            }
            else
            {
                foreach (var formField in parsedCustomer.form_fields)
                {
                    if (formField.name.ToLower().Contains("nest pro id"))
                    {
                        nestProId = formField.value;
                    }
                }
            }

            return nestProId;
        }
    }

}
