using System;

namespace com.jiuhuan.plan
{
    /// <summary>
    /// 表示该属性/字段在某些操作中应被忽略（例如：数据库保存、序列化）
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class HiddenColumnAttribute : Attribute
    {
        // 可选构造函数参数
        public string Reason { get; }

        public HiddenColumnAttribute()
        {
        }

        public HiddenColumnAttribute(string reason)
        {
            this.Reason = reason;
        }
    }
}