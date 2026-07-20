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
    public partial class LoginForm : BasePopup
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            // 清空之前的输入
            textBox1.Clear();
            textBox2.Clear();

            // 设置焦点到用户名输入框
            string account = ConfigHelper.GetConfigKey(ConfigHelper.buffer_current_account);
            if (string.IsNullOrEmpty(account))
            {
                textBox1.Focus();
            }
            else
            {
                textBox1.Text = ConfigHelper.GetConfigKey(ConfigHelper.buffer_current_account);
                textBox2.Focus();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string username = textBox1.Text.Trim();
                string password = textBox2.Text.Trim();

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("用户名和密码不能为空！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 验证用户信息
                var user = DaoTemplate.FindOne<User>(string.Format(Config.Default.sql_user_name_password, username, password));
                
                if (user != null)
                {
                    var session = this.GetModel<ISessionModel>();
                    session.SetUser(user);

                    ConfigHelper.SetConfigKey(ConfigHelper.buffer_current_account, user.FName);

                    DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("用户名或密码错误，请重新输入！", "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"登录过程中发生错误：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// 密码输入框回车事件处理
        /// </summary>
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && this.textBox1.Text != "" && this.textBox2.Text != "")
            {
                button1_Click(sender, e);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string username = textBox1.Text.Trim();
                string password = textBox2.Text.Trim();

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("用户名和密码不能为空！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 验证用户信息
                var user = DaoTemplate.FindOne<User>(string.Format(Config.Default.sql_user_name_password, username, password));

                if (user != null)
                {
                    var session = this.GetModel<ISessionModel>();
                    session.SetUser(user);
                   
                    // 打开修改密码窗口
                    ChangePasswordForm changePasswordForm = new ChangePasswordForm();
                    if (changePasswordForm.ShowDialog() == DialogResult.OK)
                    {
                        MessageBox.Show("密码修改成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("用户名或密码错误，请重新输入！", "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"登录过程中发生错误：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
