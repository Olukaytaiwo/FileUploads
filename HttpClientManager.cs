using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using PartnersPlatform.Interface.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PartnersPlatform.Utility
{
    public class HttpClientManager : IHttpClientManager
    {
        HttpClient _client;
        private string _token;

        public HttpClientManager(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072 | SecurityProtocolType.Tls12;
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));            
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            _client = new HttpClient();
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072 | SecurityProtocolType.Tls12;
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //_client.BaseAddress = new Uri(url);
            HttpResponseMessage responseMessage = await _client.GetAsync(url);
            return responseMessage;
        }

        public async Task<HttpResponseMessage> Post(string payload, string url)
        {
            _client = new HttpClient();
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072 | SecurityProtocolType.Tls12;
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.BaseAddress = new Uri(url);
            HttpResponseMessage responseMessage = null;
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            responseMessage = await _client.PostAsync(url, content);
            return responseMessage;
        }

        public async Task<HttpResponseMessage> PostwithBearerToken(string url, string token, string payload)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072 | SecurityProtocolType.Tls12;
            using (var client = new HttpClient())
            {
                if (!string.IsNullOrWhiteSpace(token))
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                }
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072 | SecurityProtocolType.Tls12;
                StringContent content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                return response;
            }
        }


        public async Task<HttpResponseMessage> GetwithBearerToken(string url, string token)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072 | SecurityProtocolType.Tls12;
            using (var client = new HttpClient())
            {
                if (!string.IsNullOrWhiteSpace(token))
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                }
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072 | SecurityProtocolType.Tls12;
                var response = await client.SendAsync(request);
                return response;
            }
        }




    }
}
