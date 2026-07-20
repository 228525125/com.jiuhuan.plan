using QFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{
    public class User : Entity
    {
        public static string IsStartupOperationPlanStartDate = "IsStartupOperationPlanStartDate";
        public static string FOperationPlanStartDate = "FOperationPlanStartDate";
        public static string FPageSize = "FPageSize";


        [Keyword]
        public string FName { get; set; }
        public string FPassword { get; set; }
        public string FDepartment { get; set; }
        public string FRole { get; set; }

        [Serialization("序列化为JSON字符串，再保存到数据库，默认类型：Dictionary<string, object>")]
        public Dictionary<string, object> FBuffer { get; set; }

        public object GetSettings(string key)
        {
            if (null == FBuffer || !FBuffer.Any())
                return null;

            if (!FBuffer.ContainsKey(key))
                return null;

            return FBuffer[key];
        }

        public void SetSettings(string key, object value)
        {
            if (null == FBuffer)
                FBuffer = new Dictionary<string, object>();

            FBuffer[key] = value;
        }
    }
}
