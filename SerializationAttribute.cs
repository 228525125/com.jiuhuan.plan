using System;

namespace com.jiuhuan.plan
{
    /// <summary>
    /// 用于标记Entity对象中的Dictionary类型字段，表示该字段应序列化为JSON字符串存储
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SerializationAttribute : Attribute
    {
        // 可选构造函数参数
        public string Reason { get; }

        /// <summary>
        /// 指定反序列化的类型，用于在从数据库读取时正确解析JSON字符串
        /// </summary>
        public Type DeserializeType { get; }

        public SerializationAttribute()
        {
        }

        public SerializationAttribute(string reason)
        {
            this.Reason = reason;
        }

        public SerializationAttribute(Type deserializeType)
        {
            this.DeserializeType = deserializeType;
        }

        public SerializationAttribute(string reason, Type deserializeType)
        {
            this.Reason = reason;
            this.DeserializeType = deserializeType;
        }
    }
}
