using System;

namespace com.jiuhuan.plan
{
    /// <summary>
    /// 该类的配置信息
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class EntityAttribute : Attribute
    {
        /// <summary>
        /// 是否整体配置
        /// </summary>
        public bool Entirety { get; set; }

        /// <summary>
        /// 描述信息
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// 表格名称
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 维护列表界面名称
        /// </summary>
        public string FormName { get; set; }

        /// <summary>
        /// 编辑界面名称
        /// </summary>
        public string EditFormName { get; set; }

        /// <summary>
        /// 分页
        /// </summary>
        public bool Pagination { get; set; }

        public EntityAttribute()
        {
            this.Entirety = false;
            this.Description = "";
            this.TableName = "";
            this.Title = "";
            this.FormName = "";
            this.EditFormName = "";
            this.Pagination = false;
        }

        public EntityAttribute(string tableName = "", bool entirety = false, string databaseName = "", string title ="", string formName = "", string editFormName = "", bool pagination = false, string description = "")
        {
            this.Entirety = entirety;
            this.Database = databaseName;
            this.Description = description;
            this.TableName = tableName;
            this.Title = title;
            this.FormName = formName;
            this.EditFormName = editFormName;
            this.Pagination = pagination;
        }
    }
}