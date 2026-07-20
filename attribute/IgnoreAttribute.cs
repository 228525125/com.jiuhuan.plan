using System;

namespace com.jiuhuan.plan
{
    /// <summary>
    /// 表示该属性/字段在某些操作中应被忽略（例如：数据库保存、序列化）
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class IgnoreAttribute : Attribute
    {
        // 可选构造函数参数
        public string Reason { get; }

        public bool Excel { get; set; }      // excel操作是否忽略

        public bool Database { get; set; }   // 数据库操作是否忽略 

        public IgnoreAttribute()
        {
        }

        public IgnoreAttribute(string reason = "", bool excel = true, bool database = true)
        {
            this.Reason = reason;
            this.Excel = excel;
            this.Database = database;
        }
    }
}