using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{
    [Entity(entirety: true, title: "权限")]
    public class Permission : Entity
    {
        [Column(width: 100)]
        [TextBox(width: 100, readOnly: false)]
        [Field("角色编号")]
        public string FRoleNumber { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, readOnly: false)]
        [Field("单据编号")]
        public string FDocumentNumber { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, readOnly: false)]
        [Field("单据名称")]
        public string FDocumentName { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, readOnly: false)]
        [Field("单据类型")]
        public string FDocumentType { get; set; }

        [Column(width: 100, readOnly: false)]
        [CheckBox(readOnly: false)]
        [Field("新建")]
        public bool FIsCreate { get; set; }

        [Column(width: 100, readOnly: false)]
        [CheckBox(readOnly: false)]
        [Field("删除")]
        public bool FIsDelete { get; set; }

        [Column(width: 100, readOnly: false)]
        [CheckBox(readOnly: false)]
        [Field("修改")]
        public bool FIsModify { get; set; }

        [Column(width: 100, readOnly: false)]
        [CheckBox(readOnly: false)]
        [Field("浏览")]
        public bool FIsQuery { get; set; }

        [Column(width: 100, readOnly: false)]
        [CheckBox(readOnly: false)]
        [Field("导出")]
        public bool FIsExport { get; set; }

        [Column(width: 100, readOnly: false)]
        [CheckBox(readOnly: false)]
        [Field("打印")]
        public bool FIsPrint { get; set; }

        [Column(width: 100, readOnly: false)]
        [CheckBox(readOnly: false)]
        [Field("审核")]
        public bool FIsPass { get; set; }

        [Column(width: 100, readOnly: false)]
        [CheckBox(readOnly: false)]
        [Field("作废")]
        public bool FIsInvalid { get; set; }
    }
}
