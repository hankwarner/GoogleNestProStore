using System;
using System.Net;
using RestSharp;
using NetSuiteOAuthHelper;

namespace NestProHelpers
{
    public class NetSuiteHelper
    {
        private static readonly string netsuiteUsername = Environment.GetEnvironmentVariable("NS_USER");
        private static readonly string netsuitePassword = Environment.GetEnvironmentVariable("NS_PASSWORD");
        private static readonly string netsuiteSandboxAccount = Environment.GetEnvironmentVariable("NS_SANDBOX_ACCOUNT");
        private static readonly string netsuiteAccount = Environment.GetEnvironmentVariable("NS_ACCT");
        private static readonly string netsuiteRole = Environment.GetEnvironmentVariable("NS_ROLE");

        // WebsiteOrderImporterRESTlet creds
        private static readonly string consumerKeyOrderImporter = Environment.GetEnvironmentVariable("OAUTH_ORDER_IMPORTER_CONSUMER_KEY");
        private static readonly string consumerSecretOrderImporter = Environment.GetEnvironmentVariable("OAUTH_ORDER_IMPORTER_CONSUMER_SECRET");
        private static readonly string tokenIDOrderImporter = Environment.GetEnvironmentVariable("OAUTH_ORDER_IMPORTER_TOKEN_ID");
        private static readonly string tokenSecretOrderImporter = Environment.GetEnvironmentVariable("OAUTH_ORDER_IMPORTER_TOKEN_SECRET");

        // CreateCustomerRESTlet creds
        private static readonly string consumerKeyCreateCustomer = Environment.GetEnvironmentVariable("OAUTH_CREATE_CUSTOMER_CONSUMER_KEY");
        private static readonly string consumerSecretCreateCustomer = Environment.GetEnvironmentVariable("OAUTH_CREATE_CUSTOMER_CONSUMER_SECRET");
        private static readonly string tokenIDCreateCustomer = Environment.GetEnvironmentVariable("OAUTH_CREATE_CUSTOMER_TOKEN_ID");
        private static readonly string tokenSecretCreateCustomer = Environment.GetEnvironmentVariable("OAUTH_CREATE_CUSTOMER_TOKEN_SECRET");

        public static string GetAthenaDBConnectionString(string dbName)
        {
            return Environment.GetEnvironmentVariable("NS_DB") + dbName;
        }


        public static void SetNetSuiteTestParameters(RestRequest request, string salesOrderId)
        {
            request.AddParameter("functionType", "create");
            request.AddParameter("salesOrderRecordId", salesOrderId);
        }


        public static RestRequest CreateNewRestletRequest(string jsonRequest, string restletUrl, string restletName)
        {
            string token;
            string tokenSecret;
            string consumerKey;
            string consumerSecret;

            if (restletName == "CreateCustomer")
            {
                token = tokenIDCreateCustomer;
                tokenSecret = tokenSecretCreateCustomer;
                consumerKey = consumerKeyCreateCustomer;
                consumerSecret = consumerSecretCreateCustomer;
            }
            else
            {
                token = tokenIDOrderImporter;
                tokenSecret = tokenSecretOrderImporter;
                consumerKey = consumerKeyOrderImporter;
                consumerSecret = consumerSecretOrderImporter;
            }

            Creds creds = new Creds(
                token,
                tokenSecret,
                consumerKey,
                consumerSecret,
                netsuiteAccount
            );

            var request = new RestRequest(Method.POST);

            string authHeader = OAuthBase.GenerateAuthorizationHeader(restletUrl, "POST", creds);

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", authHeader);
            request.AddParameter("application/json; charset=utf-8", jsonRequest, ParameterType.RequestBody);

            return request;
        }


        public static string GetStateByName(string name)
        {
            switch (name.ToUpper())
            {
                case "ALABAMA":
                    return "AL";

                case "ALASKA":
                    return "AK";

                case "ALBERTA":
                    return "AB";

                case "ARIZONA":
                    return "AZ";

                case "ARKANSAS":
                    return "AR";

                case "BRITISH COLUMBIA":
                    return "BC";

                case "CALIFORNIA":
                    return "CA";

                case "COLORADO":
                    return "CO";

                case "CONNECTICUT":
                    return "CT";

                case "DELAWARE":
                    return "DE";

                case "DISTRICT OF COLUMBIA":
                    return "DC";

                case "FEDERATED STATES OF MICRONESIA":
                    return "FM";

                case "FLORIDA":
                    return "FL";

                case "GEORGIA":
                    return "GA";

                case "GUAM":
                    return "GU";

                case "HAWAII":
                    return "HI";

                case "IDAHO":
                    return "ID";

                case "ILLINOIS":
                    return "IL";

                case "INDIANA":
                    return "IN";

                case "IOWA":
                    return "IA";

                case "KANSAS":
                    return "KS";

                case "KENTUCKY":
                    return "KY";

                case "LOUISIANA":
                    return "LA";

                case "MAINE":
                    return "ME";

                case "MAI":
                    return "MB";

                case "MARSHALL ISLANDS":
                    return "MH";

                case "MARYLAND":
                    return "MD";

                case "MASSACHUSETTS":
                    return "MA";

                case "MICHIGAN":
                    return "MI";

                case "MINNESOTA":
                    return "MN";

                case "MISSISSIPPI":
                    return "MS";

                case "MISSOURI":
                    return "MO";

                case "MONTANA":
                    return "MT";

                case "NEBRASKA":
                    return "NE";

                case "NEVADA":
                    return "NV";

                case "NEW BRUNSWICK":
                    return "NB";

                case "NEW HAMPSHIRE":
                    return "NH";

                case "NEW JERSEY":
                    return "NJ";

                case "NEW MEXICO":
                    return "NM";

                case "NEW YORK":
                    return "NY";

                case "NEWFOUNDLAND":
                    return "NL";

                case "NORTH CAROLINA":
                    return "NC";

                case "NORTH DAKOTA":
                    return "ND";

                case "NORTHERN MARIANA ISLANDS":
                    return "MP";

                case "NORTHWEST TERRITORIES":
                    return "NT";

                case "NOVA SCOTIA":
                    return "NS";

                case "NUNAVUT":
                    return "NU";

                case "OHIO":
                    return "OH";

                case "ONTARIO":
                    return "ON";

                case "OKLAHOMA":
                    return "OK";

                case "OREGON":
                    return "OR";

                case "PENNSYLVANIA":
                    return "PA";

                case "PRINCE EDWARD ISLAND":
                    return "PE";

                case "PUERTO RICO":
                    return "PR";

                case "QUEBEC":
                    return "QC";

                case "RHODE ISLAND":
                    return "RI";

                case "SASKATCHEWAN":
                    return "SK";

                case "SOUTH CAROLINA":
                    return "SC";

                case "SOUTH DAKOTA":
                    return "SD";

                case "TENNESSEE":
                    return "TN";

                case "TEXAS":
                    return "TX";

                case "UTAH":
                    return "UT";

                case "VERMONT":
                    return "VT";

                case "VIRGIN ISLANDS":
                    return "VI";

                case "VIRGINIA":
                    return "VA";

                case "WASHINGTON":
                    return "WA";

                case "WEST VIRGINIA":
                    return "WV";

                case "WISCONSIN":
                    return "WI";

                case "WYOMING":
                    return "WY";

                case "YUKON":
                    return "YT";
            }

            throw new Exception($"Invalid state name {name}");
        }
    }
}
