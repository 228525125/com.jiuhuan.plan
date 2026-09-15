using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{
    [Entity(entirety: true, title: "工序参数列表")]
    public class TechnologyD : Entity
    {
        [Column(width: 100)]
        [Keyword]
        [Field("品号")]
        public string FItemCode { get; set; }

        [Column(width: 100, readOnly:false)]
        [Keyword]
        [Field("工序号")]
        public string FOperationNumber { get; set; }

        [Column(width: 100, readOnly: false)]
        [Field("工艺名称")]
        public string FOperationName { get; set; }

        [Column(width: 100, readOnly: false)]
        [Field("工艺说明")]
        public string FOperationComment { get; set; }

        [Column(width: 100, readOnly: false)]
        [Field("标准工时")]
        public int FStandardWorkHours { get; set; }

        [Column(width: 100, readOnly: false)]
        [Field("辅助工时")]
        public int FAidedWorkHours {  get; set; }
    }
}
