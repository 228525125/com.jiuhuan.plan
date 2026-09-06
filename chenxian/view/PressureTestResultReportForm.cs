using com.jiuhuan.plan.domain;
using com.jiuhuan.plan.tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.jiuhuan.plan.view
{
    public partial class PressureTestResultReportForm : GridViewForm4<PressureTestResult>
    {

        public PressureTestResultReportForm()
        {
            InitializeComponent();
        }

        private void DatabaseForm_Load(object sender, EventArgs e)
        {
            InitializeData();

            dateTimePicker1.Value = DateTime.Now;
            dateTimePicker2.Value = DateTime.Now.AddMonths(-1);
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

        protected override TextBox GetTextBox1()
        {
            return textBox1; ;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Query1();
        }

        private void 加载ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadData(() => {

                UV.InitializationDataGridView<PressureTestResult>(GetDataGridView1(), this);
                UV.InitializationDataGridView<PressureTestResult>(GetDataGridView2());
                // 设置筛选DataGridView
                UV.SetupFilterDataGridView<PressureTestResult>(GetDataGridView2());

                // 从User.FBuffer获取配置并应用到DataGridView           
                string configKey = _title;
                var valueConfig = user.GetSettings(configKey);
                if (valueConfig != null)
                {
                    Dictionary<string, object> savedConfig = null;
                    if (valueConfig is Dictionary<string, object> vc)
                        savedConfig = vc;
                    else
                        savedConfig = JsonHelper.toObject<Dictionary<string, object>>(valueConfig.ToString());
                    UV.ApplyColumnConfiguration(GetDataGridView1(), savedConfig);
                }
            });
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
            SelectPrintTemplate(_title);
        }

        private void 打印ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PrintSeletedTemplate(_title);
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView_CellDoubleClick(sender, e);
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
