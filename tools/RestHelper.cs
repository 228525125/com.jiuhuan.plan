using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.tools {

    public class RestHelper {

        /// <summary>
        /// POST方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">服务访问地址</param>
        /// <param name="request">提交数据</param>
        /// <param name="jsonConverters">对返回结果进行转换控制</param>
        /// <returns></returns>
        public static async Task<Response<T>> PostForEntity<T>(string url, object request, params JsonConverter[] jsonConverters) {            
            return await HttpClientHelper.PostAsync<T>(url, JsonHelper.toJson(request), jsonConverters);
        }

        public static async Task<Response<T>> GetForEntity<T>(string url, params JsonConverter[] jsonConverters) {
            return await HttpClientHelper.GetAsync<T>(url, jsonConverters);
        }
    }
}
