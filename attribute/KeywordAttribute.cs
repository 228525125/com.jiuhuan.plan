using System;

namespace com.jiuhuan.plan
{
    /// <summary>
    /// 表示该属性/字段用于数据库查找唯一记录的条件之一
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class KeywordAttribute : Attribute
    {
        // 可选构造函数参数
        public string Reason { get; }

        public KeywordAttribute()
        {
        }

        public KeywordAttribute(string reason)
        {
            this.Reason = reason;
        }
    }
}