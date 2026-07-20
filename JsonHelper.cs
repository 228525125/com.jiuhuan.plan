using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace com.jiuhuan.plan.tools {

    public class JsonHelper
    {
        public static string toJson<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T toObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static object toObject(string json, Type type) {
            return JsonConvert.DeserializeObject(json, type);
        }

        public static T toObject<T>(string json, params JsonConverter[] converters)
        {            
            return JsonConvert.DeserializeObject<T>(json,converters);
        }

    }

    /// <summary>
    /// 多态反序列化
    /// </summary>
    /// <typeparam name="T">GameObject</typeparam>
    public abstract class SupportCustomJsonConverter<T> : JsonConverter
    {
        protected abstract T Create(Type objectType, JObject jsonObject);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (JsonToken.Null == reader.TokenType) return null; //JsonToken.Null 表示字段为null，无法解析会抛出异常

            JObject jsonObject = JObject.Load(reader);
            T target = Create(objectType, jsonObject);
            serializer.Populate(jsonObject.CreateReader(), target);
            return target;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override bool CanWrite => false;         //写操作使用默认行为
    }
}