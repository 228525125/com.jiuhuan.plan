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
    public partial class ToScheduleFinalRecordForm : BasePopup
    {
        public ToScheduleFinalRecordForm()
        {
            InitializeComponent();
        }

        private void ToScheduleFinalRecordForm_Load(object sender, EventArgs e)
        {
            dateTimePicker1.Value = DateTime.Now.AddDays(1);
            dateTimePicker2.Value = DateTime.Now.AddDays(1);
            comboBox1.SelectedIndex = 0;
        }

        public DateTime GetBeginTime()
        {
            return dateTimePicker1.Value.Date;
        }

        public DateTime GetEndTime()
        {
            return dateTimePicker2.Value.Date;
        }

        public string GetClasses()
        {
            return comboBox1.SelectedItem?.ToString() ?? "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 关闭窗体并返回OK结果
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 关闭窗体并返回OK结果
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            dateTimePicker2.Value = dateTimePicker1.Value; 
        }
    }
}
