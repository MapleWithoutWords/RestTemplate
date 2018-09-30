using Consul;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RestTemplate
{
    public class RestConsul
    {
        public RestConsul(string consulAdress, string consulDatacenter, HttpClient client)
        {
            ConsulAdress = consulAdress;
            ConsulDatacenter = consulDatacenter;
            this.client = client;
        }
        /// <summary>
        /// 例如：http://127.0.0.1:8500
        /// </summary>
        public string ConsulAdress { get; set; } = "http://127.0.0.1:8500";
        public string ConsulDatacenter { get; set; } = "dc1";
        private HttpClient client;
        public async Task<RestResponseContent> GetAsync(string url, HttpRequestHeaders heads = null)
        {
            HttpRequestMessage requestMessage = await CreateHttpRequestMessage(url, heads);
            requestMessage.Method = HttpMethod.Get;
            return await SendAsync(requestMessage);
        }
        public async Task<RestResponseBody<T>> GetForEntityAsync<T>(string url, HttpRequestHeaders heads = null)
        {
            HttpRequestMessage requestMessage = await CreateHttpRequestMessage(url, heads);
            requestMessage.Method = HttpMethod.Get;
            return await SendForEntityAsync<T>(requestMessage);
        }
        public async Task<RestResponseContent> PostAsync(string url, object obj = null, HttpRequestHeaders heads = null)
        {
            HttpRequestMessage requestMessage = await CreateHttpRequestMessage(url, obj, heads);
            requestMessage.Method = HttpMethod.Post;
            return await SendAsync(requestMessage);
        }
        public async Task<RestResponseBody<T>> PostForEntityAsync<T>(string url, object obj = null, HttpRequestHeaders heads = null)
        {
            HttpRequestMessage requestMessage = await CreateHttpRequestMessage(url, obj, heads);
            requestMessage.Method = HttpMethod.Post;
            return await SendForEntityAsync<T>(requestMessage);
        }

        public async Task<RestResponseContent> PutAsync(string url, object obj = null, HttpRequestHeaders heads = null)
        {
            HttpRequestMessage requestMessage = await CreateHttpRequestMessage(url, obj, heads);
            requestMessage.Method = HttpMethod.Put;
            return await SendAsync(requestMessage);
        }
        public async Task<RestResponseBody<T>> PutForEntityAsync<T>(string url, object obj = null, HttpRequestHeaders heads = null)
        {
            HttpRequestMessage requestMessage = await CreateHttpRequestMessage(url, obj, heads);
            requestMessage.Method = HttpMethod.Put;
            return await SendForEntityAsync<T>(requestMessage);
        }
        public async Task<RestResponseBody<T>> DeleteForEntityAsync<T>(string url, HttpRequestHeaders heads = null)
        {
            HttpRequestMessage requestMessage = await CreateHttpRequestMessage(url, heads);
            requestMessage.Method = HttpMethod.Delete;
            return await SendForEntityAsync<T>(requestMessage);
        }
        public async Task<RestResponseContent> DeleteAsync(string url, HttpRequestHeaders heads = null)
        {
            HttpRequestMessage requestMessage = await CreateHttpRequestMessage(url, heads);
            requestMessage.Method = HttpMethod.Delete;
            return await SendAsync(requestMessage);
        }

        /// <summary>
        /// 创建HttpRequestMessage
        /// </summary>
        /// <param name="url"></param>
        /// <param name="obj"></param>
        /// <param name="heads"></param>
        /// <returns></returns>
        private async Task<HttpRequestMessage> CreateHttpRequestMessage(string url, object obj = null, HttpRequestHeaders heads = null)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage();
            if (heads != null)
            {
                foreach (var item in heads)
                {
                    requestMessage.Headers.Add(item.Key, item.Value);
                }
            }
            if (obj != null)
            {
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(obj));
            }
            string sendUrl = await TOServiceAdress(url);
            requestMessage.RequestUri = new Uri(sendUrl);
            return requestMessage;
        }
        private async Task<HttpRequestMessage> CreateHttpRequestMessage(string url, HttpRequestHeaders heads = null)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage();
            if (heads != null)
            {
                foreach (var item in heads)
                {
                    requestMessage.Headers.Add(item.Key, item.Value);
                }
            }
            string sendUrl = await TOServiceAdress(url);
            requestMessage.RequestUri = new Uri(sendUrl);
            return requestMessage;
        }

        /// <summary>
        /// 将：http://userCenterService/api/user转换成:http://127.0.0.1:6034/api/user
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<string> TOServiceAdress(string url)
        {
            Uri uri = new Uri(url);
            string servicName = uri.Host;
            string ipPort = await GetServiceAdress(servicName);
            return uri.Scheme + "://" + ipPort + uri.PathAndQuery;
        }
        private async Task<RestResponseContent> SendAsync(HttpRequestMessage requestMessage)
        {
            HttpResponseMessage msg = await client.SendAsync(requestMessage);
            RestResponseContent content = new RestResponseContent();
            content.Content = msg.Content;
            content.IsSuccessStatusCode = msg.IsSuccessStatusCode;
            content.StatusCode = msg.StatusCode;
            return content;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        private async Task<RestResponseBody<T>> SendForEntityAsync<T>(HttpRequestMessage requestMessage)
        {
            HttpResponseMessage msg = await client.SendAsync(requestMessage);
            RestResponseBody<T> content = new RestResponseBody<T>();
            content.Content = msg.Content;
            content.IsSuccessStatusCode = msg.IsSuccessStatusCode;
            content.StatusCode = msg.StatusCode;
            string json = await msg.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(json))
            {
                content.Body = JsonConvert.DeserializeObject<T>(json);
            }
            return content;
        }
        /// <summary>
        /// 根据服务名字获取服务地址
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private async Task<string> GetServiceAdress(string serviceName)
        {
            using (var consulClient = new ConsulClient(c =>
            {
                c.Address = new Uri(ConsulAdress);
                c.Datacenter = ConsulDatacenter;
            }))
            {
                var services = (await consulClient.Agent.Services()).Response.Values.Where(e => e.Service.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
                if (!services.Any())
                {
                    throw new ArgumentException($"找不到该服务={serviceName}");
                }

                var ser = services.ElementAt(new Random().Next(services.Count()));
                return ser.Address + ":" + ser.Port;
            }
        }
    }
}
