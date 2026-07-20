using com.jiuhuan.plan.domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.jiuhuan.plan.view
{
    public partial class MachineCapacityCalendarForm : Form
    {
        private List<Workday> _workdays;
        private List<ScheduleRecord> _scheduleRecords;

        public MachineCapacityCalendarForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始化界面并加载数据
        /// </summary>
        /// <param name="workdays">工厂日历数据</param>
        /// <param name="scheduleRecords">工序记录集合</param>
        public void Initialize(List<Workday> workdays, List<ScheduleRecord> scheduleRecords)
        {
            _workdays = workdays ?? throw new ArgumentNullException(nameof(workdays));
            _scheduleRecords = scheduleRecords ?? throw new ArgumentNullException(nameof(scheduleRecords));

            // 清空现有控件
            flowLayoutPanel1.Controls.Clear();

            // 创建并添加每个工作日的面板
            for (int i = 0; i < _workdays.Count; i++)
            {
                var currentWorkday = _workdays[i];
                
                // 添加当前工作日的面板
                CreateWorkdayPanel(currentWorkday);
                
                // 如果不是最后一个工作日，检查是否需要添加月份分隔符
                if (i < _workdays.Count - 1)
                {
                    var nextWorkday = _workdays[i + 1];
                    AddMonthSeparatorPanel(currentWorkday.FWorkDate, nextWorkday.FWorkDate);
                }
            }
        }

        /// <summary>
        /// 创建单个工作日的面板，包含日期、产能占用情况和剩余产能信息
        /// </summary>
        /// <param name="workday">工作日信息</param>
        private void CreateWorkdayPanel(Workday workday)
        {
            var panel = new Panel
            {
                Size = new Size(200, 150), // 增加高度以容纳更多信息
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(3)
            };

            // 创建日期文本框
            var dateTextBox = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 30,
                Multiline = true,
                Text = workday.FWorkDate.ToString("yyyy-MM-dd") + "\r\n" + GetDayOfWeekString(workday.FWorkDate),
                TextAlign = HorizontalAlignment.Center,
                ReadOnly = true,
                Font = new Font("宋体", 9, GraphicsUnit.Point),
                BackColor = Color.White
            };

            // 创建进度条
            var progressBar = new ProgressBar
            {
                Dock = DockStyle.Bottom,
                Height = 20,
                Maximum = CalculateTotalDurationForDate(workday.FWorkDate), // 使用总工作时长作为最大值
                Value = CalculateOccupiedCapacity(workday.FWorkDate),
                ForeColor = Color.Green,
                BackColor = Color.LightGray
            };

            // 创建状态标签（显示占用百分比）
            var statusLabel = new Label
            {
                Dock = DockStyle.Fill,
                Height = 20,
                Text = $"占用: {CalculateOccupiedCapacity(workday.FWorkDate)} / {CalculateTotalDurationForDate(workday.FWorkDate)} ({CalculateOccupancyPercentage(workday.FWorkDate)}%)",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("宋体", 8, GraphicsUnit.Point),
                ForeColor = Color.Black
            };

            // 创建剩余产能标签
            var remainingLabel = new Label
            {
                Dock = DockStyle.Fill,
                Height = 20,
                Text = $"剩余: {CalculateRemainingCapacity(workday.FWorkDate)}",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("宋体", 8, GraphicsUnit.Point),
                ForeColor = Color.Blue
            };

            // 添加控件到面板
            panel.Controls.Add(dateTextBox);
            panel.Controls.Add(statusLabel);
            panel.Controls.Add(remainingLabel);
            panel.Controls.Add(progressBar);

            // 将面板添加到流式布局面板
            flowLayoutPanel1.Controls.Add(panel);
        }

        /// <summary>
        /// 计算指定日期的产能占用百分比
        /// </summary>
        /// <param name="date">目标日期</param>
        /// <returns>产能占用百分比（保留一位小数）</returns>
        private double CalculateOccupancyPercentage(DateTime date)
        {
            var totalOccupied = CalculateOccupiedCapacity(date);
            var totalAvailable = CalculateTotalDurationForDate(date); // 使用新的方法计算总工作时长

            if (totalAvailable == 0)
                return 0;

            return Math.Round((double)totalOccupied / totalAvailable * 100, 1);
        }

        /// <summary>
        /// 计算指定日期的产能剩余量
        /// </summary>
        /// <param name="date">目标日期</param>
        /// <returns>产能剩余量</returns>
        private int CalculateRemainingCapacity(DateTime date)
        {
            var totalOccupied = CalculateOccupiedCapacity(date);
            var totalAvailable = CalculateTotalDurationForDate(date); // 使用新的方法计算总工作时长

            return totalAvailable - totalOccupied;
        }

        /// <summary>
        /// 计算指定日期的总工作时长，考虑该日期的所有班次
        /// </summary>
        /// <param name="date">目标日期</param>
        /// <returns>总工作时长（单位：秒）</returns>
        private int CalculateTotalDurationForDate(DateTime date)
        {
            return _workdays.Where(w => w.FWorkDate == date)
                           .Sum(w => w.FDuration);
        }

        /// <summary>
        /// 计算指定日期的产能占用总量
        /// </summary>
        /// <param name="date">目标日期</param>
        /// <returns>产能占用量</returns>
        private int CalculateOccupiedCapacity(DateTime date)
        {
            int totalOccupied = 0;

            // 遍历所有工序记录，累加对应日期的产能占用
            foreach (var record in _scheduleRecords)
            {
                if (record.FPlanCapacityOccupation != null &&
                    record.FPlanCapacityOccupation.ContainsKey(date))
                {
                    totalOccupied += record.FPlanCapacityOccupation[date];
                }
            }

            return totalOccupied;
        }

        /// <summary>
        /// 根据日期分组创建空白面板作为月份分隔符
        /// </summary>
        private void AddMonthSeparatorPanel(DateTime currentDate, DateTime nextDate)
        {
            // 检查是否跨月
            if (currentDate.Month != nextDate.Month)
            {
                var separatorPanel = new Panel
                {
                    Size = new Size(200, 50), // 设置合适的高度作为分隔符
                    BackColor = Color.LightGray,
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(3)
                };

                var label = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = $"({nextDate.Year}年{nextDate.Month}月)",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("宋体", 9, GraphicsUnit.Point),
                    ForeColor = Color.Black
                };

                separatorPanel.Controls.Add(label);
                flowLayoutPanel1.Controls.Add(separatorPanel);
            }
        }

        /// <summary>
        /// 获取指定日期的星期名称字符串
        /// </summary>
        /// <param name="date">目标日期</param>
        /// <returns>星期名称字符串，如"星期一"</returns>
        private string GetDayOfWeekString(DateTime date)
        {
            // 使用中文星期名称数组
            var dayNames = new[] { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
            
            // 获取日期的星期索引（0=星期日，1=星期一，...，6=星期六）
            int dayIndex = (int)date.DayOfWeek;
            
            // 返回对应的星期名称
            return dayNames[dayIndex];
        }
    }
}
