using com.jiuhuan.plan.expression;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace com.jiuhuan.plan.tools {

    public class Utility {

        /// <summary>
        /// 简单解析出xml标签中的内容
        /// </summary>
        /// <param name="xmlContent"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static string ParseXmlTag(string xmlContent, string tag)
        {
            if (string.IsNullOrEmpty(xmlContent))
            {
                return null;
            }

            // 使用正则匹配 <parameter>...</parameter> 标签内容
            var match = Regex.Match(
                xmlContent,
                @"<" + tag + ">(.*?)</" + tag + ">",
                RegexOptions.Singleline);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return null;
        }

        /// <summary>
        /// 解析筛选条件
        /// </summary>
        /// <param name="filterValue">筛选表达式，例如<=0</param>
        /// <returns>筛选条件对象</returns>
        public static FilterCondition ParseFilterCondition(string filterValue)
        {
            // 首先检查是否为区间表达式 A:B
            int colonIndex = filterValue.IndexOf(':');
            if (colonIndex > 0 && colonIndex < filterValue.Length - 1)
            {
                string startValue = filterValue.Substring(0, colonIndex).Trim();
                string endValue = filterValue.Substring(colonIndex + 1).Trim();

                // 确保起始值和结束值都不为空
                if (!string.IsNullOrEmpty(startValue) && !string.IsNullOrEmpty(endValue))
                {
                    return new FilterCondition
                    {
                        Operator = ":",
                        Value = startValue,
                        EndValue = endValue
                    };
                }
            }

            // 支持的运算符：>, >=, <, <=, =, !=, <>, ~=（模糊匹配）
            string[] operators = { ">=", "<=", "!=", "<>", "~=", ">", "<", "=" };

            foreach (string op in operators)
            {
                if (filterValue.StartsWith(op))
                {
                    string value = filterValue.Substring(op.Length).Trim();
                    return new FilterCondition
                    {
                        Operator = op,
                        Value = value
                    };
                }
            }

            // 如果没有运算符，默认为模糊匹配
            return new FilterCondition
            {
                Operator = "~=",
                Value = filterValue
            };
        }

        /// <summary>
        /// 判断值是否匹配筛选条件
        /// </summary>
        /// <param name="value">属性值</param>
        /// <param name="condition">筛选条件</param>
        /// <returns>是否匹配</returns>
        public static bool MatchCondition(object value, FilterCondition condition)
        {
            string valueStr = value.ToString();

            // 处理区间表达式 A:B
            if (condition.Operator == ":")
            {
                return MatchRangeCondition(valueStr, condition.Value, condition.EndValue);
            }

            switch (condition.Operator)
            {
                case "=":
                    return valueStr.Equals(condition.Value, StringComparison.OrdinalIgnoreCase);

                case "!=":
                case "<>":
                    return !valueStr.Equals(condition.Value, StringComparison.OrdinalIgnoreCase);

                case ">=":
                    if (double.TryParse(valueStr, out double num1) && double.TryParse(condition.Value, out double num2))
                        return num1 >= num2;
                    return valueStr.CompareTo(condition.Value) >= 0;

                case "<=":
                    if (double.TryParse(valueStr, out double num3) && double.TryParse(condition.Value, out double num4))
                        return num3 <= num4;
                    return valueStr.CompareTo(condition.Value) <= 0;

                case ">":
                    if (double.TryParse(valueStr, out double num5) && double.TryParse(condition.Value, out double num6))
                        return num5 > num6;
                    return valueStr.CompareTo(condition.Value) > 0;

                case "<":
                    if (double.TryParse(valueStr, out double num7) && double.TryParse(condition.Value, out double num8))
                        return num7 < num8;
                    return valueStr.CompareTo(condition.Value) < 0;
                case "~=":
                default:
                    // 模糊匹配（不区分大小写）
                    return valueStr.IndexOf(condition.Value, StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        /// <summary>
        /// 判断值是否在区间内
        /// </summary>
        /// <param name="valueStr">属性值字符串</param>
        /// <param name="startValue">区间起始值</param>
        /// <param name="endValue">区间结束值</param>
        /// <returns>是否在区间内</returns>
        public static bool MatchRangeCondition(string valueStr, string startValue, string endValue)
        {
            // 尝试解析为数字
            if (double.TryParse(valueStr, out double num) &&
                double.TryParse(startValue, out double startNum) &&
                double.TryParse(endValue, out double endNum))
            {
                return num >= startNum && num <= endNum;
            }

            // 尝试解析为日期
            if (DateTime.TryParse(valueStr, out DateTime date) &&
                DateTime.TryParse(startValue, out DateTime startDate) &&
                DateTime.TryParse(endValue, out DateTime endDate))
            {
                return date >= startDate && date <= endDate;
            }

            // 字符串比较
            return valueStr.CompareTo(startValue) >= 0 && valueStr.CompareTo(endValue) <= 0;
        }

        /// <summary>
        /// 查询T类型的属性是否有被名为attributeName的特性修饰
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="attributeName">特性名称（不包含Attribute后缀，如"DisplayName"）</param>
        /// <returns>如果找到匹配的属性，返回对应的PropertyInfo；否则返回null</returns>
        public static PropertyInfo GetPropertyWithAttribute<T>(string attributeName)
        {
            if (string.IsNullOrEmpty(attributeName))
            {
                return null;
            }

            Type type = typeof(T);
            
            // 规范化特性名称：如果用户传入"Entity"，需要匹配"EntityAttribute"
            string normalizedAttributeName = attributeName.EndsWith("Attribute", StringComparison.OrdinalIgnoreCase) 
                ? attributeName 
                : attributeName + "Attribute";

            // 遍历类型及其基类
            while (type != null && type != typeof(object))
            {
                // 获取当前类型的所有公共实例属性
                PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);

                foreach (var property in properties)
                {
                    // 获取该属性上的所有自定义特性
                    object[] attributes = property.GetCustomAttributes(false);

                    // 检查是否有匹配的特性
                    foreach (var attr in attributes)
                    {
                        if (attr.GetType().Name.Equals(normalizedAttributeName, StringComparison.OrdinalIgnoreCase))
                        {
                            return property;
                        }
                    }
                }

                // 继续查找基类
                type = type.BaseType;
            }

            return null;
        }

        /// <summary>
        /// 获取T的属性字段名为filedName的字段的特性名为attributeName的特性的配置属性值
        /// </summary>
        /// <typeparam name="T">类</typeparam>
        /// <param name="attributeName">特性名</param>
        /// <param name="propertyName">属性名</param>
        /// <param name="fieldName">属性字段名</param>
        /// <returns></returns>
        public static object GetAttributeValueByField<T>(string attributeName, string propertyName, string memberName)
        {
            if (string.IsNullOrEmpty(attributeName) || string.IsNullOrEmpty(propertyName) || string.IsNullOrEmpty(memberName))
            {
                return null;
            }

            Type type = typeof(T);
            
            // 规范化特性名称：如果用户传入"Entity"，需要匹配"EntityAttribute"
            string normalizedAttributeName = attributeName.EndsWith("Attribute", StringComparison.OrdinalIgnoreCase) 
                ? attributeName 
                : attributeName + "Attribute";

            // 遍历类型及其基类，查找指定成员上的特性
            while (type != null && type != typeof(object))
            {
                // 首先尝试获取属性（Property）
                PropertyInfo propertyInfo = type.GetProperty(memberName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                
                if (propertyInfo != null)
                {
                    // 获取该属性上的所有自定义特性
                    object[] memberAttributes = propertyInfo.GetCustomAttributes(false);

                    // 查找匹配的特性
                    foreach (var attr in memberAttributes)
                    {
                        if (attr.GetType().Name.Equals(normalizedAttributeName, StringComparison.OrdinalIgnoreCase))
                        {
                            // 获取特性类中的指定属性
                            PropertyInfo configProperty = attr.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                            if (configProperty != null)
                            {
                                try
                                {
                                    return configProperty.GetValue(attr);
                                }
                                catch (Exception ex)
                                {
                                    // 记录异常但不中断处理
                                    Debug.WriteLine($"获取属性 {memberName} 上特性 {attributeName} 的属性 {propertyName} 时出错: {ex.Message}");
                                    return null;
                                }
                            }
                        }
                    }
                    
                    // 如果找到了属性但没有找到匹配的特性，停止向上查找
                    break; 
                }

                // 如果属性没找到，尝试获取字段（Field）
                FieldInfo fieldInfo = type.GetField(memberName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

                if (fieldInfo != null)
                {
                    // 获取该字段上的所有自定义特性
                    object[] fieldAttributes = fieldInfo.GetCustomAttributes(false);

                    // 查找匹配的特性
                    foreach (var attr in fieldAttributes)
                    {
                        if (attr.GetType().Name.Equals(normalizedAttributeName, StringComparison.OrdinalIgnoreCase))
                        {
                            // 获取特性类中的指定属性
                            PropertyInfo configProperty = attr.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                            if (configProperty != null)
                            {
                                try
                                {
                                    return configProperty.GetValue(attr);
                                }
                                catch (Exception ex)
                                {
                                    // 记录异常但不中断处理
                                    Debug.WriteLine($"获取字段 {memberName} 上特性 {attributeName} 的属性 {propertyName} 时出错: {ex.Message}");
                                    return null;
                                }
                            }
                        }
                    }
                    
                    // 如果找到了字段但没有找到匹配的特性，停止向上查找
                    break; 
                }

                // 如果当前类没有找到该成员，继续向基类查找
                type = type.BaseType;
            }

            return null;
        }

        /// <summary>
        /// 获取T或其基类被特性名为attributeNames的特性的配置属性值
        /// 子类的特性的属性会覆盖父类的特性的属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="attributeName">特性名</param>
        /// <param name="propertyName">属性名</param>
        /// <returns></returns>
        public static object GetAttributeValueByClass<T>(string attributeName, string propertyName)
        {
            Type type = typeof(T);
            return GetAttributeValueByClass(type, attributeName, propertyName);
        }

        /// <summary>
        /// 获取T或其基类被特性名为attributeNames的特性的配置属性值
        /// 子类的特性的属性会覆盖父类的特性的属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="attributeName">特性名</param>
        /// <param name="propertyName">属性名</param>
        /// <returns></returns>
        public static object GetAttributeValueByClass(Type type, string attributeName, string propertyName)
        {
            if (string.IsNullOrEmpty(attributeName) || string.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            // 规范化特性名称：如果用户传入"Entity"，需要匹配"EntityAttribute"
            string normalizedAttributeName = attributeName.EndsWith("Attribute", StringComparison.OrdinalIgnoreCase)
                ? attributeName
                : attributeName + "Attribute";

            // 从最具体的子类开始遍历到基类
            // 找到第一个非默认值的属性值就返回（子类优先）
            while (type != null && type != typeof(object))
            {
                // 获取当前类型上的所有自定义特性
                var allAttributes = type.GetCustomAttributes(false);

                // 查找匹配的特性
                var matchingAttributes = allAttributes.Where(attr =>
                    attr.GetType().Name.Equals(normalizedAttributeName, StringComparison.OrdinalIgnoreCase)).ToList();

                if (matchingAttributes.Any())
                {
                    foreach (var attr in matchingAttributes)
                    {
                        // 获取特性类中的指定属性
                        PropertyInfo configProperty = attr.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                        if (configProperty != null)
                        {
                            try
                            {
                                var value = configProperty.GetValue(attr);

                                // 如果不是默认值，直接返回（子类优先）
                                if (!IsDefaultValue(value))
                                {
                                    return value;
                                }
                            }
                            catch (Exception ex)
                            {
                                // 记录异常但不中断处理
                                Debug.WriteLine($"获取特性属性 {propertyName} 时出错: {ex.Message}");
                            }
                        }
                    }
                }

                type = type.BaseType;
            }

            // 如果所有级别都是默认值，返回最后一个找到的值或null
            while (type != null && type != typeof(object))
            {
                var allAttributes = type.GetCustomAttributes(false);
                var matchingAttributes = allAttributes.Where(attr =>
                    attr.GetType().Name.Equals(normalizedAttributeName, StringComparison.OrdinalIgnoreCase)).ToList();

                if (matchingAttributes.Any())
                {
                    // 返回第一个特性的属性值（通常是最具体的子类）
                    var firstAttr = matchingAttributes.First();
                    PropertyInfo configProperty = firstAttr.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                    if (configProperty != null)
                    {
                        try
                        {
                            return configProperty.GetValue(firstAttr);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"获取特性属性 {propertyName} 时出错: {ex.Message}");
                        }
                    }
                }

                type = type.BaseType;
            }

            return null;
        }

        /// <summary>
        /// 判断值是否为默认值
        /// </summary>
        /// <param name="value">要检查的值</param>
        /// <returns>如果是默认值返回true，否则返回false</returns>
        private static bool IsDefaultValue(object value)
        {
            if (value == null)
                return true;
            
            Type type = value.GetType();
            
            // 布尔类型的默认值是false
            //if (type == typeof(bool))
            //    return !(bool)value;
            
            // 字符串类型的默认值是null或空字符串
            if (type == typeof(string))
                return string.IsNullOrEmpty((string)value);
            
            // 数值类型的默认值是0
            //if (type.IsPrimitive || type == typeof(float))
            //    return Convert.ToDecimal(value) == 0;
            
            return false;
        }

        /// <summary>
        /// 使用反射调用对象的指定方法
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="methodName">要调用的方法名</param>
        /// <param name="parameters">方法参数</param>
        public static void InvokeMethod(object obj, string methodName, params object[] parameters)
        {
            if (obj == null)
                return;

            var method = obj.GetType().GetMethod(methodName);
            if (method != null)
            {
                try
                {
                    method.Invoke(obj, parameters);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"调用方法 {methodName} 时出错: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 将prop1中的属性赋值给prop2中的同名属性，适用于不同类型的对象
        /// </summary>
        /// <typeparam name="T1">源对象类型</typeparam>
        /// <typeparam name="T2">目标对象类型</typeparam>
        /// <param name="source">源对象，属性值将从该对象获取</param>
        /// <param name="target">目标对象，属性值将赋值到该对象</param>
        /// <returns>返回目标对象</returns>
        /// <exception cref="ArgumentNullException">当source或target为null时抛出异常</exception>
        public static T2 CopyProperties<T1, T2>(T1 source, T2 target) where T1 : class where T2 : class
        {
            // 验证参数是否为空
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "源对象不能为null");
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target), "目标对象不能为null");
            }

            // 获取源对象和目标对象的类型
            var sourceType = typeof(T1);
            var targetType = typeof(T2);

            // 获取源对象的所有公共属性
            var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 遍历源对象的属性
            foreach (var property in sourceProperties)
            {
                // 检查目标对象是否存在同名属性且可写
                var targetProperty = targetType.GetProperty(property.Name);
                if (targetProperty != null && targetProperty.CanWrite)
                {
                    // 获取源对象中该属性的值
                    var value = property.GetValue(source);

                    // 尝试将值设置到目标对象的对应属性
                    try
                    {
                        targetProperty.SetValue(target, value);
                    }
                    catch (Exception ex)
                    {
                        // 记录错误信息，但继续处理其他属性
                        Debug.WriteLine($"复制属性 {property.Name} 时发生异常: {ex.Message}");
                    }
                }
            }

            return target;
        }

        /// <summary>
        /// 将prop1中的属性赋值给prop2中的同名属性
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="source">源对象，属性值将从该对象获取</param>
        /// <param name="target">目标对象，属性值将赋值到该对象</param>
        /// <returns>返回目标对象</returns>
        /// <exception cref="ArgumentNullException">当source或target为null时抛出异常</exception>
        public static T CopyProperties<T>(T source, T target) where T : class
        {
            // 验证参数是否为空
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "源对象不能为null");
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target), "目标对象不能为null");
            }

            // 获取类型的所有公共属性
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 遍历所有属性，将source的属性值复制到target
            foreach (var property in properties)
            {
                // 检查属性是否可写
                if (property.CanWrite)
                {
                    // 获取source对象中该属性的值
                    var value = property.GetValue(source);

                    // 将值设置到target对象的对应属性
                    property.SetValue(target, value);
                }
            }

            return target;
        }

        /// <summary>
        /// 复制一份List<T>
        /// </summary>
        /// <typeparam name="T">列表项的类型</typeparam>
        /// <param name="list">要复制的列表</param>
        /// <returns>复制后的列表</returns>
        public static List<T> CopyList<T>(List<T> list)
        {
            if (list == null)
            {
                return new List<T>();
            }

            return new List<T>(list);
        }

        /// <summary>
        /// 将数字转换为中文一周的写法
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string ToWeekDay(int number)
        {
            string[] chineseNumber = new string[] { "日", "一", "二", "三", "四", "五", "六"};
            return chineseNumber[number];
        }

        /// <summary>
        /// 截取字符串左边length个字符
        /// </summary>
        /// <param name="str"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string leftString(string str, int length)
        {
            return length > str.Length ? str : str.Substring(str.Length - length);
        }

        /// <summary>
        /// 随机数生成 min - max
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max">包含max</param>
        /// <returns></returns>
        public static int nextInt(int min, int max)
        {
            Random random = new Random();

            // 生成一个从0到100的随机整数
            return random.Next(min, max + 1);
        }

        public static bool isNumberic(string message, out int result) {
            System.Text.RegularExpressions.Regex rex =
            new System.Text.RegularExpressions.Regex(@"^\d+$");
            result = -1;

            if ("" == message || null == message)
                return false;

            if (rex.IsMatch(message)) {
                result = int.Parse(message);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 延迟执行
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="func"></param>
        public static void setTimeout(int delay, ElapsedEventHandler func)
        {
            var timer = new Timer();
            timer.Interval = delay;  //延迟
            timer.Elapsed += new ElapsedEventHandler(func);
            timer.AutoReset = false; //执行一次为false,重复执行为true
            timer.Start();
        }

        /// <summary>
        /// 间隔执行
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="func"></param>
        public static Timer setInterval(int interval, ElapsedEventHandler func)
        {
            var timer = new Timer();
            timer.Interval = interval;  //间隔
            timer.Elapsed += new ElapsedEventHandler(func);
            timer.AutoReset = true; //执行一次为false,重复执行为true
            timer.Start();
            return timer;
        }
    }
}
