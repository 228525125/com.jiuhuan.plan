using com.jiuhuan.plan.commands;
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
    public partial class MoListForm : BaseForm
    {
        private List<Mo> selectedRecords = new List<Mo>();
        private List<Mo> filteredRecords = new List<Mo>();
        private User user = null;

        public MoListForm()
        {
            InitializeComponent();
        }

        private void MoListForm_Load(object sender, EventArgs e)
        {
            dateTimePicker1.Value = DateTime.Now.AddMonths(-1);
            dateTimePicker2.Value = DateTime.Now.AddMonths(3);

            UV.InitializationDataGridView2<Mo>(dataGridView1);

            user = this.GetModel<ISessionModel>().GetUser();
        }

        private void schedule(List<Mo> bills)
        {
            DaoTemplate.DeleteAll<OperationRecord>(user.FName);

            string condition = "";
            for (int i = 0; i < bills.Count; i++)
            {
                var bill = bills[i];
                condition += $"'{bill.FBillNo}'";
                if (i + 1 < bills.Count)
                    condition += ",";
            }
            string sql = string.Format(Config.Default.v_operation_findall_by_mo, condition);
            var operations = DaoTemplate.FindAll<OperationRecord>(sql);
            UV.Signature(operations, user);

            UV.SaveAsync(operations, this, () => {
                var form = OpenForm<ProcessResultsForm>("ProcessResultsForm", "工序维护");
                form.LoadData();
            });
        }

        private void FilterBills()
        {
            // 获取用户选择的参数
            DateTime startDate = dateTimePicker1.Value.Date;
            DateTime endDate = dateTimePicker2.Value.Date;
            string selectedStatus = comboBox1.SelectedItem?.ToString() ?? "";
            string queryText = textBox1.Text.Trim();

            // 过滤条件
            filteredRecords = selectedRecords.Where(bill =>
                bill.FDate.Date >= startDate &&
                bill.FDate.Date <= endDate &&
                (string.IsNullOrEmpty(selectedStatus) || "全部".Equals(selectedStatus) || bill.FState == selectedStatus) &&
                (string.IsNullOrEmpty(queryText) || 
                bill.FBillNo.Contains(queryText) ||
                bill.FNumber.Contains(queryText) ||
                bill.FName.Contains(queryText) || 
                bill.FModel.Contains(queryText))
            ).ToList();
        }

        /// <summary>
        /// 查询输入栏，回车
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && this.textBox1.Text != "")
            {
                查询ToolStripMenuItem_Click(sender, e);
            }
        }

        private void 加载ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 获取选择的开始和结束日期
            DateTime startDate = dateTimePicker1.Value.Date;
            DateTime endDate = dateTimePicker2.Value.Date;

            // 使用SQL语句Config.Default.sql_mo_findall进行查询
            string sql = string.Format(Config.Default.sql_mo_findall, $"{startDate:yyyy-MM-dd}", $"{endDate:yyyy-MM-dd}");
            this.selectedRecords = UV.LoadData<Mo>(sql);
            UV.Signature(this.selectedRecords, user);
            UV.SetDataSourceForGridView(dataGridView1, selectedRecords);
        }

        private async void 导入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.selectedRecords = await ExcelHelper.Import<Mo>();
            UV.SetDataSourceForGridView(dataGridView1, selectedRecords);
        }

        private void 查询ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FilterBills();

            // 设置数据源
            UV.SetDataSourceForGridView(dataGridView1, filteredRecords);
        }

        private void 恢复底稿ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UV.SetDataSourceForGridView(dataGridView1, selectedRecords);
        }

        //private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    if (filteredBills.Count == 0)
        //    {
        //        MessageBox.Show("没有可删除的数据，请先进行查询！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }

        //    var result = MessageBox.Show($"确定要删除筛选出的{filteredBills.Count}条数据吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        //    if (result == DialogResult.Yes)
        //    {
        //        // 从selectedBills中移除filteredBills中的数据
        //        foreach (var mo in filteredBills.ToList())
        //        {
        //            this.selectedBills.Remove(mo);
        //        }

        //        // 更新DataGridView显示
        //        UV.SetDataSourceForGridView(dataGridView1, this.selectedBills);
        //        MessageBox.Show("删除成功！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    }
        //}

        private void 清空ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UV.ClearDataSourceForGridView(dataGridView1, selectedRecords, filteredRecords);
        }

        private void 导出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var bills = (List<Mo>)this.dataGridView1.DataSource;
            ExcelHelper.Export(bills);
        }

        private void 开始排产ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var bills = (List<Mo>)this.dataGridView1.DataSource;
            schedule(bills);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            //查询ToolStripMenuItem_Click(sender, e);
            //DateTime targetDate = new DateTime(2023, 10, 1, 0, 0, 0);
            //Queue<WorkdayFragment> queue = new Queue<WorkdayFragment>(new List<WorkdayFragment>
            //{
            //    new WorkdayFragment { FStartTime = new DateTime(2023, 10, 1, 9, 0, 0), FEndTime = new DateTime(2023, 10, 1, 12, 0, 0) },
            //    new WorkdayFragment { FStartTime = new DateTime(2023, 10, 1, 13, 0, 0), FEndTime = new DateTime(2023, 10, 1, 15, 0, 0) },
            //    new WorkdayFragment { FStartTime = new DateTime(2023, 10, 1, 16, 0, 0), FEndTime = new DateTime(2023, 10, 1, 18, 0, 0) }
            //});

            //List<WorkdayFragment> foundFragments = Utils.FindWorkdayFragmentsUpTo(targetDate, queue);

            //foreach (var fragment in foundFragments)
            //{
            //    Console.WriteLine($"找到时间段：{fragment.FStartTime} - {fragment.FEndTime}");
            //}

            查询ToolStripMenuItem_Click(sender, e);
        }

        private void 删除选中行ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var deleteList = UV.DeleteSelectedRows<Mo>(dataGridView1);

            if (!deleteList.Any())
                return;

            selectedRecords.RemoveAll(bill => deleteList.Contains(bill));

            UV.DeleteAsync(deleteList, this);
        }

        private void 保存到数据库ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedRecords.Count == 0)
            {
                MessageBox.Show("没有需要保存的数据，请检查！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DaoTemplate.DeleteAll<Mo>(user.FName);
            UV.SaveAsync(selectedRecords, this);
        }

        private void 重置ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show($"确定要删除已保存的所有日历数据吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                DaoTemplate.DeleteAll<Mo>(user.FName);
                UV.ClearDataSourceForGridView(dataGridView1, selectedRecords, filteredRecords);
                MessageBox.Show("删除成功！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
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
