using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{
    [Entity(entirety: true, title: "角色")]
    public class Role : Entity
    {
        [Keyword]
        [Column(width: 100)]
        [TextBox(width: 100, readOnly: false)]
        [Field("角色编号")]
        public string FNumber { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, readOnly: false)]
        [Field("角色名称")]
        public string FName { get; set; }

        [Column(width: 100)]
        [ComboBox("作业;数据", readOnly: false)]
        [Field("角色类型")]
        public string FType { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, readOnly: false)]
        [Field("优先级")]
        public int FPriority { get; set; }

        [Column(width: 100)]
        [ComboBox("包含;排除", readOnly: false)]
        [Field("关系符")]
        public string FOperator {  get; set; }

        //[Column(width: 100)]
        //[TextBox(width: 100, readOnly: false)]
        [Field("条件")]
        public string FCondition { get; set; }

        [Ignore]
        [ManyToMany(typeof(User), "Role_User", mappedBy: "FName", joinColumn: "FNumber", "员工列表")]
        [Popup("请选择员工", multipleRowSelection: true, isMultipleColumnReturn: true)]
        public List<User> UserList { get; set; } = new List<User>();

        [Ignore]
        [OneToMany(typeof(Permission), mappedBy: "FRoleNumber", joinColumn: "FNumber", title: "权限列表")]
        [Popup("请选择单据", multipleRowSelection: true, isMultipleColumnReturn: true)]
        public List<Permission> PermissionList { get; set; } = new List<Permission>();
    }
}
