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
    public partial class CustomizationSettingsForm : BaseForm
    {
        private User user = null;

        public CustomizationSettingsForm()
        {
            InitializeComponent();
        }

        private void CustomizationSettingsForm_Load(object sender, EventArgs e)
        {
            user = this.GetModel<ISessionModel>().GetUser();

            if (null == user.FBuffer)
                user.FBuffer = new Dictionary<string, object>();

            if (!user.FBuffer.ContainsKey(User.IsStartupOperationPlanStartDate))
                user.FBuffer[User.IsStartupOperationPlanStartDate] = false;

            if (!user.FBuffer.ContainsKey(User.FOperationPlanStartDate))
                user.FBuffer[User.FOperationPlanStartDate] = DateTime.Now.Date;

            
            this.checkBox1.Checked = (bool) user.FBuffer[User.IsStartupOperationPlanStartDate];
            this.dateTimePicker1.Value = (DateTime) user.FBuffer[User.FOperationPlanStartDate];
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            user.FBuffer[User.IsStartupOperationPlanStartDate] = this.checkBox1.Checked;
            DaoTemplate.Save(user);
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            user.FBuffer[User.FOperationPlanStartDate] = this.dateTimePicker1.Value.Date;
            DaoTemplate.Save(user);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBox1.Text, out int pageSize))
            {
                user.FBuffer[User.FPageSize] = pageSize;
                DaoTemplate.Save(user);
            }
        }
    }
}
