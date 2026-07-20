using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.expression
{
    /// <summary>
    /// 过滤条件，例如 <=10
    /// </summary>
    public class FilterCondition
    {
        public string Operator { get; set; }
        public string Value { get; set; }
        public string EndValue { get; set; }
    }
}
