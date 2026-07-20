using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{
    [Entity(entirety: true, databaseName: "coc", title: "附件扫码记录")]
    public class ValveAccessoryScanRecord : Entity
    {
        [Column("附件二维码", 200)]
        [TextBox("附件二维码", 200)]
        public string FQrcode { get; set; }

        [Column("工单号", 150)]
        [TextBox("工单号", 150)]
        public string FBillNo { get; set; }

        [Column("阀组二维码", 200)]
        [TextBox("阀组二维码", 200)]
        public string FSerialNumber { get; set; }
    }
}
