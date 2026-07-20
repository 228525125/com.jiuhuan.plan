using System;

namespace com.jiuhuan.plan
{
    /// <summary>
    /// 表示该属性/字段用于数据库查找唯一记录的条件之一
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DateTimePickerAttribute : ControlAttribute
    {
        public DateTimePickerAttribute()
        {
        }

        public DateTimePickerAttribute(string title = "", int width = 100, bool readOnly = true, bool visible = true, bool ui = false, int index = 1000, string description = "")
            : base(title, width, readOnly, visible, ui, index, description)
        {
        }
    }
}