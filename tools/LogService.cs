using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.tools {

    public class LogService
    {

        public const string Local_IP = "localhost";
        public const string Remote_IP = "192.168.1.205";

        public static string serverIp = Remote_IP;
        public static string version = "/v1";
        public static string prefix = "/Toolbox";
        public static string serviceName = "/toolboxservice";
        public static string baseUrl = "http://"+ serverIp + ":5555" + version + prefix;

        public static async Task<Response<T>> Save<T>(T obj) {
            return await RestHelper.PostForEntity<T>(baseUrl + "/" + obj.GetType().Name + "/Save", obj);
        }

        public static async Task<Response<T>> FindById<T>(long id) {
            return await RestHelper.GetForEntity<T>(baseUrl + "/" + typeof(T).Name + "/" + id);
        }

        public static async Task<Response<T>> FindById<T>(long id, params JsonConverter[] jsonConverters)
        {
            return await RestHelper.GetForEntity<T>(baseUrl + "/" + typeof(T).Name + "/" + id, jsonConverters);
        }

        public static async Task<Response<List<T>>> FindAll<T>(int size) {
            return await RestHelper.GetForEntity<List<T>>(baseUrl + "/" + typeof(T).Name + "/List/"+size);
        }
    }
}
