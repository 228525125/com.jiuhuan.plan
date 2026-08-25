using System;

namespace com.jiuhuan.plan
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ManyToManyAttribute : Attribute
    {
        public string Title { get; set; }        //标题，必须与EditForm中TabControl中的tabPage的标题一致

        public string ImportSql { get; set; }    //SQL查询语句

        public string[] Parameter { get; set; }  //SQL参数

        public Type ChildType { get; set; }      //子表实体类型

        public string MappedBy { get; set; }     //映射，被关联方-字段

        public string JoinColumn { get; set; }   //外键，关联方-字段

        public string MappingTable { get; set; } //中间表，保存多对多关系

        public string Description { get; set; }

        public ManyToManyAttribute()
        {
        }

        public ManyToManyAttribute(Type childType, string mappingTable, string mappedBy, string joinColumn, string title, string importSql = "", string [] parameter = null, string description = "")
        {
            this.ChildType = childType;
            this.MappingTable = mappingTable;
            this.MappedBy = mappedBy;
            this.JoinColumn = joinColumn;
            this.Title = title;
            this.ImportSql = importSql;
            this.Parameter = parameter;
            this.Description = description;
        }
    }
}