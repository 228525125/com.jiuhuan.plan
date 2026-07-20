using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{

    [Entity(entirety: true, title: "BOM")]
    public class Bom : Entity
    {
        [Column(width: 100)]
        [TextBox(width: 100)]
        [Popup("请选择品号", IsMultipleColumnReturn = true)]
        [Field("主件品号")]
        public string FNumber { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, ui: true)]
        [Field("主件品名")]
        public string FName { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, ui: true)]
        [Field("主件品名")]
        public string FModel { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, ui: true)]
        [Field("批量增量")]
        public int FStandardBatchQty { get; set; }

        [Ignore]
        [OneToMany(typeof(BomD), joinColumn: "FNumber", mappedBy: "FItemCode", title: "元件清单")]
        [Popup("请选择BOM明细", multipleRowSelection: true, isMultipleColumnReturn: true)]
        public List<BomD> BomDList { get; set; } = new List<BomD>();
    }
}
