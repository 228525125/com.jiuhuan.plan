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

namespace com.jiuhuan.plan.view
{
    public partial class ScheduleRecordEditForm2 : EditPopup<ScheduleRecord>
    {

        public ScheduleRecordEditForm2()
        {
            InitializeComponent();
        }

        private void ScheduleReocrdEditForm2_Load(object sender, EventArgs e)
        {

        }

        protected override FlowLayoutPanel GetFlowLayoutPanel()
        {
            return this.flowLayoutPanel1;
        }

        /// <summary>
        /// 初始化控件并填充数据
        /// </summary>
        /// <param name="scheduleRecord">要编辑的工单记录</param>
        public override void SetRecord(ScheduleRecord scheduleRecord)
        {
            base.SetRecord(scheduleRecord);

            // 处理FPlanCapacityOccupation字典数据
            FillCapacityOccupationData(scheduleRecord.FPlanCapacityOccupation);

            //dateTimePicker1.Value = "0".Equals(label1.Text) ? DateTime.Now : dateTimePicker1.Value;
            dateTimePicker2.Value = "0".Equals(label2.Text) ? dateTimePicker1.Value.AddDays(1) : dateTimePicker2.Value;
            dateTimePicker3.Value = "0".Equals(label3.Text) ? dateTimePicker2.Value.AddDays(1) : dateTimePicker3.Value;
            dateTimePicker4.Value = "0".Equals(label4.Text) ? dateTimePicker3.Value.AddDays(1) : dateTimePicker4.Value;


        }

        /// <summary>
        /// 当dateTimePicker1值改变时，计算并显示该日期的剩余产能
        /// </summary>
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            CalculateAndDisplayRemainingCapacity(dateTimePicker1.Value, label16);
        }

        /// <summary>
        /// 当dateTimePicker2值改变时，计算并显示该日期的剩余产能
        /// </summary>
        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            CalculateAndDisplayRemainingCapacity(dateTimePicker2.Value, label17);
        }

        /// <summary>
        /// 当dateTimePicker3值改变时，计算并显示该日期的剩余产能
        /// </summary>
        private void dateTimePicker3_ValueChanged(object sender, EventArgs e)
        {
            CalculateAndDisplayRemainingCapacity(dateTimePicker3.Value, label19);
        }

        /// <summary>
        /// 当dateTimePicker4值改变时，计算并显示该日期的剩余产能
        /// </summary>
        private void dateTimePicker4_ValueChanged(object sender, EventArgs e)
        {
            CalculateAndDisplayRemainingCapacity(dateTimePicker4.Value, label21);
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

                // 更新scheduleRecord对象
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
        /// 计算指定日期的剩余产能并在标签上显示
        /// </summary>
        /// <param name="date">要计算产能的日期</param>
        /// <param name="label">显示结果的标签控件</param>
        private void CalculateAndDisplayRemainingCapacity(DateTime date, Label label)
        {
            try
            {
                // 获取ScheduleUtility实例
                var scheduleUtility = this.GetUtility<IScheduleUtility>();

                // 获取当前表单中的工序记录（这里需要根据实际业务逻辑获取完整的records列表）
                var formModel = this.GetModel<IFormModel>();
                var scheduleResultsForm = formModel.GetForm<ScheduleResultsForm>();
                var records = scheduleResultsForm.GetRecords();

                // 获取工厂日历数据（同样需要根据实际业务逻辑获取）
                var workdays = DaoTemplate.FindAll<Workday>();

                // 计算剩余产能
                int remainingCapacity = scheduleUtility.CalculateRemainingCapacity(workdays, records, date, record.FMachineTeam);

                // 显示结果
                label.Text = $"{remainingCapacity}秒";
            }
            catch (Exception ex)
            {
                label.Text = "计算失败";
                // 可以考虑记录日志或者显示错误信息
                Console.WriteLine($"计算日期 {date:yyyy-MM-dd} 的剩余产能时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 填充产能占用数据到控件
        /// </summary>
        /// <param name="capacityOccupation">产能占用字典</param>
        private void FillCapacityOccupationData(Dictionary<DateTime, int> capacityOccupation)
        {
            if (capacityOccupation == null || capacityOccupation.Count == 0)
            {
                return;
            }

            // 这里假设只有4个日期控件和对应的文本框
            var dateControls = new[] { dateTimePicker1, dateTimePicker2, dateTimePicker3, dateTimePicker4 };
            //var valueControls = new[] { textBox10, textBox9, textBox8, textBox7 };
            var valueControls = new[] { label1, label2, label3, label4 };

            int index = 0;
            foreach (var kvp in capacityOccupation)
            {
                if (index >= dateControls.Length) break;

                dateControls[index].Value = kvp.Key;
                valueControls[index].Text = kvp.Value.ToString();
                index++;
            }
        }

        /// <summary>
        /// 从控件中提取数据并更新scheduleRecord对象
        /// </summary>
        protected override void UpdateRecordFromControls()
        {
            base.UpdateRecordFromControls();

            if (this.record == null) return;

            // 更新产能占用字典
            UpdateCapacityOccupationDictionary();
        }

        /// <summary>
        /// 更新产能占用字典，将控件中的数据填充到ScheduleRecord的FPlanCapacityOccupation属性中
        /// </summary>
        private void UpdateCapacityOccupationDictionary()
        {

            int sumOfWorkHours = GetSumOfWorkHours();

            // 表示没有进行改变操作
            if (0 == sumOfWorkHours)
                return;

            if (record.FPlanCapacityOccupation == null)
            {
                record.FPlanCapacityOccupation = new Dictionary<DateTime, int>();
            }

            // 清空现有数据
            record.FPlanCapacityOccupation.Clear();

            // 获取日期控件和对应的数值控件
            var dateControls = new[] { dateTimePicker1, dateTimePicker2, dateTimePicker3, dateTimePicker4 };
            var valueControls = new[] { textBox10, textBox9, textBox8, textBox7 };

            // 遍历所有控件对，将有效数据添加到字典中
            for (int i = 0; i < dateControls.Length; i++)
            {
                DateTime date = dateControls[i].Value;
                string valueText = valueControls[i].Text.Trim();

                // 验证数值是否有效
                if (!string.IsNullOrEmpty(valueText) && int.TryParse(valueText, out int capacity))
                {
                    // 如果容量大于0，则添加到字典中
                    if (capacity > 0)
                    {
                        record.FPlanCapacityOccupation.Add(date, capacity);
                    }
                }
            }

            // 赋值开始日期和完工日期
            if (record.FPlanCapacityOccupation.Any())
            {
                record.FPlanCapacityOccupation = record.FPlanCapacityOccupation.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                record.FOperationPlanStartDate = record.FPlanCapacityOccupation.Keys.First();
                record.FOperationPlanCompleteDate = record.FPlanCapacityOccupation.Keys.Last();
            }
        }

        private int GetSumOfWorkHours()
        {
            int sumOfWorkHours = 0;
            var workHourControls = new[] { textBox10, textBox9, textBox8, textBox7 };
            foreach (var control in workHourControls)
            {
                if (int.TryParse(control.Text, out int workHour))
                {
                    sumOfWorkHours += workHour;
                }
            }

            return sumOfWorkHours;
        }

        /// <summary>
        /// 验证输入数据的有效性
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool ValidateInput()
        {
            // 验证工时数是否等于各部分之和
            var needWorkHoursControl = FindControlByName(flowLayoutPanel1, "FNeedWorkHours");
            if (!int.TryParse(needWorkHoursControl.Text, out int totalWorkHours))
            {
                MessageBox.Show("请输入有效的总工时数。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                needWorkHoursControl.Focus();
                return false;
            }

            int sumOfWorkHours = GetSumOfWorkHours();

            // 表示没有进行改变操作
            if (0 == sumOfWorkHours)
                return true;

            if (sumOfWorkHours != totalWorkHours)
            {
                MessageBox.Show("各部分工时数之和必须等于总工时数。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 验证 textBox4 的值不能大于 label16 的值
            if (!ValidateCapacity(textBox10.Text, label1.Text, label16.Text, "第一个日期产能"))
            {
                textBox10.Focus();
                return false;
            }

            // 验证 textBox5 的值不能大于 label17 的值
            if (!ValidateCapacity(textBox9.Text, label2.Text, label17.Text, "第二个日期产能"))
            {
                textBox9.Focus();
                return false;
            }

            // 验证 textBox6 的值不能大于 label19 的值
            if (!ValidateCapacity(textBox8.Text, label3.Text, label19.Text, "第三个日期产能"))
            {
                textBox8.Focus();
                return false;
            }

            // 验证 textBox7 的值不能大于 label21 的值
            if (!ValidateCapacity(textBox7.Text, label4.Text, label21.Text, "第四个日期产能"))
            {
                textBox7.Focus();
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
                DialogResult result = MessageBox.Show($"输入的{capacityType}({inputCapacity}秒)不能大于可用产能({availableCapacity}秒)。\n是否继续？",
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
                // 将needWorkHours - (textBox9.Text + textBox8.Text + textBox7.Text)的值赋值给textBox10.Text
                var needWorkHoursControl = FindControlByName(flowLayoutPanel1, "FNeedWorkHours");
                if (!int.TryParse(needWorkHoursControl.Text, out int needWorkHours))
                {
                    MessageBox.Show("请输入有效的总工时数。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    needWorkHoursControl.Focus();
                }

                int num9 = string.IsNullOrWhiteSpace(textBox9.Text) ? 0 : int.Parse(textBox9.Text); 
                int num8 = string.IsNullOrWhiteSpace(textBox8.Text) ? 0 : int.Parse(textBox8.Text);
                int num7 = string.IsNullOrWhiteSpace(textBox7.Text) ? 0 : int.Parse(textBox7.Text);


                int num10 = needWorkHours - (num9 + num8 + num7);

                textBox10.Text = num10.ToString();
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
                // 将needWorkHours - (textBox10.Text + textBox8.Text + textBox7.Text)的值赋值给textBox9.Text
                var needWorkHoursControl = FindControlByName(flowLayoutPanel1, "FNeedWorkHours");
                if (!int.TryParse(needWorkHoursControl.Text, out int needWorkHours))
                {
                    MessageBox.Show("请输入有效的总工时数。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    needWorkHoursControl.Focus();
                }

                int num10 = string.IsNullOrWhiteSpace(textBox10.Text) ? 0 : int.Parse(textBox10.Text);
                int num8 = string.IsNullOrWhiteSpace(textBox8.Text) ? 0 : int.Parse(textBox8.Text);
                int num7 = string.IsNullOrWhiteSpace(textBox7.Text) ? 0 : int.Parse(textBox7.Text);


                int num9 = needWorkHours - (num10 + num8 + num7);

                textBox9.Text = num9.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"自动计算早班工时失败：{ex.Message}", "计算错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                // 将needWorkHours - (textBox10.Text + textBox9.Text + textBox7.Text)的值赋值给textBox8.Text
                var needWorkHoursControl = FindControlByName(flowLayoutPanel1, "FNeedWorkHours");
                if (!int.TryParse(needWorkHoursControl.Text, out int needWorkHours))
                {
                    MessageBox.Show("请输入有效的总工时数。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    needWorkHoursControl.Focus();
                }

                int num10 = string.IsNullOrWhiteSpace(textBox10.Text) ? 0 : int.Parse(textBox10.Text);
                int num9 = string.IsNullOrWhiteSpace(textBox9.Text) ? 0 : int.Parse(textBox9.Text);
                int num7 = string.IsNullOrWhiteSpace(textBox7.Text) ? 0 : int.Parse(textBox7.Text);


                int num8 = needWorkHours - (num10 + num9 + num7);

                textBox8.Text = num8.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"自动计算早班工时失败：{ex.Message}", "计算错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                // 将needWorkHours - (textBox10.Text + textBox9.Text + textBox8.Text)的值赋值给textBox7.Text
                var needWorkHoursControl = FindControlByName(flowLayoutPanel1, "FNeedWorkHours");
                if (!int.TryParse(needWorkHoursControl.Text, out int needWorkHours))
                {
                    MessageBox.Show("请输入有效的总工时数。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    needWorkHoursControl.Focus();
                }

                int num10 = string.IsNullOrWhiteSpace(textBox10.Text) ? 0 : int.Parse(textBox10.Text);
                int num9 = string.IsNullOrWhiteSpace(textBox9.Text) ? 0 : int.Parse(textBox9.Text);
                int num8 = string.IsNullOrWhiteSpace(textBox8.Text) ? 0 : int.Parse(textBox8.Text);


                int num7 = needWorkHours - (num10 + num9 + num8);

                textBox7.Text = num7.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"自动计算早班工时失败：{ex.Message}", "计算错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
