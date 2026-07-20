using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{
    [Entity(Title = "排产结果明细", FormName = "ScheduleDetailsResultsForm", EditFormName = "ScheduleDetailsResultsEditForm2", Pagination = false)]
    public class ScheduleDetailsRecord : OperationRecord
    {
        [Ignore("用于DataGridView中显示")]
        [Column(title: "班次", 100, index: 90)]
        public string FClasses2
        {
            get
            {
                string context = "";
                if (null != FClasses)
                {
                    foreach (var key in FClasses.Keys)
                        context += $"{FOperationPlanStartDate.ToString("MM-dd")}/{key}/{FClasses[key]};";
                }
                return context;
            }
        }

        /// <summary>
        /// 班次（日期 -> 班次）
        /// </summary>
        [Serialization("序列化为JSON字符串，再保存到数据库", typeof(Dictionary<string, int>))]
        public Dictionary<string, int> FClasses { get; set; } = new Dictionary<string, int>();

        [Ignore("用于DataGridView中显示")]
        [Column(title: "已排产", 100, index: 95)]
        public string FIndicate { get; set; }
    }
}
