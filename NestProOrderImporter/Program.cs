using System;
using System.Collections.Generic;
using System.Linq;
using NestProHelpers;
using NestProModels;
using NestProOrderImporter.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace NestProOrderImporter
{
    public class Program
    {
        private static string netsuiteCustomerId { get; set; }
        private static string logPath = Environment.GetEnvironmentVariable("LOG_PATH");
        public static string errorLogsUrl = Environment.GetEnvironmentVariable("TEAMS_LOGS_ERROR");
        public static string nestTeamUrl = Environment.GetEnvironmentVariable("TEAMS_LOGS_B2B");
        private static List<int> ordersMissingProId { get; set; } = new List<int>();

        static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logPath)
                .CreateLogger();
            Log.Information("Program started");

            try
            {
                // Call the Orders API to get orders that are in 'Awaiting Fulfillment' status
                JArray ordersAwaitingFulfillment = BigCommerceController.GetOrdersAwaitingFulfillment();

                if (ordersAwaitingFulfillment.Count() == 0)
                {
                    Log.Information("No orders to import");
                    return;
                }

                ImportOrdersToNetSuite(ordersAwaitingFulfillment);

                if(ordersMissingProId.Count > 0)
                {
                    AlertB2BTeam();
                }

            }
            catch(Exception ex)
            {
                Log.Error($"Error: {ex}");
                string title = "Error in NestProOrderImporter";
                string text = $"Error message: {ex.Message}";
                string color = "red";
                TeamsHelper teamsMessage = new TeamsHelper(title, text, color, errorLogsUrl);
                teamsMessage.LogToMicrosoftTeams(teamsMessage);
            }
        }


        private static void ImportOrdersToNetSuite(JArray ordersAwaitingFulfillment)
        {
            try
            {
                foreach (var order in ordersAwaitingFulfillment)
                {
                    Order parsedOrder = JsonConvert.DeserializeObject<Order>(order.ToString());

                    if (parsedOrder.is_deleted == true)
                    {
                        Log.Information($"Skipping order {parsedOrder.customer_id} because it is marked as deleted/archived.");
                        continue;
                    }

                    BigCommerceController.customerId = parsedOrder.customer_id;
                    int bigCommerceOrderId = parsedOrder.id;
                    Log.Information($"bigCommerceOrderId Id {bigCommerceOrderId}");

                    // Get the shipping information
                    string shippingAddressUrl = parsedOrder.shipping_addresses.url;
                    ShippingAddress customerShippingAddress = BigCommerceController.GetCustomerShippingAddress(shippingAddressUrl);

                    // Format the request object to send to CreateCustomerRESTlet
                    OrderToImport netsuiteRequest = NetSuiteController.CreateNetSuiteRequest(parsedOrder, customerShippingAddress);

                    if(netsuiteRequest.NestProId == "")
                    {
                        // We alert these to B2B so they can contact the customer
                        ordersMissingProId.Add(bigCommerceOrderId);
                        continue;
                    }

                    netsuiteCustomerId = NetSuiteController.GetNetSuiteCustomerId(netsuiteRequest);
                    netsuiteRequest.CustomerId = Convert.ToInt32(netsuiteCustomerId);

                    // Call the Products API to get the products on the order
                    string productsUrl = parsedOrder.products.url;
                    netsuiteRequest.Items = BigCommerceController.GetProductsOnOrder(productsUrl);

                    NetSuiteController.SetNetSuiteItemIdAndPersonalItemFlag(netsuiteRequest);

                    // Import order to Netsuite
                    string netsuiteOrderId = NetSuiteController.ImportOrderToNetSuite(netsuiteRequest);

                    // Set the Big Commerce status to 'awaiting shipment' and add the NetSuite order ID to 'staff notes'
                    BigCommerceController.SetOrderStatus(bigCommerceOrderId, netsuiteOrderId);
                }
            }
            catch(Exception ex)
            {
                Log.Error($"Error: {ex}");
                string title = "Error in ImportOrdersToNetSuite";
                string text = $"Error message: {ex.Message}";
                string color = "red";
                TeamsHelper teamsMessage = new TeamsHelper(title, text, color, errorLogsUrl);
                teamsMessage.LogToMicrosoftTeams(teamsMessage);
            }
        }

        private static void AlertB2BTeam()
        {
            var ordersMissingProIdJSON = JsonConvert.SerializeObject(ordersMissingProId);
            var warningMessage = $"The following orders cannot import because the customer is missing a Nest Pro ID: {ordersMissingProIdJSON}";

            Log.Warning(warningMessage);

            string title = "Error Importing Nest Pro Orders";
            string text = warningMessage;
            string color = "red";
            TeamsHelper teamsMessage = new TeamsHelper(title, text, color, nestTeamUrl);
            teamsMessage.LogToMicrosoftTeams(teamsMessage);
        }
    }
}

