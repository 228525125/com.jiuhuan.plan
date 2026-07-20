using System;

namespace com.jiuhuan.plan
{
    /// <summary>
    /// 表示该属性/字段用于数据库查找唯一记录的条件之一
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ColumnAttribute : ControlAttribute
    {
        public ColumnAttribute()
        {
        }

        public ColumnAttribute(string title = "", int width = 125, bool readOnly = true, bool visible = true, int index = 100, string description = "")
            : base(title, width, readOnly, visible, false, index, description)
        {
        }
    }
}