using com.jiuhuan.plan.models;
using com.jiuhuan.plan.tools;
using QFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.jiuhuan.plan.commands
{
    /// <summary>
    /// 用于处理用户点击treeView1节点时，创建对应的BaseForm并显示在tabControl1中的命令类
    /// </summary>
    public class OpenFormCommand : AbstractCommand
    {
        private string _formName; // 要打开的窗体名称
        private string _tabPageText; // TabPage 的标题文本
        private Form _form; // 要显示的 BaseForm 实例

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="formName">要创建的窗体类型名称</param>
        /// <param name="tabPageText">TabPage 的标题文本</param>
        public OpenFormCommand(string formName, string tabPageText)
        {
            _formName = formName;
            _tabPageText = tabPageText;
        }

        /// <summary>
        /// 执行命令：创建指定类型的窗体，并将其添加到 tabControl1 中
        /// </summary>
        protected override void OnExecute()
        {
            // 获取主窗体实例
            var mainForm = this.GetModel<IFormModel>().GetMainForm();

            if (mainForm == null)
            {
                MessageBox.Show("主窗体未找到！");
                return;
            }

            // 检查是否已经存在该TabPage
            foreach (TabPage tp in mainForm.TabControl1.TabPages)
            {
                if (tp.Text == _tabPageText)
                {
                    mainForm.TabControl1.SelectedTab = tp;
                    return;
                }
            }

            // 根据类名动态创建窗体实例
            try
            {
                // 获取当前程序集
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                
                // 构建完整的类型名称，假设窗体类在 com.jiuhuan.plan.views 命名空间下
                // 注意：请根据实际项目中窗体所在的命名空间调整下面的命名空间字符串
                string namespaceName = "com.jiuhuan.plan";
                string fullTypeName = $"{namespaceName}.{_formName}";
                
                // 获取类型
                Type formType = assembly.GetType(fullTypeName);
                
                if (formType == null)
                {
                    MessageBox.Show($"未找到类型为 {_formName} 的窗体类，请检查命名空间和类名是否正确。");
                    return;
                }

                // 检查该类型是否继承自 Form
                if (!typeof(Form).IsAssignableFrom(formType))
                {
                    MessageBox.Show($"类型 {_formName} 不是有效的 Form 类。");
                    return;
                }

                // 创建实例
                _form = Activator.CreateInstance(formType) as Form;

                if (_form == null)
                {
                    MessageBox.Show($"无法创建 {_formName} 的实例。");
                    return;
                }

                if ("ReportForm".Equals(_formName))
                {
                    MethodInfo setTitle = formType.GetMethod("SetTitle");
                    if (setTitle != null)
                    {
                        // 调用SetTitle方法设置数据
                        setTitle.Invoke(_form, new object[] { _tabPageText });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"创建窗体 {_formName} 时发生错误: {ex.Message}");
                return;
            }

            // 初始化窗体属性
            _form.TopLevel = false;
            _form.FormBorderStyle = FormBorderStyle.None;
            _form.Dock = DockStyle.Fill;

            // 创建TabPage
            var tabPage = new TabPage(_tabPageText);
            tabPage.ToolTipText = "双击关闭";

            // 添加窗体到TabPage
            tabPage.Controls.Add(_form);
            _form.Show();

            // 将TabPage添加到TabControl
            mainForm.TabControl1.TabPages.Add(tabPage);

            // 切换到新打开的TabPage
            mainForm.TabControl1.SelectedTab = tabPage;
        }
    }
}
