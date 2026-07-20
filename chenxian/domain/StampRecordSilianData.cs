using com.jiuhuan.plan.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{
    [Entity(entirety: true, Database = "coc", Title = "四联二次阀编码")]
    public class StampRecordSilianData : Entity
    {
        [Column("二次阀编码", 200)]
        [TextBox("二次阀编码", 200)]
        [Keyword]
        public string FSilianSerialNumber { get; set; }

        [Column("工单号", 150)]
        [TextBox("工单号",150)]
        [Keyword]
        public string FBillNo { get; set; }
    }
}
