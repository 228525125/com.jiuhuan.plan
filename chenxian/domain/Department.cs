using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{
    [Entity(entirety: true, title: "部门")]
    public class Department : Entity
    {
        [Keyword]
        [Column(width: 100)]
        [TextBox(width: 100, ui: true)]
        [Field("部门编号")]
        public string FNumber { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, ui: true)]
        [Field("部门名称")]
        public string FName { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, ui: true)]
        [Field("上级部门")]
        public string FParent { get; set; }

        [Ignore]
        [ManyToMany(typeof(User), "Department_User", mappedBy: "FName", joinColumn: "FNumber", "员工列表")]
        [Popup("请选择部门员工", multipleRowSelection: true, isMultipleColumnReturn: true)]
        public List<User> UserList { get; set; } = new List<User>();
    }
}
