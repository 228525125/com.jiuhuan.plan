using com.jiuhuan.plan.domain;
using com.jiuhuan.plan.models;
using com.jiuhuan.plan.tools;
using QFramework;
using System;
using System.Windows.Forms;

namespace com.jiuhuan.plan
{
    public partial class ChangePasswordForm : BasePopup
    {
        private User _user;

        public ChangePasswordForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string oldPassword = textBox1.Text.Trim();
                string newPassword = textBox2.Text.Trim();
                string confirmPassword = textBox3.Text.Trim();

                // 验证输入是否为空
                if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
                {
                    MessageBox.Show("所有密码字段不能为空！", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 验证新密码和确认密码是否一致
                if (newPassword != confirmPassword)
                {
                    MessageBox.Show("新密码和重复输入的密码不一致，请重新输入！", "密码不匹配", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox2.Clear();
                    textBox3.Clear();
                    textBox2.Focus();
                    return;
                }

                // 验证旧密码是否正确
                if (!_user.FPassword.Equals(oldPassword))
                {
                    MessageBox.Show("旧密码错误，请重新输入！", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox1.Clear();
                    textBox1.Focus();
                    return;
                }

                // 更新密码
                _user.FPassword = newPassword;
                var result = DaoTemplate.Save(_user) > 0;
                
                if (result)
                {
                    //MessageBox.Show("密码修改成功！", "操作成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("密码修改失败，请重试！", "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"修改密码过程中发生错误：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && this.textBox1.Text != "" && this.textBox2.Text != "" && this.textBox3.Text != "")
            {
                button1_Click(sender, e);
            }
        }

        private void ChangePasswordForm_Load(object sender, EventArgs e)
        {
            // 清空之前的输入
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();

            _user = this.GetModel<ISessionModel>().GetUser();
            textBox1.Text = _user.FPassword;
        }
    }
}
