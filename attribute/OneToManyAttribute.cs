using System;

namespace com.jiuhuan.plan
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class OneToManyAttribute : Attribute
    {
        public string Title { get; set; }

        public string ImportSql { get; set; }

        public string[] Parameter { get; set; }

        public Type ChildType { get; set; }

        public string MappedBy { get; set; }

        public string JoinColumn { get; set; }

        public string Description { get; set; }

        public OneToManyAttribute()
        {
        }

        public OneToManyAttribute(Type childType, string mappedBy, string joinColumn = "", string title = "", string importSql = "", string [] parameter = null, string description = "")
        {
            this.ChildType = childType;
            this.MappedBy = mappedBy;
            this.JoinColumn = joinColumn;
            this.Title = title;
            this.ImportSql = importSql;
            this.Parameter = parameter;
            this.Description = description;
        }
    }
}