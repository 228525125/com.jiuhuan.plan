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
using QFramework;

namespace com.jiuhuan.plan
{
    public partial class LogSettingForm : BaseForm
    {
        public LogSettingForm()
        {
            InitializeComponent();

            this.checkBox9.Checked = bool.Parse(ConfigHelper.GetConfigKey(ConfigHelper.log_upload_cloud));
            this.textBox1.Text = ConfigHelper.GetConfigKey(ConfigHelper.log_server_ip);
            this.textBox1.Enabled = this.checkBox9.Checked;
        }

        private void LogSettingForm_Load(object sender, EventArgs e)
        {
            
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            ConfigHelper.SetConfigKey(ConfigHelper.log_upload_cloud, checkBox9.Checked.ToString());
            textBox1.Enabled = checkBox9.Checked;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            var log = this.GetUtility<ILogUtility>();
            log.SetServerIp(textBox1.Text);
            ConfigHelper.SetConfigKey(ConfigHelper.log_server_ip, textBox1.Text);
        }
    }
}
