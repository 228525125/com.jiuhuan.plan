using com.jiuhuan.plan.commands;
using com.jiuhuan.plan.models;
using QFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.jiuhuan.plan
{
    /// <summary>
    /// 所有在 tabControl1 中打开的 Form 的基类，带框架
    /// </summary>
    public class BaseForm : Form, IController
    {
        private IFormModel _formModel;

        public BaseForm()
        {
            // 获取 TabControlModel 实例
            _formModel = this.GetModel<IFormModel>();

            // 将当前 Form 添加到 TabControlModel 中
            _formModel.AddForm(this);

            // 订阅 FormClosed 事件
            this.FormClosed += OnFormClosed;
        }

        public IArchitecture GetArchitecture()
        {
            return Framework.Interface;
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            // 从 TabControlModel 中移除当前 Form
            if (_formModel != null)
            {
                _formModel.RemoveForm(this);
            }

            // 取消事件订阅，防止内存泄漏
            this.FormClosed -= OnFormClosed;
        }

        protected T OpenForm<T>(string formName, string title) where T : BaseForm
        {
            var formModel = this.GetModel<IFormModel>();
            this.SendCommand(new OpenFormCommand(formName, title));
            return formModel.GetForm<T>();
        }
    }
}
