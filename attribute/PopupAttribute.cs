using com.jiuhuan.plan.view;
using System;

namespace com.jiuhuan.plan
{
    /// <summary>
    /// 控件属性基类，表示控件的基本属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class PopupAttribute : Attribute
    {
        public string Name { get; set; }

        public string Sql { get; set; }

        public string[] Parameter { get; set; }

        public string SqlFile { get; set; }

        public Type Type { get; set; }

        public string Return { get; set; }

        public bool AllowMultipleRowSelection { get; set; }

        public bool IsMultipleColumnReturn { get; set; }

        public bool AllowDuplicates { get; set; }  //是否允许重复

        public string Description { get; set; }

        public PopupAttribute()
        {
        }

        public PopupAttribute(string name = "弹窗", string sql = "", string[] parameter = null, string sqlFile = "", Type type = null, string back = "", bool multipleRowSelection = false, bool isMultipleColumnReturn = false, bool allowduplicates = false, string description = "")
        {
            this.Name = name;
            this.Sql = sql;
            this.Parameter = parameter;
            this.SqlFile = sqlFile;
            this.Type = null == type ? typeof(ReturnPopup) : type;
            this.Return = back;
            this.AllowMultipleRowSelection = multipleRowSelection;
            this.IsMultipleColumnReturn = isMultipleColumnReturn;
            this.AllowDuplicates = allowduplicates;
            this.Description = description;
        }
    }
}