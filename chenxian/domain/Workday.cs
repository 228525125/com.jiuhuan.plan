using System;
using System.Collections.Generic;

namespace com.jiuhuan.plan.domain
{
    [Entity(Title = "工厂日历", FormName = "WorkCalendarForm", EditFormName = "WorkCalendarEditForm")]
    public class Workday : Entity
    {
        [Column("工作日期")]
        [TextBox("工作日期")]
        [Keyword]
        public DateTime FWorkDate { get; set; }

        [Column("班次", 60)]
        [TextBox("班次")]
        [Keyword]
        public string FClasses { get; set; }

        //[Column("科室", 80)]
        public string FWorkCenter { get; set; }

        [Column("设备组")]
        [TextBox("设备组")]
        [Keyword]
        public string FMachineTeam { get; set; }

        [Column("时长（秒）", description: "工作时长（单位：秒）")]
        [TextBox("时长", readOnly: true)]
        public int FDuration { get; set; }

        [Column("时长（小时）", description: "工作时长（单位：小时）")]
        [TextBox("时长（小时）", readOnly: false)]
        public float FDurationByHour { get; set; }

        [Column("系数", 40)]
        [TextBox("系数", readOnly: false)]
        public float FFactor { get; set; }

        [Column("人数", 40)]
        [TextBox("人数", readOnly: false)]
        public int FMultiple { get; set; }

        [Column("备注", 200)]
        [TextBox("备注", readOnly: false)]
        public string FRemark { get; set; } = "";

        /// <summary>
        /// 有效工作时段（起始时间 -> 结束时间）
        /// </summary>
        [Serialization("序列化为JSON字符串，再保存到数据库", typeof(Dictionary<int, int>))]
        public Dictionary<int, int> FWorkTimeSlot { get; set; } = new Dictionary<int, int>();

        [Ignore("用于DataGridView中显示")]
        [Column(title: "有效时段")]
        public string FWorkTimeSlot2
        {
            get
            {
                string context = "";
                if (null != FWorkTimeSlot)
                {
                    foreach (var key in FWorkTimeSlot.Keys)
                        context += $"{key}/{FWorkTimeSlot[key]};";
                }
                return context;
            }
        }

        /// <summary>
        /// 根据当前时间和FWorkTimeSlot字段计算出有效工作时段剩余百分比
        /// </summary>
        /// <returns>剩余百分比，范围0-100</returns>
        public double GetRemainingWorkTimePercentage()
        {
            if (FWorkTimeSlot == null || FWorkTimeSlot.Count == 0)
            {
                //return 100.0; // 如果没有有效工作时段数据，返回0%
                FWorkTimeSlot = new Dictionary<int, int>();
                switch (FClasses)
                {
                    case "早" :
                        FWorkTimeSlot[0] = 0;
                        FWorkTimeSlot[1] = 0;
                        FWorkTimeSlot[2] = 0;
                        FWorkTimeSlot[3] = 0;
                        FWorkTimeSlot[4] = 0;
                        FWorkTimeSlot[5] = 0;
                        FWorkTimeSlot[6] = 0;
                        FWorkTimeSlot[7] = 30;
                        FWorkTimeSlot[8] = 60;
                        FWorkTimeSlot[9] = 60;
                        FWorkTimeSlot[10] = 60;
                        FWorkTimeSlot[11] = 60;
                        FWorkTimeSlot[12] = 30;
                        FWorkTimeSlot[13] = 60;
                        FWorkTimeSlot[14] = 60;
                        FWorkTimeSlot[15] = 60;
                        FWorkTimeSlot[16] = 60;
                        FWorkTimeSlot[17] = 0;
                        FWorkTimeSlot[18] = 0;
                        FWorkTimeSlot[19] = 0;
                        FWorkTimeSlot[20] = 0;
                        FWorkTimeSlot[21] = 0;
                        FWorkTimeSlot[22] = 0;
                        FWorkTimeSlot[23] = 0;
                        break;
                    case "中" :
                        FWorkTimeSlot[0] = 60;
                        FWorkTimeSlot[1] = 60;
                        FWorkTimeSlot[2] = 60;
                        FWorkTimeSlot[3] = 60;
                        FWorkTimeSlot[4] = 0;
                        FWorkTimeSlot[5] = 0;
                        FWorkTimeSlot[6] = 0;
                        FWorkTimeSlot[7] = 0;
                        FWorkTimeSlot[8] = 0;
                        FWorkTimeSlot[9] = 0;
                        FWorkTimeSlot[10] = 0;
                        FWorkTimeSlot[11] = 0;
                        FWorkTimeSlot[12] = 0;
                        FWorkTimeSlot[13] = 0;
                        FWorkTimeSlot[14] = 0;
                        FWorkTimeSlot[15] = 0;
                        FWorkTimeSlot[16] = 0;
                        FWorkTimeSlot[17] = 60;
                        FWorkTimeSlot[18] = 60;
                        FWorkTimeSlot[19] = 60;
                        FWorkTimeSlot[20] = 60;
                        FWorkTimeSlot[21] = 60;
                        FWorkTimeSlot[22] = 60;
                        FWorkTimeSlot[23] = 60;
                        break;
                }
            }

            // 获取当前时间
            DateTime currentTime = DateTime.Now;
            
            // 如果当前日期不等于工作日期，返回0%
            if (currentTime.Date != FWorkDate.Date)
            {
                return 100.0;
            }

            // 计算总的可用工作时长（分钟）
            double totalWorkMinutes = 0;
            foreach (var kvp in FWorkTimeSlot)
            {
                totalWorkMinutes += kvp.Value;
            }

            if (totalWorkMinutes == 0)
            {
                return 100.0; // 如果总工作时长为0，返回0%
            }

            // 获取当前小时
            int currentHour = currentTime.Hour;

            // 计算已过去的工作时长（分钟）
            double elapsedWorkMinutes = 0;
            for (int hour = 6; hour <= currentHour; hour++)    //从6点钟开始算
            {
                if (FWorkTimeSlot.ContainsKey(hour))
                {
                    // 如果是当前小时，只计算已过去的分钟数
                    if (hour == currentHour)
                    {
                        elapsedWorkMinutes += (currentTime.Minute * FWorkTimeSlot[hour]) / 60.0;
                    }
                    else
                    {
                        elapsedWorkMinutes += FWorkTimeSlot[hour];
                    }
                }
            }

            // 计算剩余工作时长
            double remainingWorkMinutes = totalWorkMinutes - elapsedWorkMinutes;

            // 计算剩余百分比
            double percentage = (remainingWorkMinutes / totalWorkMinutes) * 100;

            // 确保结果在0-100范围内
            if (percentage < 0)
            {
                return 0.0;
            }
            else if (percentage > 100)
            {
                return 100.0;
            }
            else
            {
                return Math.Round(percentage, 2); // 保留两位小数
            }
        }
    }
}
