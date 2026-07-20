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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.jiuhuan.plan.view
{
    public partial class WorkCalendarForm : BaseForm
    {
        private List<Workday> selectedRecords = new List<Workday>();
        private List<Workday> filteredRecords = new List<Workday>();
        private List<MachineTeam> machineTeams = new List<MachineTeam>();

        /// <summary>
        /// 当 dataGridView1 的 DataSource 发生变化时触发的事件类
        /// </summary>
        public class DataGridViewDataSourceChangedEvent : EasyEvent<List<Workday>>{ }         //类似委托
        private DataGridViewDataSourceChangedEvent dataGridViewDataSourceChangedEvent = new DataGridViewDataSourceChangedEvent();

        public WorkCalendarForm()
        {
            InitializeComponent();

        }

        private void WorkCalendarForm_Load(object sender, EventArgs e)
        {
            
            dateTimePicker1.Value = DateTime.Now;
            dateTimePicker2.Value = DateTime.Now.AddMonths(6);

            UV.InitializationDataGridView2<Workday>(dataGridView1);

            //machineTeams = DaoTemplate.FindAll<MachineTeam>(Config.Default.sql_machine_team_findall);

            ////加载机器组
            //foreach (var team in machineTeams)
            //    comboBox2.Items.Add(team.FCode + '-' + team.FName);
            comboBox2.SelectedIndex = 0;

            //加载工作中心
            foreach (var bean in DaoTemplate.FindAll(Config.Default.sql_work_center_findall))
                comboBox1.Items.Add(bean["WORK_CENTER_NAME"]);

            //注册事件
            dataGridViewDataSourceChangedEvent.Register(dataSource => {
                UV.SetDataSourceForGridView(dataGridView1, dataSource);
            });
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // 获取新值
                //var newValue = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            }
        }

        /// <summary>
        /// 双击dataGridView1时，创建并显示ScheduleRecordEditForm，将选中行的ScheduleRecord数据填充到对应控件
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // 验证点击位置是否有效
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            DataGridView dataGridView = sender as DataGridView;
            if (dataGridView == null)
            {
                return;
            }

            // 获取选中的行
            DataGridViewRow selectedRow = dataGridView.Rows[e.RowIndex];

            // 获取该行的数据对象
            var workdayRecord = selectedRow.DataBoundItem as Workday;

            // 验证数据对象是否存在
            if (workdayRecord == null)
            {
                MessageBox.Show("无法获取工序记录数据，请重试。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 创建编辑窗体实例
            using (var editForm = new WorkCalendarEditForm())
            {
                // 设置编辑窗体的数据源
                editForm.SetRecord(workdayRecord);

                // 显示编辑窗体（模态对话框）
                DialogResult result = editForm.ShowDialog();

                // 根据用户操作决定是否更新数据源
                if (result == DialogResult.OK)
                {
                    // 更新原始数据
                    UpdateDataSourceWithEditedRecord(workdayRecord);

                    // 刷新DataGridView显示
                    UpdateUI(sender, e);

                }
            }
        }

        protected void UpdateUI(object sender, EventArgs e)
        {
            查询ToolStripMenuItem_Click(sender, e);
        }

        /// <summary>
        /// 将编辑后的ScheduleRecord更新到数据源中
        /// </summary>
        /// <param name="editedRecord">编辑后的工单记录</param>
        protected void UpdateDataSourceWithEditedRecord(Workday editedRecord)
        {
            // 在实际应用中，这里应该根据具体的业务逻辑来更新数据源
            // 例如：在selectedRecords列表中找到对应的记录并更新
            var recordToUpdate = selectedRecords.FirstOrDefault(r => r.FID == editedRecord.FID);

            if (recordToUpdate != null)
            {
                // 使用反射复制所有公共属性
                var properties = typeof(Workday).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var property in properties)
                {
                    // 跳过只读属性和特殊处理的属性
                    if (!property.CanWrite || property.Name == "FID")
                    {
                        continue;
                    }

                    try
                    {
                        var value = property.GetValue(editedRecord);
                        property.SetValue(recordToUpdate, value);
                    }
                    catch (Exception ex)
                    {
                        // 记录可能的异常但不中断操作
                        Console.WriteLine($"更新属性 {property.Name} 时出错: {ex.Message}");
                    }
                }

                // 保存数据
                UV.SaveAsync(recordToUpdate, this);
            }
        }

        private void 生成ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 获取用户选择的参数
            DateTime begin = dateTimePicker1.Value.Date;
            DateTime end = dateTimePicker2.Value.Date;
            //string selectedDepartment = comboBox1.SelectedItem?.ToString() ?? "";
            string selectedMachineTeam = comboBox2.SelectedItem.ToString();
            List<string> selectedWorkdays = new List<string>();
            Dictionary<string, string> selectedClasses = new Dictionary<string, string>();

            // 获取选中的工作日
            if (checkBox6.Checked) selectedWorkdays.Add("一");
            if (checkBox5.Checked) selectedWorkdays.Add("二");
            if (checkBox4.Checked) selectedWorkdays.Add("三");
            if (checkBox9.Checked) selectedWorkdays.Add("四");
            if (checkBox8.Checked) selectedWorkdays.Add("五");
            if (checkBox7.Checked) selectedWorkdays.Add("六");
            if (checkBox10.Checked) selectedWorkdays.Add("日");

            // 获取选中的班次
            if (checkBox1.Checked)
            {
                var key = "早";
                var value = "";
                var num = 0;
                if (Utility.isNumberic(this.textBox1.Text, out num))
                {
                    value = $"{num},";
                }
                else
                {
                    MessageBox.Show("请输入早班小时数！！！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (Utility.isNumberic(this.textBox4.Text, out num))
                {
                    value += num;
                }
                else
                {
                    MessageBox.Show("请输入早班人数！！！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                selectedClasses[key] = value;
            }

            if (checkBox2.Checked)
            {
                var key = "中";
                var value = "";
                var num = 0;
                if (Utility.isNumberic(this.textBox2.Text, out num))
                {
                    value = $"{num},";
                }
                else
                {
                    MessageBox.Show("请输入中班小时数！！！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (Utility.isNumberic(this.textBox5.Text, out num))
                {
                    value += num;
                }
                else
                {
                    MessageBox.Show("请输入中班人数！！！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                selectedClasses[key] = value;
            }

            if (checkBox3.Checked)
            {
                var key = "夜";
                var value = "";
                var num = 0;
                if (Utility.isNumberic(this.textBox3.Text, out num))
                {
                    value = $"{num},";
                }
                else
                {
                    MessageBox.Show("请输入夜班小时数！！！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (Utility.isNumberic(this.textBox6.Text, out num))
                {
                    value += num;
                }
                else
                {
                    MessageBox.Show("请输入夜班人数！！！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                selectedClasses[key] = value;
            }

            //验证系数
            float factor = 1.0f;
            if (!float.TryParse(textBox7.Text, out factor) || factor <= 0)
            {
                MessageBox.Show("请输入有效的系数值（大于0的数字）！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 验证工作日至少选择一天
            if (selectedWorkdays.Count == 0)
            {
                MessageBox.Show("请至少选择一个工作日！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 验证班次至少选择一个
            if (selectedClasses.Count == 0)
            {
                MessageBox.Show("请至少选择一个班次！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 验证科室不能为空
            //if (string.IsNullOrEmpty(selectedDepartment))
            //{
            //    MessageBox.Show("请选择一个科室！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            // 验证设备组不能为空
            if (string.IsNullOrEmpty(selectedMachineTeam))
            {
                MessageBox.Show("请选择一个设备组！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 生成工作日历
            if ("全部".Equals(selectedMachineTeam))
            {
                foreach (var team in machineTeams)
                    this.selectedRecords.AddRange(Utils.GenerateCustomWorkCalendar(begin, end, $"{team.FCode}-{team.FName}", selectedWorkdays, selectedClasses, factor));
            }
            else
            {
                this.selectedRecords = Utils.GenerateCustomWorkCalendar(begin, end, selectedMachineTeam, selectedWorkdays, selectedClasses, factor);
            }

            dataGridViewDataSourceChangedEvent.Trigger(this.selectedRecords);
        }

        private void 加载ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.selectedRecords = DaoTemplate.FindAll<Workday>();
            selectedRecords.RemoveAll(workday => workday.FWorkDate < DateTime.Now.Date);
            dataGridViewDataSourceChangedEvent.Trigger(this.selectedRecords);
        }

        private async void 导入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.selectedRecords = await ExcelHelper.Import<Workday>();
            dataGridViewDataSourceChangedEvent.Trigger(this.selectedRecords);
        }

        private void 恢复底稿ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridViewDataSourceChangedEvent.Trigger(this.selectedRecords);
        }

        private void 查询ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 获取用户选择的参数
            DateTime begin = dateTimePicker1.Value.Date;
            DateTime end = dateTimePicker2.Value.Date;
            string selectedDepartment = comboBox1.SelectedItem?.ToString() ?? "";
            //string selectedMachineTeam = -1 < comboBox2.SelectedItem.ToString().IndexOf('-') ? comboBox2.SelectedItem.ToString().Split('-')[0] : comboBox2.SelectedItem.ToString();
            string selectedMachineTeam = comboBox2.SelectedItem.ToString() ?? "";
            List<string> selectedClasses = new List<string>();

            // 获取选中的班次
            //if (checkBox1.Checked) selectedClasses.Add("早");
            //if (checkBox2.Checked) selectedClasses.Add("中");
            //if (checkBox3.Checked) selectedClasses.Add("夜");

            // 过滤条件
            filteredRecords = selectedRecords.Where(workday =>
                workday.FWorkDate >= begin &&
                workday.FWorkDate <= end //&&
                //selectedClasses.Contains(workday.FClasses)
            ).ToList();

            // 如果选择了科室，则进一步过滤
            //if (!string.IsNullOrEmpty(selectedDepartment))
            //{
            //    filteredWorkdays = filteredWorkdays.Where(workday => workday.FWorkCenter == selectedDepartment).ToList();
            //}

            // 如果选择了具体的设备组（非“全部”），则进一步过滤
            if (!string.IsNullOrEmpty(selectedMachineTeam) && !"全部".Equals(selectedMachineTeam))
            {
                filteredRecords = filteredRecords.Where(workday => workday.FMachineTeam == selectedMachineTeam).ToList();
            }

            // 设置数据源
            dataGridViewDataSourceChangedEvent.Trigger(this.filteredRecords);
        }

        //private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    if (filteredWorkdays.Count == 0)
        //    {
        //        MessageBox.Show("没有可删除的数据，请先进行查询！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }

        //    var result = MessageBox.Show($"确定要删除筛选出的{filteredWorkdays.Count}条日历数据吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        //    if (result == DialogResult.Yes)
        //    {
        //        // 从selectedWorkdays中移除filteredWorkdays中的数据
        //        foreach (var workday in filteredWorkdays.ToList())
        //        {
        //            this.selectedWorkdays.Remove(workday);
        //        }

        //        // 更新DataGridView显示
        //        dataGridViewDataSourceChangedEvent.Trigger(this.selectedWorkdays);
        //        MessageBox.Show("删除成功！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    }
        //}

        private void 清空ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UV.ClearDataSourceForGridView(dataGridView1, selectedRecords, filteredRecords);
        }

        private void 导出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var workdays = (List<Workday>)this.dataGridView1.DataSource;
            ExcelHelper.Export(workdays);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            查询ToolStripMenuItem_Click(sender, e);
        }

        private void 保存到数据库ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedRecords.Count == 0)
            {
                MessageBox.Show("没有需要保存的数据，请检查！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //DaoTemplate.DeleteAll<Workday>();
            UV.SaveAsync(selectedRecords, this);

            //每次修改工厂日历都要更新已打开窗口的数据
            var formModel = this.GetModel<IFormModel>();
            var forms = formModel.GetOpenedForms();
            foreach (var form in forms)
            {
                if (form is BaseForm baseForm && !(form is WorkCalendarForm))
                {
                    // 使用工具方法调用UpdateData方法（如果存在）
                    Utility.InvokeMethod(baseForm, "UpdateData");
                }
            }
        }

        private void 重置ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show($"确定要删除已保存的所有日历数据吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                DaoTemplate.DeleteAll<Workday>();
                UV.ClearDataSourceForGridView(dataGridView1, selectedRecords, filteredRecords);
                MessageBox.Show("删除成功！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void 删除选中行ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var deleteList = UV.DeleteSelectedRows<Workday>(dataGridView1);

            if (!deleteList.Any())
                return;

            selectedRecords.RemoveAll(workday => deleteList.Contains(workday));

            UV.DeleteAsync(deleteList, this);
        }

        /// <summary>
        /// 删除选中行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            删除选中行ToolStripMenuItem_Click(sender, e);
        }
    }
}
