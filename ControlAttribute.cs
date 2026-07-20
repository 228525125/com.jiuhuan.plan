using System;

namespace com.jiuhuan.plan
{
    /// <summary>
    /// 控件属性基类，表示控件的基本属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ControlAttribute : Attribute
    {
        // 可选构造函数参数
        public string Title { get; set; }
        public string Description { get; set; }
        public int Width { get; set; }
        public bool ReadOnly { get; set; }
        public int Index { get; set; }
        public bool Visible { get; set; }
        public bool UI { get; set; }

        public ControlAttribute()
        {
        }

        public ControlAttribute(string title, int width = 100, bool readOnly = true, bool visible = true, bool ui = false, int index = 1000, string description = "")
        {
            this.Title = title;
            this.Description = description;
            this.Width = width;
            this.ReadOnly = readOnly;
            this.Index = index;
            this.Visible = visible;
            this.UI = ui;
        }
    }
}