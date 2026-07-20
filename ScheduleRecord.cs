using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{
    [Entity(Title = "排产结果", FormName = "ScheduleResultsForm", EditFormName = "ScheduleResultsEditForm2", Pagination = false)]
    public class ScheduleRecord : OperationRecord
    {
        /// <summary>
        /// 是否固定，即人工指定开工时间、完工时间，参与计算时固定不变
        /// </summary>
        [Column("固定", 40, index: 90, readOnly: false)]
        [CheckBox("固定", false, index: 90)]
        public bool FFixed { get; set; }

        /// <summary>
        /// 计划产能占用（日期 -> 产能）
        /// </summary>
        [Serialization("序列化为JSON字符串，再保存到数据库", typeof(Dictionary<DateTime, int>))]
        public Dictionary<DateTime, int> FPlanCapacityOccupation { get; set; } = new Dictionary<DateTime, int>();

        [Ignore("用于DataGridView中显示")]
        [Column(title: "产能分配", index:95)]
        public string FPlanCapacityOccupation2 { get {
                string context = "";
                if(null != FPlanCapacityOccupation)
                {
                    foreach (var key in FPlanCapacityOccupation.Keys)
                        context += $"{key.ToString("yyyy-MM-dd")}/{FPlanCapacityOccupation[key]};";
                }
                return context;
            } 
        }
    }
}
