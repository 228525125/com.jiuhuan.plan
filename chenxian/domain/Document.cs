using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{
    [Entity(entirety: true, title: "表单")]
    public class Document : Entity
    {
        [Keyword]
        [Column(width: 100)]
        [TextBox(width: 100, readOnly: false)]
        [Field("单据编号")]
        public string FNumber { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, readOnly: false)]
        [Field("单据名称")]
        public string FName { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, readOnly: false)]
        [Field("单据类型")]
        public string FType { get; set; }

        [Column(width: 90)]
        [CheckBox(readOnly: false, index: 90)]
        [Field("新建")]
        public bool FIsCreate {  get; set; }

        [Column(width: 90)]
        [CheckBox(readOnly: false, index: 90)]
        [Field("删除")]
        public bool FIsDelete {  get; set; }

        [Column(width: 90)]
        [CheckBox(readOnly: false, index: 90)]
        [Field("修改")]
        public bool FIsModify { get; set; }

        [Column(width: 90)]
        [CheckBox(readOnly: false, index: 90)]
        [Field("浏览")]
        public bool FIsQuery {  get; set; }

        [Column(width: 90)]
        [CheckBox(readOnly: false, index: 90)]
        [Field("导出")]
        public bool FIsExport {  get; set; }

        [Column(width: 90)]
        [CheckBox(readOnly: false, index: 90)]
        [Field("打印")]
        public bool FIsPrint { get; set; }

        [Column(width: 90)]
        [CheckBox(readOnly: false, index: 90)]
        [Field("审核")]
        public bool FIsPass {  get; set; }

        [Column(width: 90)]
        [CheckBox(readOnly: false, index: 90)]
        [Field("作废")]
        public bool FIsInvalid {  get; set; }
    }
}
