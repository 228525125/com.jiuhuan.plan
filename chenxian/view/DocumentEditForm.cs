using com.jiuhuan.plan.domain;
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
    public partial class DocumentEditForm : EditPopup<Document>
    {
        public DocumentEditForm()
        {
            InitializeComponent();
        }

        private void DatabaseEditForm_Load(object sender, EventArgs e)
        {
            var numberControl = FindControlByName<TextBox>(flowLayoutPanel1, "FNumber");
            numberControl.TextChanged += (s, args) =>
            {
                this.dataGridView1.DataSource = null;
                this.dataGridView1.Rows.Clear();
            };
        }

        private void TabPage_Layout(object sender, EventArgs e)
        {
            // 获取当前选中的 TabPage
            TabPage targetTabPage = (TabPage)sender;

            var manyToManyProperties = typeof(Document).GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(ManyToManyAttribute)))
                .ToList();

            foreach (var property in manyToManyProperties)
            {
                var manyToManyAttr = property.GetCustomAttribute<ManyToManyAttribute>();
                if (manyToManyAttr == null || string.IsNullOrEmpty(manyToManyAttr.Title))
                    continue;

                if (string.Equals(targetTabPage.Text, manyToManyAttr.Title, StringComparison.OrdinalIgnoreCase))
                {
                    // 从 TabPage 中查找 DataGridView 控件
                    DataGridView dataGridView = UV.FindDataGridViewInTabPage(targetTabPage);
                    if (dataGridView == null)
                        continue;

                    UV.InitializationDataGridView(manyToManyAttr.ChildType, dataGridView);
                }
            }
        }

        protected override FlowLayoutPanel GetFlowLayoutPanel()
        {
            return this.flowLayoutPanel1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // 更新scheduleDetailsRecord对象
                UpdateRecordFromControls();

                // 验证输入数据
                if (!ValidateInput())
                {
                    return;
                }

                // 关闭窗体并返回OK结果
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            try
            {
                Import_OpenPopup();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"操作失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            DeleteSelectedRows();
        }
    }
}
