using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{
    public class Mo : Entity
    {
        [Column(title: "工单号")]
        [Keyword]
        public string FBillNo { get; set; }

        [Column("品号")]
        public string FNumber { get; set; }

        [Column("品名")]
        public string FName { get; set; }

        [Column("规格")]
        public string FModel { get; set; }

        [Column("状态", 80)]
        public string FState { get; set; }

        [Column("缺料", 40)]
        public bool FLack { get; set; }                           //是否缺料

        [Column("数量", 60)]
        public float FQty { get; set; }

        [Column("完工数", 70)]
        public float FCompletedQty { get; set; }

        [Column("报废数", 70)]
        public float FScrapQty { get; set; }

        [Column("退料数", 70)]
        public float FReceiptQty { get; set; }

        [Column("计划开工")]
        public DateTime FPlanStartDate { get; set; }

        [Column("计划完工")]
        public DateTime FPlanCompletedDate { get; set; }

        [Column("备注")]
        public string FRemark { get; set; } = "";
    }
}
