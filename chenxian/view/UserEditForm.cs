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
using System.Windows.Controls.Primitives;
using System.Windows.Forms;

namespace com.jiuhuan.plan.view
{
    public partial class UserEditForm : EditPopup<User>
    {
        public UserEditForm()
        {
            InitializeComponent();
        }

        private void DatabaseEditForm_Load(object sender, EventArgs e)
        {
            var numberControl = FindControlByName<TextBox>(flowLayoutPanel1, "FName");
            numberControl.TextChanged += (s, args) =>
            {
                this.dataGridView1.DataSource = null;
                this.dataGridView1.Rows.Clear();
            };
        }

        protected override FlowLayoutPanel GetFlowLayoutPanel()
        {
            return this.flowLayoutPanel1;
        }

        /// <summary>
        /// TabControl选项卡切换后触发，用于在切换到对应TabPage时动态加载关联数据
        /// </summary>
        private void tabControl_Selected(object sender, TabControlEventArgs e)
        {
            
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

        private void toolStripButton4_Click(object sender, EventArgs e)
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

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            DeleteSelectedRows();
        }
    }
}
