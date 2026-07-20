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
    public partial class ProgressWindow : Form
    {
        public ProgressWindow()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        public void UpdateProgress(int percentComplete, string message)
        {
            progressBar1.Value = percentComplete;
            textBox1.Text = message;
        }

        public void UpdateProgress2(int percentComplete, string message)
        {
            progressBar1.Value = percentComplete;
            PrintMessage(message);
        }

        public void PrintMessage(string msg)
        {
            var text = "";
            text += msg;
            text += "\r\n";
            textBox1.AppendText(text);
            textBox1.ScrollToCaret();
        }
    }
}
