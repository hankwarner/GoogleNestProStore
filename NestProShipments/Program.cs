using System;
using Serilog;
using NestProShipments.Controllers;
using Newtonsoft.Json.Linq;
using NestProModels;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using NestProHelpers;

namespace NestProShipments
{
    public class Program
    {
        private const int awaitingShipmentStatusId = 9;
        private const int partiallyShippedStatusId = 3;
        private static string logPath = Environment.GetEnvironmentVariable("LOG_PATH");
        public static string teamsUrl = Environment.GetEnvironmentVariable("TEAMS_URL");

        static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logPath)
                .CreateLogger();
            Log.Information("Program started");

            try
            {
                // Pull orders in 'Awaiting Shipment' and 'Partially Shipped' status
                JArray ordersAwaitingShipment = BigCommerceController.GetOrdersByStatus(awaitingShipmentStatusId);
                JArray partiallyShippedOrders = BigCommerceController.GetOrdersByStatus(partiallyShippedStatusId);

                // Merge the two JArray's
                JArray allOrdersAwaitingShipments = MergeJArrays(ordersAwaitingShipment, partiallyShippedOrders);

                // Create shipments in Big Commerce if any new item fulfillments are found in NetSuite
                if (allOrdersAwaitingShipments.Count() == 0)
                {
                    Log.Information("No shipments to import");
                    return;
                }

                ImportShipmentsToBigCommerce(allOrdersAwaitingShipments);

                return;
            }
            catch (Exception ex)
            {
                Log.Error("Error: {@ex}", ex);
                string title = "Error in NestProShipments";
                string text = $"Error message: {ex.Message}";
                string color = "red";
                TeamsHelper teamsMessage = new TeamsHelper(title, text, color, teamsUrl);
                teamsMessage.LogToMicrosoftTeams(teamsMessage);
            }

        }


        private static JArray MergeJArrays(JArray jArrayOne, JArray jArrayTwo)
        {
            foreach (var item in jArrayTwo)
            {
                jArrayOne.Add(item);
            }

            return jArrayOne;
        }


        public static void ImportShipmentsToBigCommerce(JArray allOrdersAwaitingShipments)
        {
            try
            {
                foreach (var order in allOrdersAwaitingShipments)
                {
                    Order parsedOrder = JsonConvert.DeserializeObject<Order>(order.ToString());
                    BigCommerceController.bigCommerceOrderId = parsedOrder.id;
                    Log.Information($"Big Commerce Order Id: {BigCommerceController.bigCommerceOrderId}");

                    // Query NetSuite to get any matching item fulfillments
                    string netsuiteSalesOrderId = parsedOrder.staff_notes;

                    /* Get a list of NetSuite item fulfillment ids (partially shipped orders only) that already exist 
                    *  in Big Commerce to exclude so we do not create duplicate shipments.
                    */
                    List<string> importedItemFulfillmentIds = new List<string>();
                    if (parsedOrder.status.ToLower() == "partially shipped")
                    {
                        importedItemFulfillmentIds = BigCommerceController.GetImportedItemFulfillments();
                    }

                    var itemFulfillmentGroupsToImport = NetSuiteController.GetItemFulfillmentsNotImported(netsuiteSalesOrderId, importedItemFulfillmentIds);

                    // Skip line if no item fulfillments are found
                    if (itemFulfillmentGroupsToImport.Count() == 0)
                    {
                        Log.Information($"No item fulfillments to import.");
                        continue;
                    }

                    // Send each item fulfillment group to Big Commerce as a Shipment
                    foreach (var itemFulfillmentGroupToImport in itemFulfillmentGroupsToImport)
                    {
                        Log.Information($"Itfil ID: {itemFulfillmentGroupToImport.Key}");

                        BigCommerceController.currentItemFulfillment = itemFulfillmentGroupToImport;

                        Shipment shipmentToCreate = BigCommerceController.CreateShipmentRequest(itemFulfillmentGroupToImport);

                        // Big Commerce will throw exception if shipment does not have a tracking number
                        if (shipmentToCreate.TrackingNumber == "")
                        {
                            Log.Warning($"No tracking numbers found. Shipment not created.");
                            continue;
                        }

                        // Create the Shipment in Big Commerce
                        try
                        {
                            Shipment shipmentCreated = BigCommerceController.PostShipmentToBigCommerce(shipmentToCreate);
                            Log.Information($"shipment id {shipmentCreated.ShipmentId} created.");
                        }
                        catch(Exception ex)
                        {
                            string errorMessage = $"Error Posting Shipment To Big Commerce. Error: {ex}";
                            Log.Error(errorMessage);
                            string title = "Error in NestProShipments PostShipmentToBigCommerce";
                            string text = errorMessage;
                            string color = "yellow";
                            TeamsHelper teamsMessage = new TeamsHelper(title, text, color, Program.teamsUrl);
                            teamsMessage.LogToMicrosoftTeams(teamsMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error in ImportShipmentsToBigCommerce. Error: {ex}";
                Log.Error(errorMessage);
                string title = "Error in NestProShipments ImportShipmentsToBigCommerce";
                string text = errorMessage;
                string color = "red";
                TeamsHelper teamsMessage = new TeamsHelper(title, text, color, Program.teamsUrl);
                teamsMessage.LogToMicrosoftTeams(teamsMessage);
            }
        }
    }
}
