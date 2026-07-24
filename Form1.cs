using com.jiuhuan.plan.commands;
using com.jiuhuan.plan.domain;
using com.jiuhuan.plan.events;
using com.jiuhuan.plan.framework.system;
using com.jiuhuan.plan.models;
using com.jiuhuan.plan.tools;
using QFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace com.jiuhuan.plan.view
{
    public partial class Form1 : Form, IController
    {
        

        public TabControl TabControl1 { get { return this.tabControl1; } }

        public Form1()
        {
            InitializeComponent();
            ConfigHelper.Init();
            int i = 0;
            string ss = "123";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;     //加载时 取消跨线程检查

            // 程序启动时，隐藏主窗口并弹出登录窗口
            this.Hide();
            ShowLoginForm();
        }

        /// <summary>
        /// 显示登录窗口
        /// </summary>
        private void ShowLoginForm()
        {
            try
            {
                using (var loginForm = new LoginForm())
                {
                    var result = loginForm.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        // 登录成功，显示主窗口
                        this.Show();
                        this.WindowState = FormWindowState.Maximized;

                        // 初始化其他组件
                        InitializeComponents();
                    }
                    else
                    {
                        // 登录取消，退出程序
                        Application.Exit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"登录过程中发生错误：{ex.Message}", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        /// <summary>
        /// 初始化主窗口组件
        /// </summary>
        private void InitializeComponents()
        {
            // 初始化日志
            Logger.Initialize(this.GetUtility<ILogUtility>());

            // 设置窗口最大化
            this.WindowState = FormWindowState.Maximized;

            this.GetModel<IFormModel>().AddForm(this);

            // 程序启动时，展开RootNode
            if (treeView1.Nodes.Count > 0)
            {
                treeView1.Nodes[0].Expand();
            }

            this.pictureBox1.Image = Image.FromFile(Config.Default.image_home);

            if (DaoTemplate.ConnectionTest())
            {
                this.label1.Text = "数据库连接成功！";
                this.label1.ForeColor = Color.Green;
            }
            else
            {
                this.label1.Text = "数据库连接失败，请检查网络！";
                this.label1.ForeColor = Color.Red;
            }

            this.RegisterEvent<FormClosedEvent>(evt => {
                Debug.WriteLine($"关闭窗口:{evt.formName}！！！");
            });

            this.RegisterEvent<ClickMenuEvent>(evt => {
                // 使用命令模式打开窗体
                this.SendCommand(new OpenFormCommand(evt.formName, evt.menuTitle));
            });
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.GetSystem<IMenuTreeSystem>().ClickMenu(e.Node.Text);
        }

        //双击TabPage关闭
        private void tabControl1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TabControl tabControl = sender as TabControl;

            for (int i = 0; i < tabControl.TabPages.Count; i++)
            {
                Rectangle tabRect = tabControl.GetTabRect(i);

                if (tabRect.Contains(e.Location))
                {
                    // 使用命令模式执行关闭操作
                    this.SendCommand(new CloseTabPageCommand(tabControl, i));
                    break;
                }
            }
        }

        //右键TabPage关闭
        private void tabControl1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TabControl tabControl = sender as TabControl;

                for (int i = 0; i < tabControl1.TabPages.Count; i++)
                {
                    Rectangle tabRect = tabControl1.GetTabRect(i);
                    if (tabRect.Contains(e.Location))
                    {
                        ContextMenuStrip menuStrip = new ContextMenuStrip();
                        ToolStripItem closeItem = menuStrip.Items.Add("关闭");
                        closeItem.Click += (s, ev) =>
                        {
                            // 使用命令模式执行关闭操作
                            this.SendCommand(new CloseTabPageCommand(tabControl, i));
                        };

                        menuStrip.Show(tabControl1, e.Location);
                        break;
                    }
                }
            }
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public IArchitecture GetArchitecture()
        {
            return Framework.Interface;
        }

        private void 版本信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"标准版号: 1.0.1", "版本信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void 程序安装目录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var appDirectory = Utils.GetAppDirectory();
            Utils.OpenFolder(appDirectory);
        }
    }
}
