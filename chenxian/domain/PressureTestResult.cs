using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{

    [Entity(title: "试压结果明细", entirety: true)]
    [DataGridView_RowDoubleClick("试压记录")]
    public class PressureTestResult : Entity
    {
        [Column(title: "试压日期")]
        public string FTestDate { get; set; }

        [Column(title: "工单号")]
        public string FBillNo { get; set; }

        [Column("品号")]
        public string FNumber { get; set; }

        [Column("品名")]
        public string FName { get; set; }

        [Column("规格")]
        public string FModel { get; set; }

        [Column("阀组编号")]
        public string FValveFullSerialNumber { get; set; }

        [Column("试压结果")]
        public string FResult { get; set; }
        
    }
}
