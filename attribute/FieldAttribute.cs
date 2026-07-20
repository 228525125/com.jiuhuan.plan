using System;

namespace com.jiuhuan.plan
{
    /// <summary>
    /// 控件属性基类，表示控件的基本属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class FieldAttribute : Attribute
    {
        // 可选构造函数参数
        public string Name { get; set; }
        public string Description { get; set; }
        public int Length { get; set; }
        public bool AllowNull { get; set; }
        public string Validation { get; set; }
        public string ErrorMessage { get; set; }

        public FieldAttribute()
        {
        }

        /// <summary>
        /// 字段特性
        /// </summary>
        /// <param name="name">字段名</param>
        /// <param name="length">长度</param>
        /// <param name="allowNull">字段是否允许为空</param>
        /// <param name="validation">支持的运算符：>, >=, <, <=, =, !=, <>, ~=（模糊匹配），例如,填入 >0</param>
        /// <param name="errMsg">验证错误时提示内容</param>
        /// <param name="description"></param>
        public FieldAttribute(string name, int length = 100, bool allowNull = true, string validation = "", string errMsg = "", string description = "")
        {
            this.Name = name;
            this.Length = length;
            this.AllowNull = allowNull;
            this.Validation = validation;
            this.Description = description;
            if (string.IsNullOrEmpty(errMsg))
                ErrorMessage = $"字段{name}存在错误，请检查！";
            else
                ErrorMessage = errMsg;
        }
    }
}