using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{

    [Entity(entirety: true, title: "工序参数")]
    public class Technology : Entity
    {
        [Column(width: 100)]
        [TextBox(width: 100)]
        [Popup("请选择品号", IsMultipleColumnReturn = true)]
        [Field("品号")]
        public string FNumber { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, ui: true)]
        [Field("品名")]
        public string FName { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, ui: true)]
        [Field("规格")]
        public string FModel { get; set; }

        [Ignore]
        [OneToMany(typeof(TechnologyD), joinColumn: "FNumber", mappedBy: "FItemCode", title: "工序列表")]
        [Popup("请选择工序明细", multipleRowSelection: true, isMultipleColumnReturn: true)]
        public List<TechnologyD> TechnologyDList { get; set; } = new List<TechnologyD>();
    }
}
