using System;

namespace com.jiuhuan.plan
{
    /// <summary>
    /// 权限
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class PermissionAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public PermissionAttribute()
        {
        }

        /// <summary>
        /// 方法特性
        /// </summary>
        /// <param name="name">方法名</param>
        /// <param name="description"></param>
        public PermissionAttribute(string name, string description = "")
        {
            this.Name = name;
            this.Description = description;
        }
    }
}