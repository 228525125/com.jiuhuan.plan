using com.jiuhuan.plan.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{
    [Entity(entirety: true, Database = "coc", Title = "客户产品码")]
    public class StampRecordSilianData : Entity
    {
        [Column("客户编码", 200)]
        [TextBox("客户编码", 200)]
        public string FSilianSerialNumber { get; set; }

        [Column("工单号", 150)]
        [TextBox("工单号",150)]
        public string FBillNo { get; set; }
    }
}
