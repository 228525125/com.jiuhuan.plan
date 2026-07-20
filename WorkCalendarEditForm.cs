using com.jiuhuan.plan.domain;
using com.jiuhuan.plan.tools;
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
    public partial class WorkCalendarEditForm : EditPopup<Workday>
    {
        public WorkCalendarEditForm()
        {
            InitializeComponent();
        }

        private void WorkCalendarEditForm_Load(object sender, EventArgs e)
        {
            var durationByHourControl = FindControlByName(flowLayoutPanel1, "FDurationByHour");
            var durationControl = FindControlByName(flowLayoutPanel1, "FDuration");
            var factorControl = FindControlByName(flowLayoutPanel1, "FFactor");
            var multipleControl = FindControlByName(flowLayoutPanel1, "FMultiple");

            // 当用户修改时长（小时）时，需要做相应调整
            durationByHourControl.TextChanged += (s, args) =>
            {
                float hour = 8.0f;
                if (!float.TryParse(durationByHourControl.Text, out hour) || hour < 0)
                {
                    MessageBox.Show("请输入有效的时长值！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                float factor = 1.0f;
                if (!float.TryParse(factorControl.Text, out factor) || factor < 0)
                {
                    MessageBox.Show("请输入有效的系数值！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int multiple = 1;
                if (!int.TryParse(multipleControl.Text, out multiple) || multiple <= 0)
                {
                    MessageBox.Show("请输入有效的人数值（大于0的数字）！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var duration = (int)(hour * multiple * 60 * 60 * factor);
                durationControl.Text = duration.ToString();
            };

            multipleControl.TextChanged += (s, args) =>
            {
                float hour = 8.0f;
                if (!float.TryParse(durationByHourControl.Text, out hour) || hour < 0)
                {
                    MessageBox.Show("请输入有效的时长值！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                float factor = 1.0f;
                if (!float.TryParse(factorControl.Text, out factor) || factor < 0)
                {
                    MessageBox.Show("请输入有效的系数值！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int multiple = 1;
                if (!int.TryParse(multipleControl.Text, out multiple) || multiple <= 0)
                {
                    MessageBox.Show("请输入有效的人数值（大于0的数字）！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var duration = (int)(hour * multiple * 60 * 60 * factor);
                durationControl.Text = duration.ToString();
            };

            factorControl.TextChanged += (s, args) =>
            {
                float hour = 8.0f;
                if (!float.TryParse(durationByHourControl.Text, out hour) || hour < 0)
                {
                    MessageBox.Show("请输入有效的时长值！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                float factor = 1.0f;
                if (!float.TryParse(factorControl.Text, out factor) || factor < 0)
                {
                    MessageBox.Show("请输入有效的系数值！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int multiple = 1;
                if (!int.TryParse(multipleControl.Text, out multiple) || multiple <= 0)
                {
                    MessageBox.Show("请输入有效的人数值（大于0的数字）！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var duration = (int)(hour * multiple * 60 * 60 * factor);
                durationControl.Text = duration.ToString();
            };
        }

        protected override FlowLayoutPanel GetFlowLayoutPanel()
        {
            return this.flowLayoutPanel1;
        }

        /// <summary>
        /// 初始化控件并填充数据
        /// </summary>
        /// <param name="record">要编辑的工单记录</param>
        public override void SetRecord(Workday record)
        {
            base.SetRecord(record);

            //var textBoxes = new[] { textBox1, textBox2, textBox3, textBox4, textBox5, textBox6, textBox7, textBox8, textBox9, textBox10, textBox11, textBox12, textBox13, textBox14, textBox15, textBox16, textBox17, textBox18, textBox19, textBox20, textBox21, textBox22, textBox23, textBox24, };
            //int start = 0;
            //int end = 0;

            //if ("早".Equals(record.FClasses))
            //{
            //    start = 7;
            //    end = 16;
            //    for(int i = start; i <= end; i++)
            //    {
            //        var textBox = textBoxes[i-1];
            //        textBox.Text = "60";
            //    }

            //    this.textBox7.Text = "30";
            //    this.textBox12.Text = "30";
            //}
            //else if ("中".Equals(record.FClasses))
            //{
            //    start = 1;
            //    end = 2;
            //    for (int i = start; i <= end; i++)
            //    {
            //        var textBox = textBoxes[i-1];
            //        textBox.Text = "60";
            //    }

            //    start = 17;
            //    end = 24;
            //    for (int i = start; i <= end; i++)
            //    {
            //        var textBox = textBoxes[i-1];
            //        textBox.Text = "60";
            //    }
            //}
            //else
            //{

            //}

            Dictionary<int, int> workTimeSlot = new Dictionary<int, int>();

            switch (record.FClasses)
            {
                case "早":
                    workTimeSlot = JsonHelper.toObject<Dictionary<int, int>>(ConfigHelper.GetConfigKey(ConfigHelper.buffer_workday_work_time_slot_1));
                    FillWorkTimeSlotData(workTimeSlot);
                    break;
                case "中":
                    workTimeSlot = JsonHelper.toObject<Dictionary<int, int>>(ConfigHelper.GetConfigKey(ConfigHelper.buffer_workday_work_time_slot_2));
                    FillWorkTimeSlotData(workTimeSlot);
                    break;
                case "夜":
                    break;
            }

            // 处理FWorkTimeSlot属性
            FillWorkTimeSlotData(record.FWorkTimeSlot);
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
        /// 填充FWorkTimeSlot属性到控件
        /// </summary>
        /// <param name="workTimeSlot">字典</param>
        private void FillWorkTimeSlotData(Dictionary<int, int> workTimeSlot)
        {
            if (workTimeSlot == null || workTimeSlot.Count == 0)
            {
                return;
            }

            var valueControls = new[] { textBox1, textBox2, textBox3, textBox4, textBox5, textBox6, textBox7, textBox8, textBox9, textBox10, textBox11, textBox12, textBox13, textBox14, textBox15, textBox16, textBox17, textBox18, textBox19, textBox20, textBox21, textBox22, textBox23, textBox24, };

            foreach (var kvp in workTimeSlot)
            {
                var index = kvp.Key == 0 ? 24 : kvp.Key;    //24代表0点
                valueControls[index - 1].Text = kvp.Value.ToString();
            }
        }

        /// <summary>
        /// 从控件中提取数据并更新workday对象
        /// </summary>
        protected override void UpdateRecordFromControls()
        {
            base.UpdateRecordFromControls();

            if (this.record == null) return;

            // 更新FWorkTimeSlot属性
            UpdateFWorkTimeSlotDictionary();
        }

        /// <summary>
        /// 更新字典，将控件中的数据填充到Workday的FWorkTimeSlot属性中
        /// </summary>
        private void UpdateFWorkTimeSlotDictionary()
        {
            if (record.FWorkTimeSlot == null)
            {
                record.FWorkTimeSlot = new Dictionary<int, int>();
            }

            // 清空现有数据
            record.FWorkTimeSlot.Clear();

            // 获取日期控件和对应的数值控件
            var keyControls = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 0 };
            var valueControls = new[] { textBox1, textBox2, textBox3, textBox4, textBox5, textBox6, textBox7, textBox8, textBox9, textBox10, textBox11, textBox12, textBox13, textBox14, textBox15, textBox16, textBox17, textBox18, textBox19, textBox20, textBox21, textBox22, textBox23, textBox24, };

            // 判断所有valueControls的值是否为0或空字符串
            bool allZeroOrEmpty = valueControls.All(control => 
                string.IsNullOrEmpty(control.Text) || control.Text == "0");
            
            // 如果所有值都是0或空，则清空字典
            if (allZeroOrEmpty)
            {
                return;
            }


            // 遍历所有控件对，将有效数据添加到字典中
            for (int i = 0; i < keyControls.Length; i++)
            {
                int key = keyControls[i];
                int value = int.Parse(valueControls[i].Text);
                record.FWorkTimeSlot.Add(key, value);
            }

            // 缓存本次修改结果
            switch (record.FClasses)
            {
                case "早":
                    ConfigHelper.SetConfigKey(ConfigHelper.buffer_workday_work_time_slot_1, JsonHelper.toJson(record.FWorkTimeSlot));
                    break;
                case "中":
                    ConfigHelper.SetConfigKey(ConfigHelper.buffer_workday_work_time_slot_2, JsonHelper.toJson(record.FWorkTimeSlot));
                    break;
                case "夜":
                    break;
            }
        }

        /// <summary>
        /// 验证输入数据的有效性
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool ValidateInput()
        {
            var textBoxes = new[] { textBox1, textBox2, textBox3, textBox4, textBox5, textBox6, textBox7, textBox8, textBox9, textBox10, textBox11, textBox12, textBox13, textBox14, textBox15, textBox16, textBox17, textBox18, textBox19, textBox20, textBox21, textBox22, textBox23, textBox24, };

            // 验证所有文本框中的值是否为0-60之间的数字
            foreach (var textBox in textBoxes)
            {
                if (!int.TryParse(textBox.Text, out int value) || value < 0 || value > 60)
                {
                    MessageBox.Show($"请输入有效的数值（0-60）在第 {Array.IndexOf(textBoxes, textBox) + 1} 个时间槽中", "输入验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 清空
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            var valueControls = new[] { textBox1, textBox2, textBox3, textBox4, textBox5, textBox6, textBox7, textBox8, textBox9, textBox10, textBox11, textBox12, textBox13, textBox14, textBox15, textBox16, textBox17, textBox18, textBox19, textBox20, textBox21, textBox22, textBox23, textBox24, };
            for (int i = 0; i < valueControls.Length; i++)
            {
                valueControls[i].Text = "0";
            }

            // 清空现有数据
            record.FWorkTimeSlot.Clear();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            UpdateFWorkTimeSlotDictionary();
        }
    }
}
