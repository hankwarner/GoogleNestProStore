using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using NestProModels;
using NestProHelpers;
using Dapper;
using Serilog;
using Polly;

namespace NestProShipments.Controllers
{
    public class NetSuiteController
    {
        private static string dbName = Environment.GetEnvironmentVariable("NS_DB");

        public static IEnumerable<IGrouping<string, ItemFulfillmentLine>> GetItemFulfillmentsNotImported(string netsuiteSalesOrderId, List<string> importedItemFulfillmentIds)
        {
            var getItemFulfillmentsNotImportedRetryPolicy = Policy.Handle<SqlException>()
                .WaitAndRetry(4, _ => TimeSpan.FromSeconds(30), (ex, ts, count, context) =>
                {
                    string errorMessage = "Error in GetItemFulfillmentsNotImported";
                    Log.Warning(ex, $"{errorMessage} . retrying...");

                    if (count == 4)
                    {
                        Log.Error(ex, errorMessage);
                    }
                });

            return getItemFulfillmentsNotImportedRetryPolicy.Execute(() =>
            {
                using (var conn = new SqlConnection(NetSuiteHelper.GetAthenaDBConnectionString(dbName)))
                {
                    conn.Open();

                    string query = "SELECT itfil.TRANSACTION_ID ItemFulfillmentId" +
                                        ", (itfilLine.ITEM_COUNT * -1) Quantity" +
                                        ", itfilLine.KIT_ID KitId" +
                                        ", item.SKU " +
                                        ", itfil.ACTUAL_SHIPPING_CARRIER Carrier " +
                                        ", CASE " +
                                            "WHEN soLine.NEST_PRO_PERSONAL_ITEM = 'F' THEN 'false' " +
                                            "ELSE 'true' end as 'IsPersonal' " +
                                    "FROM [NetSuite].[data].[TRANSACTIONS] itfil " +
                                    "JOIN [NetSuite].[data].[TRANSACTION_LINES] itfilLine " +
                                        "ON itfil.TRANSACTION_ID = itfilLine.TRANSACTION_ID " +
                                        "AND itfil.TRANSACTION_TYPE = 'Item Fulfillment' " +
                                    "JOIN [NetSuite].[data].[TRANSACTION_LINKS] link " +
                                        "ON itfil.TRANSACTION_ID = link.APPLIED_TRANSACTION_ID " +
                                        "AND itfilLine.TRANSACTION_LINE_ID = link.APPLIED_TRANSACTION_LINE_ID " +
                                    "JOIN [NetSuite].[data].[TRANSACTIONS] so " +
                                        "ON so.TRANSACTION_ID = link.ORIGINAL_TRANSACTION_ID " +
                                    "JOIN [NetSuite].[data].[TRANSACTION_LINES] soLine " +
                                        "ON soLine.TRANSACTION_ID = so.TRANSACTION_ID " +
                                        "AND so.TRANSACTION_TYPE = 'Sales Order' " +
                                        "AND soLine.TRANSACTION_LINE_ID = link.ORIGINAL_TRANSACTION_LINE_ID " +
                                    "JOIN [NetSuite].[data].[ITEMS] item " +
                                        "ON item.ITEM_ID = itfilLine.ITEM_ID " +
                                    $"WHERE so.TRANSACTION_ID = '{netsuiteSalesOrderId}' " +
                                    "AND itfil.TRANSACTION_ID not in @importedItemFulfillmentIds";

                    List<ItemFulfillmentLine> itemFulfillments = conn.Query<ItemFulfillmentLine>(query, new { importedItemFulfillmentIds }, commandTimeout: 500).ToList();

                    conn.Close();

                    var groupedItemFulfillments = itemFulfillments.GroupBy(itfil => itfil.ItemFulfillmentId);

                    return groupedItemFulfillments;
                }
            });
        }


        public static List<string> GetTrackingNumbersByItemFulfillment(string itemFulfillmentId)
        {
            using (var conn = new SqlConnection(NetSuiteHelper.GetAthenaDBConnectionString(dbName)))
            {
                try
                {
                    conn.Open();

                    string query = "SELECT TRACKING_NUMBER " +
                                   "FROM NetSuite.data.SHIPMENT_PACKAGES " +
                                   $"WHERE ITEM_FULFILLMENT_ID = '{itemFulfillmentId}' " +
                                   "AND TRACKING_NUMBER is not null";

                    List<string> trackingNumbers = conn.Query<string>(query, commandTimeout: 500).ToList();

                    conn.Close();

                    return trackingNumbers;
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Error in GetTrackingNumbersByItemFulfillment. Error: {ex}";
                    Log.Error(errorMessage);
                    throw new Exception(errorMessage);
                }
            }
        }


        public static int GetSumOfKitMembers(int kitId)
        {
            var retryPolicy = Policy.Handle<SqlException>()
                .WaitAndRetry(4, _ => TimeSpan.FromSeconds(30), (ex, ts, count, context) =>
                {
                    string errorMessage = "Error in GetKitMembers";
                    Log.Warning(ex, $"{errorMessage} . retrying...");

                    if (count == 4)
                    {
                        Log.Error(ex, errorMessage);
                    }
                });

            return retryPolicy.Execute(() =>
            {
                using (var conn = new SqlConnection(NetSuiteHelper.GetAthenaDBConnectionString(dbName)))
                {
                    try
                    {
                        conn.Open();

                        string query = "SELECT SUM(itgp.QUANTITY) SumOfKitItems " +
                                       "FROM NetSuite.data.ITEMS it " +
                                       "JOIN NetSuite.data.ITEM_GROUP itgp " +
                                       "ON it.ITEM_ID = itgp.PARENT_ID " +
                                       $"WHERE it.ITEM_ID = '{kitId}' " +
                                       "GROUP BY itgp.PARENT_ID";

                        int sumOfKitItems = conn.QuerySingle<int>(query, commandTimeout: 500);

                        return sumOfKitItems;

                    }
                    catch (Exception ex)
                    {
                        var errorMessage = $"Error in GetKitMembers. Error: {ex}";
                        Log.Error(errorMessage);
                        throw;
                    }
                }
            });
        }


        public static string GetSKUOfParentKit(int? kitId)
        {
            var retryPolicy = Policy.Handle<SqlException>()
                .WaitAndRetry(4, _ => TimeSpan.FromSeconds(30), (ex, ts, count, context) =>
                {
                    string errorMessage = "Error in GetSKUOfParentKit";
                    Log.Warning(ex, $"{errorMessage} . retrying...");

                    if (count == 4)
                    {
                        Log.Error(ex, errorMessage);
                    }
                });

            return retryPolicy.Execute(() =>
            {
                using (var conn = new SqlConnection(NetSuiteHelper.GetAthenaDBConnectionString(dbName)))
                {
                    try
                    {
                        conn.Open();

                        string query = "SELECT SKU " +
                                       "FROM NetSuite.data.ITEMS " +
                                       $"WHERE ITEM_ID = '{kitId}'";

                        string kitSKU = conn.QuerySingle<string>(query, commandTimeout: 500);

                        return kitSKU;

                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error in GetSKUOfParentKit.");
                        throw;
                    }
                }
            });
        }
    }
}
