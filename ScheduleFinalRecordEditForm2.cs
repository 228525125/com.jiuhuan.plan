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
    public partial class ScheduleFinalRecordEditForm2 : EditPopup<ScheduleFinalRecord>
    {
        public ScheduleFinalRecordEditForm2()
        {
            InitializeComponent();
        }

        private void ScheduleFinalRecordEditForm2_Load(object sender, EventArgs e)
        {

        }

        protected override FlowLayoutPanel GetFlowLayoutPanel()
        {
            return this.flowLayoutPanel1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // 验证输入数据
                if (!ValidateInput())
                {
                    return;
                }

                // 更新scheduleDetailsRecord对象
                UpdateRecordFromControls();

                // 操作可能修改了待加工数，例如早班改中班，已部分加工
                this.record.FNeedWorkHours = (int)(this.record.FAidedWorkHours + this.record.FRemainQty * this.record.FStandardWorkHours);

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

        /// <summary>
        /// 验证输入数据的有效性
        /// </summary>
        /// <returns>验证是否通过</returns>
        private bool ValidateInput()
        {
            return true;
        }
    }
}
