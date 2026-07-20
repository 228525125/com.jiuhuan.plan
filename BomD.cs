using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{
    [Entity(entirety: true, title: "元件清单")]
    public class BomD : Entity
    {
        [Column(width: 100)]
        [TextBox(width: 100, readOnly: false)]
        [Keyword]
        [Field("主件品号")]
        public string FItemCode { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, readOnly: false)]
        [Keyword]
        [Field("元件品号")]
        public string FSubCode { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, ui: true)]
        [Field("元件品名")]
        public string FName { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, ui: true)]
        [Field("元件规格")]
        public string FModel { get; set; }

        [Column(width: 100, readOnly: false)]
        [CheckBox(readOnly: false, index: 90)]
        [Field("附件")]
        public bool FIsAccessory { get; set; }

        [Column(width: 100, readOnly: false)]
        [TextBox(width: 100, readOnly: false)]
        [Field("套数")]
        public int FPackageQty { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, ui: true)]
        [Field("组成用量")]
        public int FQtyPer { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, ui: true)]
        [Field("底数")]
        public int FDeominator { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, ui: true)]
        [Field("生效日期")]
        public DateTime FEffectiveDate { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, ui: true)]
        [Field("失效日期")]
        public DateTime FExprityDate { get; set; }
    }
}
