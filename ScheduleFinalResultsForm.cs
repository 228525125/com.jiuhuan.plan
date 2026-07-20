using com.jiuhuan.plan.domain;
using com.jiuhuan.plan.models;
using com.jiuhuan.plan.tools;
using Microsoft.VisualBasic;
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
    public partial class ScheduleFinalResultsForm : GridViewForm<ScheduleFinalRecord>
    {
        public ScheduleFinalResultsForm()
        {
            InitializeComponent();
        }

        private void ScheduleFinalResultsForm_Load(object sender, EventArgs e)
        {
            InitializeData();

            dateTimePicker1.Value = DateTime.Now;
            dateTimePicker2.Value = DateTime.Now;

            comboBox1.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
        }

        protected override string Sql()
        {
            return Config.Default.v_schedule_final_record_findall;
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
            string selectedMachineTeam = comboBox4.SelectedItem?.ToString() ?? "";
            string queryText = textBox1.Text.Trim();

            // 过滤条件
            filteredRecords1 = selectedRecords.Where(bill =>
                bill.FOperationPlanStartDate >= startDate &&
                bill.FOperationPlanStartDate <= endDate &&
                (string.IsNullOrEmpty(selectedStatus) || "全部".Equals(selectedStatus) || bill.FClasses.Equals(selectedStatus)) &&
                (string.IsNullOrEmpty(selectedMachineTeam) || "全部".Equals(selectedMachineTeam) || bill.FMachineTeam.Equals(selectedMachineTeam)) &&
                (string.IsNullOrEmpty(queryText) ||
                bill.FBillNo.Contains(queryText) ||
                bill.FNumber.Contains(queryText) ||
                bill.FName.Contains(queryText) ||
                bill.FModel.Contains(queryText))
            ).OrderBy(item => item.FStatus).ToList();
        }

        private void UpdateTotalRow()
        {
            // 计算filteredRecords1中FNeedWorkHours的总和并显示在label11上
            var records = (List<ScheduleFinalRecord>)this.dataGridView1.DataSource;
            int totalWorkHours = records.Sum(record => record.FNeedWorkHours);
            label11.Text = $"{totalWorkHours}";

            int capacity = GetCapacitySumByWorkdayAndShift();
            label12.Text = $"{capacity}";
        }

        /// <summary>
        /// 根据工作日期和班次查询workdays中对应日期的产能合计
        /// </summary>
        /// <returns>产能合计</returns>
        private int GetCapacitySumByWorkdayAndShift()
        {
            // 获取工作日期和班次
            DateTime workDate = dateTimePicker1.Value.Date;
            string shift = comboBox1.SelectedItem?.ToString() ?? "";

            // 过滤符合条件的记录
            var filteredWorkdays = workdays.Where(w =>
                w.FWorkDate.Date == workDate.Date &&
                (string.IsNullOrEmpty(shift) || "全部".Equals(shift) || w.FClasses.Equals(shift))
            ).ToList();

            // 计算产能合计
            // 产能 = 时长(秒) * 系数 * 人数 / 3600 (转换为小时)
            int capacitySum = 0;
            foreach (var workday in filteredWorkdays)
            {
                capacitySum += workday.FDuration;
            }

            return capacitySum;
        }

        protected override bool ValidateRecord()
        {
            return true;
        }

        private void 加载ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadData();
            UpdateTotalRow();
        }

        private void 查询ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Query1();
            UpdateTotalRow();
        }

        private void 恢复底稿ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReloadRecords();
        }

        private void 分屏ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowExtraGridView();
        }

        private void 清空ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clear1();
        }

        private void 删除选中行ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteSelectedRows();
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

        private void button1_Click(object sender, EventArgs e)
        {
            查询ToolStripMenuItem_Click(sender, e);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DeleteSelectedRows();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            设备日历ToolStripMenuItem_Click(sender, e);
        }

        /// <summary>
        /// 合法性检查
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 设备日历ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ValidateRecord();
        }

        /// <summary>
        /// 双击dataGridView1时，创建并显示ScheduleFinalRecordEditForm，将选中行的ScheduleDetailsRecorscheduleFinalRecordd数据填充到对应控件
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            CreateEditForm<ScheduleFinalRecordEditForm2>(sender, e);

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
            //var scheduleFinalRecord = selectedRow.DataBoundItem as ScheduleFinalRecord;

            //// 验证数据对象是否存在
            //if (scheduleFinalRecord == null)
            //{
            //    MessageBox.Show("无法获取工序记录数据，请重试。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            //// 创建编辑窗体实例
            //using (var editForm = new ScheduleFinalRecordEditForm2())
            //{
            //    // 设置编辑窗体的数据源
            //    editForm.SetRecord(scheduleFinalRecord);

            //    // 显示编辑窗体（模态对话框）
            //    DialogResult result = editForm.ShowDialog();

            //    // 根据用户操作决定是否更新数据源
            //    if (result == DialogResult.OK)
            //    {
            //        // 更新原始数据
            //        UpdateDataSourceWithEditedRecord(scheduleFinalRecord);

            //        // 刷新DataGridView显示
            //        UpdateUI();
            //    }
            //}
        }

        private void 从工单加载ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var content = Interaction.InputBox("请输入工单号", "扫描", "", -1, -1);
            if ("" == content)
                return;

            string sql = string.Format(Config.Default.v_schedule_final_findall_by_mo, $"'{content}'");
            var records = DaoTemplate.FindAll<ScheduleFinalRecord>(sql);
            if (!records.Any())
            {
                MessageBox.Show("没有找到合适的工序，请重新输入！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            UV.Signature(records, user);
            foreach (var record in records)
            {
                record.FOperationPlanStartDate = DateTime.Now.Date;
                record.FOperationPlanCompleteDate = DateTime.Now.Date;
            }

            selectedRecords.InsertRange(selectedRecords.Count, records);

            UV.SaveAsync(records, this, ()=> {
                加载ToolStripMenuItem_Click(sender, e);
                Query1();
            });
        }

        private async void 从Excel导入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.selectedRecords = await ExcelHelper.Import<ScheduleFinalRecord>();
            Query1();
        }

        private void 全选ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectAll();
        }

        private void 排序ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FixedSequence();
            this.button6.Visible = true;
            this.button7.Visible = true;
        }

        /// <summary>
        /// 上移
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            MoveUp();
        }

        /// <summary>
        /// 下移
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            MoveDown();
        }

        private void 取消排序ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CannelSequence();
            this.button6.Visible = false;
            this.button7.Visible = false;
        }

        private void 显示隐藏列ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColumnSettings();
        }

        private void 默认样式ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrintDefaultPrintTemplate();
        }

        private void 默认样式ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenDefaultPrintTemplate();
        }

        private void 查询实体ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowEntityInformation();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            GoToFirstPage();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            GoToPreviousPage();
        }

        private void button2_Click(object sender, EventArgs e)
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