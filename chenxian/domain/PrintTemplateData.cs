using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{
    [Entity(entirety: true, Database = "coc", Title = "阀组变送器编号")]
    public class PrintTemplateData : Entity
    {
        [Column(width: 100)]
        [TextBox(width: 100, readOnly: false)]
        [Field("序号")]
        public int FSequenceNumber { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, readOnly: false)]
        [Field("阀组编号")]
        public string FValveSerialNumber { get; set; }

        [Keyword]
        [Column(width: 100)]
        [TextBox(width: 100, readOnly: false)]
        [Field("变送器号")]
        public string FDeviceNumber { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, readOnly: false)]
        [Field("客户规格")]
        public string FCustomerModel { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, readOnly: false)]
        [Field("跟踪单号")]
        public string FTrackNo { get; set; }
    }
}
