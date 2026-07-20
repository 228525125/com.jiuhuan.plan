using com.jiuhuan.plan.events;
using QFramework;
using System.Windows.Forms;

namespace com.jiuhuan.plan.commands
{
    /// <summary>
    /// 处理用户双击 TabPage 时关闭该 TabPage 的命令类
    /// </summary>
    public class CloseTabPageCommand : AbstractCommand
    {
        private TabControl _tabControl; // TabControl 控件
        private int _tabPageIndex;     // 要关闭的 TabPage 索引

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tabControl">要操作的 TabControl</param>
        /// <param name="tabPageIndex">要关闭的 TabPage 索引</param>
        public CloseTabPageCommand(TabControl tabControl, int tabPageIndex)
        {
            _tabControl = tabControl;
            _tabPageIndex = tabPageIndex;
        }

        /// <summary>
        /// 执行命令：关闭指定索引的 TabPage，并释放对应的 Form 资源
        /// </summary>
        protected override void OnExecute()
        {
            if (_tabControl == null || _tabPageIndex < 0 || _tabPageIndex >= _tabControl.TabPages.Count)
            {
                return;
            }

            TabPage tabPage = _tabControl.TabPages[_tabPageIndex];

            // 防止关闭固定页面（如“主页”）
            if (tabPage.Text == "主页")
            {
                return;
            }

            // 移除 TabPage
            _tabControl.TabPages.RemoveAt(_tabPageIndex);

            // 遍历TabPage中的所有控件并关闭对应的Form
            foreach (Control control in tabPage.Controls)
            {
                if (control is Form form)
                {
                    form.Close();
                    break;
                }
            }

            this.SendEvent(new FormClosedEvent() { formName = tabPage.Text});
        }
    }
}