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
    public partial class ProcessResultsForm : GridViewForm<OperationRecord>
    {
        public ProcessResultsForm()
        {
            InitializeComponent();
        }

        private void ProcessResultsForm_Load(object sender, EventArgs e)
        {
            InitializeData();

            dateTimePicker1.Value = DateTime.Now;
            dateTimePicker2.Value = DateTime.Now;

            comboBox1.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
         
        }

        protected override string Sql()
        {
            return Config.Default.v_operation_record_findall;
        }

        protected override SplitContainer GetSplitContainer()
        {
            return splitContainer1;
        }

        protected override DataGridView GetDataGridView1()
        {
            return dataGridView1;
        }

        protected override DataGridView GetDataGridView2()
        {
            return dataGridView2;
        }

        protected override void FilterBills1()
        {
            // 获取用户选择的参数
            DateTime startDate = dateTimePicker1.Value.Date;
            DateTime endDate = dateTimePicker2.Value.Date;
            string selectedStatus = comboBox1.SelectedItem?.ToString() ?? "";
            
            string queryText = textBox1.Text.Trim();

            // 过滤条件
            filteredRecords1 = selectedRecords.Where(bill =>
                bill.FDate.Date >= startDate &&
                bill.FDate.Date <= endDate &&
                (string.IsNullOrEmpty(selectedStatus) || "全部".Equals(selectedStatus) || bill.FState == selectedStatus) &&
                GetValidityStatus(bill) &&
                (string.IsNullOrEmpty(queryText) ||
                bill.FBillNo.Contains(queryText) ||
                bill.FNumber.Contains(queryText) ||
                bill.FName.Contains(queryText) ||
                bill.FModel.Contains(queryText))
            ).ToList();
        }

        private bool GetValidityStatus(OperationRecord record)
        {
            string validityStatus = comboBox4.SelectedItem?.ToString() ?? "";
            if ("全部".Equals(validityStatus))
                return true;

            if ("合法".Equals(validityStatus))
            {
                if (Color.LightGreen.Equals(record.RowBackColor))
                    return true;
                else
                    return false;
            }

            if ("不合法".Equals(validityStatus))
            {
                if (Color.Red.Equals(record.RowBackColor))
                    return true;
                else
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 验证合法性
        /// </summary>
        protected override bool ValidateRecord()
        {
            bool allValid = true;
            foreach (DataGridViewRow row in GetDataGridView1().Rows)
            {
                if (row.IsNewRow) continue; // 跳过新行

                // 获取"需要工时"列的值
                var needWorkHoursCell = row.Cells["FNeedWorkHours"];
                var machineTeamCell = row.Cells["FMachineTeam"];
                var standardWorkHoursCell = row.Cells["FStandardWorkHours"];
                bool isValid = true;

                // 检查标准工时列
                if (standardWorkHoursCell != null && standardWorkHoursCell.Value != null)
                {
                    int standardWorkHours;
                    if (int.TryParse(standardWorkHoursCell.Value.ToString(), out standardWorkHours))
                    {
                        // 如果需要工时为0，则标记为无效
                        if (standardWorkHours == 0)
                        {
                            isValid = false;
                        }
                    }
                    else
                    {
                        isValid = false; // 无法解析为整数也视为无效
                    }
                }
                else
                {
                    isValid = false; // 没有值也视为无效
                }


                // 检查需要工时列
                if (needWorkHoursCell != null && needWorkHoursCell.Value != null)
                {
                    int needWorkHours;
                    if (int.TryParse(needWorkHoursCell.Value.ToString(), out needWorkHours))
                    {
                        // 如果需要工时为0，则标记为无效
                        if (needWorkHours == 0)
                        {
                            isValid = false;
                        }
                    }
                    else
                    {
                        isValid = false; // 无法解析为整数也视为无效
                    }
                }
                else
                {
                    isValid = false; // 没有值也视为无效
                }

                // 检查机器组
                if (machineTeamCell != null && machineTeamCell.Value != null)
                {
                    if (string.IsNullOrEmpty(machineTeamCell.Value.ToString()))
                    {
                        isValid = false; // Column18值为空字符串时视为无效
                    }
                }
                else
                {
                    isValid = false; // 没有值也视为无效
                }

                // 根据验证结果设置背景色
                if (!isValid)
                {
                    UV.GetEntityByRow<Entity>(row).RowBackColor = Color.Red;
                    allValid = false; // 发现无效行
                }
                else
                {
                    UV.GetEntityByRow<Entity>(row).RowBackColor = Color.LightGreen;
                }
            }

            UV.RefreshDataGridViewBackColor(GetDataGridView1());

            return allValid;
        }

        private void 过程结果1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void 清空ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clear1();
        }

        private void 删除选中行ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteSelectedRows();
        }

        private void 恢复底稿ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReloadRecords();
        }

        private void 查询ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Query1();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            查询ToolStripMenuItem_Click(sender, e);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            合法性检查ToolStripMenuItem_Click(sender, e);
        }

        private void 合法性检查ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ValidateRecord();
        }

        private void 开始排产ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool allValid = ValidateRecord();

            // 如果有不是LightGreen的行，提示用户
            if (!allValid)
            {
                MessageBox.Show("有部分行异常，请检查！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // 提前结束方法执行
            }

            var scheduleUtility = this.GetUtility<IScheduleUtility>();
            var workdays = DaoTemplate.FindAll<Workday>();
            List<ScheduleRecord> list = scheduleUtility.Scheduling(workdays, selectedRecords, true);

            DaoTemplate.DeleteAll<ScheduleRecord>(user.FName);

            UV.SaveAsync(list, this, () => {
                var form = OpenForm<ScheduleResultsForm>("ScheduleResultsForm", "排产结果");
                form.LoadData();
            });
        }

        private void 导出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Export();
        }

        private void 保存到数据库ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void 重置ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Reset();
        }

        /// <summary>
        /// 删除选中行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            删除选中行ToolStripMenuItem_Click(sender, e);
        }

        private void 分屏ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowExtraGridView();
        }

        private void button6_Click(object sender, EventArgs e)
        {

            var todayWorkdays = workdays.Where(w => w.FWorkDate.Date == DateTime.Today).ToList();
            foreach (var todayWorkday in todayWorkdays)
            {
                var remainingWorkTimePercentage = todayWorkday.GetRemainingWorkTimePercentage();
                todayWorkday.FDuration = Convert.ToInt32(todayWorkday.FDuration * remainingWorkTimePercentage / 100);
                MessageBox.Show($"{todayWorkday.FDuration}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 双击dataGridView1时，创建并显示ScheduleFinalRecordEditForm，将选中行的ScheduleDetailsRecorscheduleFinalRecordd数据填充到对应控件
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            CreateEditForm<ProcessResultsEditForm2>(sender, e);

            //// 验证点击位置是否有效
            //if (e.RowIndex < 0 || e.ColumnIndex < 0)
            //{
            //    return;
            //}

            //DataGridView dataGridView = sender as DataGridView;
            //if (dataGridView == null)
            //{
            //    return;
            //}

            //// 获取选中的行
            //DataGridViewRow selectedRow = dataGridView.Rows[e.RowIndex];

            //// 获取该行的数据对象
            //var operationRecord = selectedRow.DataBoundItem as OperationRecord;

            //// 验证数据对象是否存在
            //if (operationRecord == null)
            //{
            //    MessageBox.Show("无法获取工序记录数据，请重试。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            //// 创建编辑窗体实例
            //using (var editForm = new ProcessResultsEditForm2())
            //{
            //    // 设置编辑窗体的数据源
            //    editForm.SetRecord(operationRecord);

            //    // 显示编辑窗体（模态对话框）
            //    DialogResult result = editForm.ShowDialog();

            //    // 根据用户操作决定是否更新数据源
            //    if (result == DialogResult.OK)
            //    {
            //        // 更新原始数据
            //        UpdateDataSourceWithEditedRecord(operationRecord);

            //        // 刷新DataGridView显示
            //        UpdateUI();

            //    }
            //}
        }

        private void 全选ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectAll();
        }

        private void 显示隐藏列ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColumnSettings();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // 首页按钮
            GoToFirstPage();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            GoToPreviousPage();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            GoToNextPage();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            GoToLastPage();
        }

        public override void UpdatePageInfo()
        {
            this.textBox2.Text = _currentPage.ToString();
            label7.Text = "/" + _totalPages;
            label6.Text = "总数:" + _totalCount;
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && this.textBox2.Text != "")
            {
                if (int.TryParse(textBox2.Text, out int pageNumber))
                {
                    GoToPage(pageNumber);
                }
            }
        }
    }
}
