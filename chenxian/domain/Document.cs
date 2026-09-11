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
    }
}
