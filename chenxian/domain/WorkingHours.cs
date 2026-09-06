using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{

    [Entity(title: "员工工时统计", entirety: true)]
    //[DataGridView_RowDoubleClick("报工明细")]
    public class WorkingHours : Entity
    {
        [Column(title: "报工时间")]
        public string FWorkDate { get; set; }

        [Column(title: "工单号")]
        public string FBillNo { get; set; }

        [Column("品号")]
        public string FNumber { get; set; }

        [Column("品名")]
        public string FName { get; set; }

        [Column("规格")]
        public string FModel { get; set; }

        [Column("工序")]
        public string FOperationName { get; set; }

        [Column("员工")]
        public string FEmployeeName { get; set; }

        [Column("数量")]
        public float FQty { get; set; }

        [Column("工时")]
        public float FDuration { get; set; }
        
    }
}
