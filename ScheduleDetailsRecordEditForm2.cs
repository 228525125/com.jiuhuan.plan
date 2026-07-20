using com.jiuhuan.plan.domain;
using com.jiuhuan.plan.models;
using com.jiuhuan.plan.tools;
using QFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.jiuhuan.plan
{
    public partial class ScheduleDetailsRecordEditForm2 : EditPopup<ScheduleDetailsRecord>
    {
        public ScheduleDetailsRecordEditForm2()
        {
            InitializeComponent();
        }

        private void ScheduleDetailsRecordEditForm2_Load(object sender, EventArgs e)
        {
            var operationPlanStartDateControl = FindControlByName(flowLayoutPanel1, "FOperationPlanStartDate");
            var operationPlanCompleteDateControl = FindControlByName(flowLayoutPanel1, "FOperationPlanCompleteDate");
            var machineTeamControl = FindControlByName<ComboBox>(flowLayoutPanel1, "FMachineTeam");
            var remainQtyControl = FindControlByName<TextBox>(flowLayoutPanel1, "FRemainQty");
            var needWorkHoursControl = FindControlByName<TextBox>(flowLayoutPanel1, "FNeedWorkHours");

            // 当用户修改计划开工日期时，需要做相应调整
            if (operationPlanStartDateControl is DateTimePicker startDatePicker)
            {
                startDatePicker.ValueChanged += (s, args) =>
                {
                    if (operationPlanCompleteDateControl is DateTimePicker completeDatePicker)
                    {
                        completeDatePicker.Value = startDatePicker.Value;        // 使计划完工日期一致

                        // 重新计算产能
                        var targetDate = startDatePicker.Value.Date;
                        var machineTeam = machineTeamControl.SelectedItem.ToString();
                        var days = workdays.FindAll(w => w.FWorkDate.Date == targetDate && w.FMachineTeam == machineTeam);

                        foreach (var workday in days)
                        {
                            // 显示该班次的剩余产能
                            DisplayRemainingCapacityByClasses(workday);
                        }

                        user.SetSettings(User.FOperationPlanStartDate, startDatePicker.Value);
                        DaoTemplate.Save(user);
                    }
                };

                var isStartupOperationPlanStartDate = null != user.GetSettings(User.IsStartupOperationPlanStartDate) ? (bool)user.GetSettings(User.IsStartupOperationPlanStartDate) : false ;
                if (isStartupOperationPlanStartDate)
                    startDatePicker.Value = (DateTime) user.GetSettings(User.FOperationPlanStartDate);
            }

            remainQtyControl.TextChanged += (s, args) => 
            {
                // 验证剩余数量是否为有效整数
                if (!int.TryParse(remainQtyControl.Text, out int remainQty))
                {
                    // 如果输入无效，将所需工时设为0或保持原状，避免程序崩溃
                    return;
                }
                var newValue = remainQty * record.FStandardWorkHours + record.FAidedWorkHours;
                needWorkHoursControl.Text = newValue.ToString();
            };
        }

        protected override FlowLayoutPanel GetFlowLayoutPanel()
        {
            return this.flowLayoutPanel1;
        }

        public override void SetRecord(ScheduleDetailsRecord record)
        {
            base.SetRecord(record);

            // 处理FClasses字典数据
            FillClassesData(record.FClasses);

            // 查找与计划开始日期匹配的工厂日历记录
            var targetDate = record.FOperationPlanStartDate.Date;
            var machineTeam = record.FMachineTeam;
            var days = workdays.FindAll(w => w.FWorkDate.Date == targetDate && w.FMachineTeam == machineTeam);

            foreach (var workday in days)
            {
                // 显示该班次的剩余产能
                DisplayRemainingCapacityByClasses(workday);
            }
        }

        /// <summary>
        /// 填充产能占用数据到控件
        /// </summary>
        /// <param name="classes">产能占用字典</param>
        private void FillClassesData(Dictionary<string, int> classes)
        {
            if (classes == null || classes.Count == 0)
            {
                return;
            }

            foreach (var kvp in classes)
            {
                // 根据kvp.Key查找对应的日期控件并填充日期值
                switch (kvp.Key)
                {
                    case "早":
                        //textBox4.Text = kvp.Value.ToString();
                        label1.Text = kvp.Value.ToString();
                        break;
                    case "中":
                        //textBox5.Text = kvp.Value.ToString();
                        label2.Text = kvp.Value.ToString();
                        break;
                    case "夜":
                        //textBox6.Text = kvp.Value.ToString();
                        label3.Text = kvp.Value.ToString();
                        break;
                    default:

                        break;
                }
            }
        }

        /// <summary>
        /// 从控件中提取数据并更新scheduleDetailsRecord对象
        /// </summary>
        protected override void UpdateRecordFromControls()
        {
            base.UpdateRecordFromControls();

            if (record == null) return;

            // 更新班次字典
            UpdateClassesDictionary();
        }

        /// <summary>
        /// 更新班次字典
        /// </summary>
        private void UpdateClassesDictionary()
        {
            int totalCapacity = GetTotalCapacity();
            
                // 表示没有进行改变操作
                if (0 == totalCapacity)
                    return ;

            if (record.FClasses == null)
            {
                record.FClasses = new Dictionary<string, int>();
            }

            // 清空现有数据
            record.FClasses.Clear();

            var dateControls = new[] { textBox7, textBox14, textBox15 };
            var valueControls = new[] { textBox4, textBox5, textBox6 };

            // 遍历所有控件对，将有效数据添加到字典中
            for (int i = 0; i < dateControls.Length; i++)
            {
                string date = dateControls[i].Text;
                string valueText = valueControls[i].Text.Trim();

                // 验证数值是否有效
                if (!string.IsNullOrEmpty(valueText) && int.TryParse(valueText, out int capacity))
                {
                    // 如果容量大于0，则添加到字典中
                    if (capacity > 0)
                    {
                        record.FClasses.Add(date, capacity);
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // 验证输入数据
                if (!ValidateInput())
                {
                    return;
                }



                // 更新scheduleDetailsRecord对象
                UpdateRecordFromControls();

                // 关闭窗体并返回OK结果
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// 计算指定班次的剩余产能
        /// </summary>
        /// <param name="classesText">班次文本，格式应为 "yyyy-MM-dd/班次名称"</param>
        /// <param name="label">显示结果的标签控件</param>
        private int CalculateRemainingCapacity(DateTime date, string classes, string machineTeam)
        {
            // 获取ScheduleUtility实例
            var scheduleUtility = this.GetUtility<IScheduleUtility>();

            // 获取当前表单中的工序记录（这里需要根据实际业务逻辑获取完整的records列表）
            var formModel = this.GetModel<IFormModel>();
            var scheduleDetailsResultsForm = formModel.GetForm<ScheduleDetailsResultsForm>();
            var records = scheduleDetailsResultsForm.GetRecords();

            // 计算剩余产能
            int remainingCapacity = scheduleUtility.CalculateRemainingCapacityForClasses(workdays, records, date, classes, machineTeam);
            return remainingCapacity;
        }

        /// <summary>
        /// 显示各个班次的剩余产能
        /// </summary>
        /// <param name="workday">工厂日历</param>
        /// <param name="date">查询日期</param>
        /// <param name="records">工序记录</param>
        private void DisplayRemainingCapacityByClasses(Workday workday)
        {
            try
            {
                // 获取指定日期的所有班次
                var classes = workday.FClasses;
                var date = workday.FWorkDate.Date;
                string machineTeam = workday.FMachineTeam;

                // 计算并显示每个班次的剩余产能
                int remainingCapacity = CalculateRemainingCapacity(date, classes, machineTeam);

                // 根据班次名称显示在对应的标签上
                switch (classes)
                {
                    case "早":
                        label16.Text = $"{remainingCapacity}秒";
                        break;
                    case "中":
                        label17.Text = $"{remainingCapacity}秒";
                        break;
                    case "夜":
                        label19.Text = $"{remainingCapacity}秒";
                        break;
                    default:
                        // 对于其他班次，可以选择忽略或显示在默认位置
                        break;
                }
            }
            catch (Exception ex)
            {
                // 设置错误显示
                label16.Text = "计算失败";
                label17.Text = "计算失败";
                label19.Text = "计算失败";

                // 记录错误日志
                Console.WriteLine($"计算班次剩余产能时发生错误: {ex.Message}");
            }
        }

        private int GetTotalCapacity()
        {
            int totalCapacity = 0;

            if (!string.IsNullOrWhiteSpace(textBox4.Text) && int.TryParse(textBox4.Text, out int capacity4))
            {
                totalCapacity += capacity4;
            }

            if (!string.IsNullOrWhiteSpace(textBox5.Text) && int.TryParse(textBox5.Text, out int capacity5))
            {
                totalCapacity += capacity5;
            }

            if (!string.IsNullOrWhiteSpace(textBox6.Text) && int.TryParse(textBox6.Text, out int capacity6))
            {
                totalCapacity += capacity6;
            }

            return totalCapacity;
        }

        /// <summary>
        /// 验证输入数据的有效性
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool ValidateInput()
        {

            var needWorkHoursControl = FindControlByName(flowLayoutPanel1, "FNeedWorkHours");
            if (!int.TryParse(needWorkHoursControl.Text, out int needWorkHours))
            {
                MessageBox.Show("请输入有效的总工时数。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                needWorkHoursControl.Focus();
                return false;
            }

            // 验证 textBox4、textBox5、textBox6 的值加起来不能大于 textBox3 的值
            int totalCapacity = GetTotalCapacity();

            // 表示没有进行改变操作
            if (0 == totalCapacity)
                return true;

            if (totalCapacity != needWorkHours)
            {
                MessageBox.Show($"分配的总时长({totalCapacity}秒)必须等于所需产能({needWorkHours}秒)。",
                                "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 验证 textBox4 的值不能大于 label16 的值
            if (!"0".Equals(textBox4.Text) && !ValidateCapacity(textBox4.Text, label1.Text, label16.Text, "早班产能"))
            {
                textBox4.Focus();
                return false;
            }

            // 验证 textBox5 的值不能大于 label17 的值
            if (!"0".Equals(textBox5.Text) && !ValidateCapacity(textBox5.Text, label2.Text, label17.Text, "中班产能"))
            {
                textBox5.Focus();
                return false;
            }

            // 验证 textBox6 的值不能大于 label19 的值
            if (!"0".Equals(textBox6.Text) && !ValidateCapacity(textBox6.Text, label3.Text, label19.Text, "夜班产能"))
            {
                textBox6.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 验证产能值是否有效
        /// </summary>
        /// <param name="inputValue">输入值</param>
        /// <param name="oldValue">前值</param>
        /// <param name="capacityText">产能标签文本</param>
        /// <param name="capacityType">产能类型</param>
        /// <returns>验证是否通过</returns>
        private bool ValidateCapacity(string inputValue, string oldValue, string capacityText, string capacityType)
        {
            // 检查输入值是否为空
            if (string.IsNullOrWhiteSpace(inputValue))
            {
                // 如果输入为空，视为有效
                return true;
            }

            // 检查输入值是否为有效数字
            if (!int.TryParse(inputValue, out int inputCapacity))
            {
                MessageBox.Show($"请输入有效的{capacityType}数值。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 前值有错误
            if (!int.TryParse(oldValue, out int oldCapacity))
            {
                MessageBox.Show($"前值有错误。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 检查产能标签文本是否为有效数字
            if (string.IsNullOrWhiteSpace(capacityText) || capacityText == "计算失败")
            {
                MessageBox.Show($"{capacityType}数据不可用，请检查计算结果。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 从标签文本中提取数字部分
            string capacityValueStr = capacityText.Replace("秒", "").Trim();
            if (!int.TryParse(capacityValueStr, out int availableCapacity))
            {
                MessageBox.Show($"{capacityType}数据格式不正确，请重新计算。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 验证输入值不能大于可用产能
            if (inputCapacity > (oldCapacity + availableCapacity))
            {
                DialogResult result = MessageBox.Show($"输入的{capacityType}({inputCapacity}秒)不能大于可用产能({oldCapacity + availableCapacity}秒)。\n是否继续？", 
                                "验证警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                {
                    return false;
                }
            }

            return true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                // 将needWorkHours - (textBox5.Text + textBox6.Text)的值赋值给textBox4.Text
                var needWorkHoursControl = FindControlByName(flowLayoutPanel1, "FNeedWorkHours");
                if (!int.TryParse(needWorkHoursControl.Text, out int needWorkHours))
                {
                    MessageBox.Show("请输入有效的总工时数。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    needWorkHoursControl.Focus();
                }

                int middleShift = string.IsNullOrWhiteSpace(textBox5.Text) ? 0 : int.Parse(textBox5.Text); // 中班工时
                int nightShift = string.IsNullOrWhiteSpace(textBox6.Text) ? 0 : int.Parse(textBox6.Text); // 夜班工时

                // 计算早班工时
                int earlyShift = needWorkHours - (middleShift + nightShift);

                // 设置早班工时到textBox4
                textBox4.Text = earlyShift.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"自动计算早班工时失败：{ex.Message}", "计算错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                // 将needWorkHours - (textBox4.Text + textBox6.Text)的值赋值给textBox5.Text
                var needWorkHoursControl = FindControlByName(flowLayoutPanel1, "FNeedWorkHours");
                if (!int.TryParse(needWorkHoursControl.Text, out int needWorkHours))
                {
                    MessageBox.Show("请输入有效的总工时数。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    needWorkHoursControl.Focus();
                }
                int earlyShift = string.IsNullOrWhiteSpace(textBox4.Text) ? 0 : int.Parse(textBox4.Text); // 早班工时
                int nightShift = string.IsNullOrWhiteSpace(textBox6.Text) ? 0 : int.Parse(textBox6.Text); // 夜班工时

                // 计算中班工时
                int middleShift = needWorkHours - (earlyShift + nightShift);

                // 设置中班工时到textBox5
                textBox5.Text = middleShift.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"自动计算中班工时失败：{ex.Message}", "计算错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                // 将needWorkHours - (textBox4.Text + textBox5.Text)的值赋值给textBox6.Text
                var needWorkHoursControl = FindControlByName(flowLayoutPanel1, "FNeedWorkHours");
                if (!int.TryParse(needWorkHoursControl.Text, out int needWorkHours))
                {
                    MessageBox.Show("请输入有效的总工时数。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    needWorkHoursControl.Focus();
                }
                int earlyShift = string.IsNullOrWhiteSpace(textBox4.Text) ? 0 : int.Parse(textBox4.Text); // 早班工时
                int middleShift = string.IsNullOrWhiteSpace(textBox5.Text) ? 0 : int.Parse(textBox5.Text); // 中班工时

                // 计算夜班工时
                int nightShift = needWorkHours - (earlyShift + middleShift);

                // 设置夜班工时到textBox6
                textBox6.Text = nightShift.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"自动计算夜班工时失败：{ex.Message}", "计算错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
    }
}
