using System;
using System.Collections.Generic;

namespace com.jiuhuan.plan
{
    /// <summary>
    /// 表示该属性/字段用于数据库查找唯一记录的条件之一
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ComboBoxAttribute : ControlAttribute
    {
        public string Items { get; set; }

        public ComboBoxAttribute()
        {
        }

        public ComboBoxAttribute(string items, int width = 100, bool readOnly = true, bool visible = true, bool ui = false, int index = 1000, string title = "", string description = "")
            : base(title, width, readOnly, visible, ui, index, description)
        {
            this.Items = items;
        }
    }
}