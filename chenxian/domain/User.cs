using QFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{
    [Entity(entirety: true, title: "员工")]
    public class User : Entity
    {
        public static string IsStartupOperationPlanStartDate = "IsStartupOperationPlanStartDate";
        public static string FOperationPlanStartDate = "FOperationPlanStartDate";
        public static string FPageSize = "FPageSize";

        [Keyword]
        [Column(width: 100)]
        [TextBox(width: 100, ui: true)]
        [Field("帐号")]
        public string FName { get; set; }

        [Ignore]
        [Column(width : 100)]
        [TextBox(width: 100, ui: true)]
        [Field("姓名")]
        public string FDescription { get { return FNote; } }

        public string FPassword { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, readOnly: false)]
        [Popup("请选择主要部门")]
        [Field("主要部门")]
        public string FDepartment { get; set; }

        [Ignore]
        [ManyToMany(typeof(Department), "Department_User", mappedBy: "FNumber", joinColumn: "FName", title: "部门列表")]
        [Popup("请选择部门", multipleRowSelection: true, isMultipleColumnReturn: true)]
        public List<Department> DepartmentList { get; set; } = new List<Department>();

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
