using System;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace NestProHelpers
{
    public class BigCommerceHelper
    {
        public static string baseUrl = @"https://api.bigcommerce.com/stores/v68kp5ifsa/v2/orders";
        private static string bigCommerceToken = Environment.GetEnvironmentVariable("BIG_COMMERCE_TOKEN");
        private static string bigCommerceClient = Environment.GetEnvironmentVariable("BIG_COMMERCE_CLIENT");


        public static RestRequest CreateNewGetRequest()
        {
            RestRequest request = new RestRequest(Method.GET);
            SetBigCommerceHeaders(request);

            return request;
        }


        public static RestRequest CreateNewPostRequest(string jsonRequest)
        {
            RestRequest request = new RestRequest(Method.POST);
            SetBigCommerceHeaders(request);
            request.AddParameter("application/json; charset=utf-8", jsonRequest, ParameterType.RequestBody);

            return request;
        }


        public static RestRequest CreateNewPutRequest(int orderId, string jsonRequest)
        {
            RestRequest request = new RestRequest($"{orderId}", Method.PUT);
            SetBigCommerceHeaders(request);
            request.AddParameter("application/json; charset=utf-8", jsonRequest, ParameterType.RequestBody);

            return request;
        }


        public static RestRequest CreateNewDeleteRequest()
        {
            RestRequest request = new RestRequest(Method.DELETE);
            SetBigCommerceHeaders(request);

            return request;
        }


        public static void SetBigCommerceHeaders(RestRequest request)
        {
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-Auth-Token", bigCommerceToken);
            request.AddHeader("X-Auth-Client", bigCommerceClient);
        }


        public static JArray ParseApiResponse(string jsonResponse)
        {
            if (jsonResponse.Contains("X-Auth-Client and X-Auth-Token headers should have correct format"))
            {
                throw new Exception("Request was rejected by Big Commerce Api. Check headers to ensure X-Auth-Token and X-Auth-Client are correct.");
            }

            JArray parsedResponse = JArray.Parse(jsonResponse);
            return parsedResponse;
        }


        public static string GetShippingMethodName(string shippingMethod)
        {
            string shippingMethodName = shippingMethod;

            if (!shippingMethod.ToLower().Contains("priority"))
            {
                shippingMethodName = "Standard";
            }

            return shippingMethodName;
        }
    }
}
