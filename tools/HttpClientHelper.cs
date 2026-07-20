using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.tools
{
    public class HttpClientHelper
    {
        private static readonly HttpClient _httpClient;
        static HttpClientHelper()
        {
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.None};
            _httpClient = new HttpClient(handler);
            _httpClient.Timeout = TimeSpan.FromMilliseconds(5000);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, application/*+json");
        }

        /// <summary>
        /// 以POST方式提交数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">服务访问地址</param>
        /// <param name="postData">提交的数据</param>
        /// <param name="converters">对返回结果进行转换控制</param>
        /// <returns></returns>
        public static async Task<Response<T>> PostAsync<T>(string url, string postData, params JsonConverter[] converters)
        {
            byte[] data = Encoding.UTF8.GetBytes(postData);
            using (Stream stream = new MemoryStream(data ?? new byte[0]))
            {
                using (HttpContent content = new StreamContent(stream))
                {
                    content.Headers.Add("Content-Type", "application/json");
                    using (HttpResponseMessage message = await _httpClient.PostAsync(url, content))
                    {
                        Response<T> response;
                        if(HttpStatusCode.OK == message.StatusCode) {
                            response = new Response<T>() { StatusCode = message.StatusCode, RespData = JsonHelper.toObject<T>(message.Content.ReadAsStringAsync().Result, converters) };
                        }
                        else {
                            response = new Response<T>() { StatusCode = message.StatusCode, Error = JsonHelper.toObject<Error>(message.Content.ReadAsStringAsync().Result) };
                        }
                        return response;
                    }
                }
            }
        }

        /// <summary>
        /// 以GET方式查询数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">服务访问地址</param>
        /// <param name="converters">对返回结果进行转换控制</param>
        /// <returns></returns>
        public static async Task<Response<T>> GetAsync<T>(string url, params JsonConverter[] converters) {

            using (HttpResponseMessage message = await _httpClient.GetAsync(url)) {
                Response<T> response;
                if (HttpStatusCode.OK == message.StatusCode) {
                    response = new Response<T>() { StatusCode = message.StatusCode, RespData = JsonHelper.toObject<T>(message.Content.ReadAsStringAsync().Result, converters) };
                }
                else {
                    response = new Response<T>() { StatusCode = message.StatusCode, Error = JsonHelper.toObject<Error>(message.Content.ReadAsStringAsync().Result) };
                }
                return response;
            }
        }
    }
}
