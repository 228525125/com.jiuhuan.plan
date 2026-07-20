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
    public partial class ValveAccessoryScanRecordForm : GridViewForm<ValveAccessoryScanRecord>
    {
        public ValveAccessoryScanRecordForm()
        {
            InitializeComponent();
        }

        private void DatabaseForm_Load(object sender, EventArgs e)
        {
            InitializeData();

            dateTimePicker1.Value = DateTime.Now;
            dateTimePicker2.Value = DateTime.Now.AddMonths(1);
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
            string queryText = textBox1.Text.Trim();

            // 过滤条件
            filteredRecords1 = selectedRecords.Where(bill =>
                bill.FDate >= startDate &&
                bill.FDate <= endDate &&
                (string.IsNullOrEmpty(queryText) ||
                bill.FBillNo.Contains(queryText) ||
                bill.FSerialNumber.Contains(queryText) ||
                bill.FBillNo.Contains(queryText))
            ).ToList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Query1();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DeleteSelectedRows();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PrintSeletedTemplate();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void 新增ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateRecord<ValveAccessoryScanRecordEditForm>();
        }

        private void 加载ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void 导入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import();
        }

        private void 查询ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Query1();
        }

        private void 恢复底稿ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReloadRecords();
        }

        private void 筛选过滤ToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void 配置列信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColumnSettings();
        }

        private void 全选ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectAll();
        }

        private void 导出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Export();
        }

        private void 选择打印模板ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectPrintTemplate();
        }

        private void 打印ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PrintSeletedTemplate();
        }

        private void 查询实体ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowEntityInformation();
        }

        private void 保存到数据库ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void 重置ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private void 初始化ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            CreateEditForm<ValveAccessoryScanRecordEditForm>(sender, e);
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && this.textBox1.Text != "")
            {
                Query1();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // 首页按钮
            GoToFirstPage();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // 上一页按钮
            GoToPreviousPage();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // 下一页按钮
            GoToNextPage();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            // 末页按钮
            GoToLastPage();
        }

        public override void UpdatePageInfo()
        {
            this.textBox2.Text = _currentPage.ToString();
            label3.Text = "/" + _totalPages;
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
