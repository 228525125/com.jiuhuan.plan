using QFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.jiuhuan.plan.models
{
    /// <summary>
    /// 管理已打开 Form 的接口，用于统一访问窗体集合
    /// </summary>
    public interface IFormModel : IModel
    {
        /// <summary>
        /// 添加一个 Form 到集合中
        /// </summary>
        void AddForm(Form form);

        /// <summary>
        /// 从集合中移除一个 Form
        /// </summary>
        void RemoveForm(Form form);

        /// <summary>
        /// 获取所有已打开的 Form 集合
        /// </summary>
        List<Form> GetOpenedForms();

        /// <summary>
        /// 获取指定类型的 Form
        /// </summary>
        T GetForm<T>() where T : Form;

        /// <summary>
        /// 获取主窗体
        /// </summary>
        Form1 GetMainForm();
    }

    /// <summary>
    /// 用于保存 tabControl1 中所有已打开的 Form 的 Model 类
    /// </summary>
    public class FormModel : AbstractModel,IFormModel
    {
        // 存储已打开的 Form 集合
        private List<Form> _openedForms = new List<Form>();

        /// <summary>
        /// 初始化方法，在模型初始化时调用
        /// </summary>
        protected override void OnInit()
        {
            // 可以在这里进行初始化操作
        }

        /// <summary>
        /// 添加一个 Form 到集合中
        /// </summary>
        /// <param name="form">要添加的 Form</param>
        public void AddForm(Form form)
        {
            if (!_openedForms.Contains(form))
            {
                _openedForms.Add(form);
            }
        }

        /// <summary>
        /// 从集合中移除一个 Form
        /// </summary>
        /// <param name="form">要移除的 Form</param>
        public void RemoveForm(Form form)
        {
            if (_openedForms.Contains(form))
            {
                _openedForms.Remove(form);
            }
        }

        /// <summary>
        /// 获取所有已打开的 Form 集合
        /// </summary>
        /// <returns>包含所有已打开 Form 的列表</returns>
        public List<Form> GetOpenedForms()
        {
            return new List<Form>(_openedForms);
        }

        /// <summary>
        /// 获取指定类型的 Form
        /// </summary>
        /// <typeparam name="T">要获取的 Form 类型</typeparam>
        /// <returns>指定类型的 Form，如果不存在则返回 null</returns>
        public T GetForm<T>() where T : Form
        {
            foreach (Form form in _openedForms)
            {
                if (form is T)
                {
                    return (T)form;
                }
            }

            // 如果没有找到，可以返回 null 或者根据需要创建一个新的实例
            return null;
        }

        /// <summary>
        /// 获取主窗体
        /// </summary>
        public Form1 GetMainForm()
        {
            return GetForm<Form1>();
        }
    }
}
