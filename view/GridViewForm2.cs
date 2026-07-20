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
    /// <summary>
    /// 弹窗显示数据，不带任何功能
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GridViewForm2<T> : Form where T : new()
    {
        protected List<T> items = new List<T>();

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // GridViewForm2
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "GridViewForm2";
            this.Load += new System.EventHandler(this.GridViewForm2_Load);
            this.ResumeLayout(false);

        }

        private void GridViewForm2_Load(object sender, EventArgs e)
        {

        }

        protected void InitializeData()
        {
            UV.InitializeColumnsWithAttributes<T>(GetDataGridView());
        }

        protected virtual DataGridView GetDataGridView()
        {
            throw new NotImplementedException();
        }

        public void SetDataSource(List<T> list)
        {
            items = list;
            UV.SetDataSourceForGridView(GetDataGridView(), list);
        }

        public List<T> GetItems()
        {
            return this.items;
        }
    }
}
