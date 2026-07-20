using System;

namespace com.jiuhuan.plan
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class JoinColumnAttribute : Attribute
    {
        public Type ParentType { get; set; }

        public string MappedBy { get; set; }

        public string Description { get; set; }

        public JoinColumnAttribute()
        {
        }

        public JoinColumnAttribute(Type parentType, string mappedBy = "", string description = "")
        {
            this.ParentType = parentType;
            this.MappedBy = mappedBy;
            this.Description = description;
        }
    }
}