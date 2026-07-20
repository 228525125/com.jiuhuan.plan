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

namespace com.jiuhuan.plan
{
    public partial class DepartmentForm : GridViewForm<Department>
    {
        public DepartmentForm()
        {
            InitializeComponent();
        }

        private void DepartmentForm_Load(object sender, EventArgs e)
        {

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

            string queryText = textBox1.Text.Trim();

            // 过滤条件
            filteredRecords1 = selectedRecords.Where(bill =>
                bill.FDate.Date >= startDate &&
                bill.FDate.Date <= endDate &&
                (string.IsNullOrEmpty(queryText) ||
                bill.FNumber.Contains(queryText) ||
                bill.FName.Contains(queryText)) 
            ).ToList();
        }
    }
}
