using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan
{
    public class MoInventoryStatusDetails
    {
        [Column("品号", 80)]
        public string FNumber { get; set; }

        [Column("品名")]
        public string FName { get; set; }

        [Column("规格")]
        public string FModel { get; set; }

        [Column("查缺", 80)]
        public int FQty { get; set; }

        [Column("需求数", 80)]
        public float FRequiredQuantity { get; set; }

        [Column("库存数", 80)]
        public float FInventoryQuantity { get; set; }

        [Column("预计入库数", 120)]
        public int FPlanStockQuantity { get; set; }

        [Column("预计完工日期")]
        public DateTime FPlanCompleteDate { get; set; }

        [Column("工单号")]
        public string FBillNo { get; set; }

        [Column("库存状态")]
        public string FInventoryStatus { get; set; }
    }
}
