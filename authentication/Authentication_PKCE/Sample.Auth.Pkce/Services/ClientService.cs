using System;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using Sample.Auth.Pkce.Models;

namespace Sample.Auth.Pkce.Services
{
    public class ClientService : BaseService
    {
        public readonly string _tokenType, _accessToken, _apiBaseUrl, _logPath;
        private Uri _apiFullUrl;

        public ClientService(string openApiBaseUrl, string accessToken, string tokenType)
        {
            _apiBaseUrl = openApiBaseUrl;
            _accessToken = accessToken;
            _tokenType = tokenType;
            _logPath = Path.GetTempFileName();
        }

        /// <summary>
        /// Run Get command
        /// </summary>
        private dynamic HttpGet(string getURL)
        {
            _apiFullUrl = new Uri(new Uri(_apiBaseUrl), getURL);
            var request = new HttpRequestMessage(HttpMethod.Get, _apiFullUrl);
            request.Headers.Authorization = GetAuthorizationHeader(_accessToken, _tokenType);

            try
            {
                return Send<dynamic>(request);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Error recorded in GET request - {_apiFullUrl}", ex);
            }
        }

        /// <summary>
        /// Run Get command
        /// </summary>
        private dynamic HttpPost(string getURL, HttpContent parameters)
        {
            _apiFullUrl = new Uri(new Uri(_apiBaseUrl), getURL);
            var request = new HttpRequestMessage(HttpMethod.Post, _apiFullUrl);
            request.Headers.Authorization = GetAuthorizationHeader(_accessToken, _tokenType);
            request.Content = parameters;

            try
            {
                return Send<dynamic>(request);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Error recorded in POST request - {_apiFullUrl} - {ex.Message}", ex);
            }
        }

        public dynamic GetClient()
        {
            return HttpGet("port/v1/clients/me");
        }

        public dynamic GetUser()
        {
            return HttpGet("port/v1/users/me");
        }
        public dynamic GetInstruments(string keyword, string assetType)
        {
            return HttpGet($"ref/v1/instruments?KeyWords={keyword}&AssetTypes={assetType}");
        }

        public dynamic GetOrders()
        {
            return HttpGet($"port/v1/orders/me?fieldGroups=DisplayAndFormat");
        }

        public dynamic PlaceOrder(Order inputOrder)
        {
            var objAsJson = JsonConvert.SerializeObject(inputOrder);
            HttpContent content = new StringContent(objAsJson, Encoding.UTF8, "application/json");
            return HttpPost("trade/v2/orders", content);
        }

        public void WriteFile(dynamic input)
        {
            var logWriter = new StreamWriter(_logPath, append: true);

            // The unescaped canonical representation of the Uri instance.
            logWriter.WriteLine($"Request: { _apiFullUrl }");
            logWriter.WriteLine("Response: ");
            logWriter.WriteLine(JsonConvert.SerializeObject(new { Response = input }, Formatting.Indented));
            logWriter.WriteLine();

            logWriter.Dispose();

            Console.WriteLine($"API call executed successfully. Output in the log file: {_logPath}");
        }
    }
}
