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
    public partial class ReturnPopup : GridViewForm3
    {
        public ReturnPopup()
        {
            InitializeComponent();
        }

        private void ReturnPopup_Load(object sender, EventArgs e)
        {
            InitializeData();

            //dateTimePicker1.Value = DateTime.Now;
            //dateTimePicker2.Value = DateTime.Now;
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
            return this.textBox1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Query1();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ReloadRecords();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ShowExtraGridView();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // 检查dataGridView1是否有选中行
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("请至少选择一行数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
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

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && this.textBox1.Text != "")
            {
                Query1();
            }
        }

        /// <summary>
        /// dataGridView1单元格双击事件处理
        /// 当用户双击单元格时，可以根据需要处理选中的数据
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // 检查行和列索引是否有效
            //if (e.RowIndex < 0 || e.ColumnIndex < 0)
            //    return;

            //this.DialogResult = DialogResult.OK;
            //this.Close();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            SelectAll();
        }
    }
}
