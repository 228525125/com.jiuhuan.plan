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

namespace com.jiuhuan.plan.view
{
    public partial class MoInventoryStatusDetailsDataGridViewForm : GridViewForm2<MoInventoryStatusDetails>
    {
        private string billNo = "";

        public MoInventoryStatusDetailsDataGridViewForm()
        {
            InitializeComponent();
        }

        private void DataGridViewForm_Load(object sender, EventArgs e)
        {
            InitializeData();

            string sql = string.Format(Config.Default.v_MoInventoryStatus_D_find_by_mo, billNo);
            items = DaoTemplate.FindAll<MoInventoryStatusDetails>(sql);
            SetDataSource(items);
        }

        protected override DataGridView GetDataGridView()
        {
            return this.dataGridView1;
        }

        public void SetBillNo(string billNo)
        {
            this.billNo = billNo;
        }
    }
}
