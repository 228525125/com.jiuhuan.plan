using com.jiuhuan.plan.view;
using System;

namespace com.jiuhuan.plan
{
    /// <summary>
    /// 当操作者双击表格某行时进行弹窗
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DataGridView_RowDoubleClickAttribute : Attribute
    {
        public string Name { get; set; }

        public string Sql { get; set; }

        public string[] Parameter { get; set; }

        public string SqlFile { get; set; }

        public Type Type { get; set; }

        public string Description { get; set; }

        public DataGridView_RowDoubleClickAttribute()
        {
        }

        public DataGridView_RowDoubleClickAttribute(string name = "弹窗", string sql = "", string[] parameter = null, string sqlFile = "", Type type = null, string back = "", string description = "")
        {
            this.Name = name;
            this.Sql = sql;
            this.Parameter = parameter;
            this.SqlFile = sqlFile;
            this.Type = null == type ? typeof(ReturnPopup) : type;
            this.Description = description;
        }
    }
}