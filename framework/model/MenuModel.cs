using com.jiuhuan.plan.domain;
using com.jiuhuan.plan.tools;
using QFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.framework.model
{
    public interface IMenuModel : IModel
    {
        /// <summary>
        /// 添加一个 Entity 到集合中
        /// </summary>
        void AddEntity<T>() where T : Entity;

        /// <summary>
        /// 添加一个 Entity 到集合中，用于无Entity的数据表格，例如报表
        /// </summary>
        /// <param name="title"></param>
        /// <param name="formName"></param>
        void AddEntity(string title, string formName);

        /// <summary>
        /// 从集合中移除一个 Entity
        /// </summary>
        void RemoveEntity<T>() where T : Entity;

        /// <summary>
        /// 根据title获取formName
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        string GetFormName(string title);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        string GetEditFormName(string title);
    }

    public class MenuModel : AbstractModel, IMenuModel
    {
        private Dictionary<string, string> _titleToForm = new Dictionary<string, string>();

        private Dictionary<string, string> _titleToEditForm = new Dictionary<string, string>();

        /// <summary>
        /// 初始化方法，在模型初始化时调用
        /// </summary>
        protected override void OnInit()
        {
            _titleToForm["工单维护"] = "MoListForm";
            _titleToForm["日志"] = "LogSettingForm";
            _titleToForm["个人偏好"] = "CustomizationSettingsForm";
        }

        /// <summary>
        /// 添加一个 Entity 到集合中
        /// </summary>
        /// <param name="entity">要添加的 Entity</param>
        public void AddEntity<T>() where T : Entity
        {
            var title = Utility.GetAttributeValueByClass<T>("Entity", "Title") as string;
            if (string.IsNullOrEmpty(title))
                return;

            if (!_titleToForm.ContainsKey(title.ToString()))
            {
                var formName = Utility.GetAttributeValueByClass<T>("Entity", "FormName") as string;
                if (string.IsNullOrEmpty(formName))
                {
                    var typeName = typeof(T).Name;
                    _titleToForm[title.ToString()] = typeName + "Form";
                }
                else
                {
                    _titleToForm[title.ToString()] = formName;
                }
            }

            if (!_titleToEditForm.ContainsKey(title.ToString()))
            {
                var editFormName = Utility.GetAttributeValueByClass<T>("Entity", "EditFormName") as string;
                if (string.IsNullOrEmpty(editFormName))
                {
                    var typeName = typeof(T).Name;
                    _titleToEditForm[title.ToString()] = typeName + "EditForm";
                }
                else
                {
                    _titleToEditForm[title.ToString()] = editFormName;
                }
            }
        }

        public void AddEntity(string title, string formName)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(formName))
                return;

            _titleToForm[title] = formName;
        }

        /// <summary>
        /// 从集合中移除一个 Entity
        /// </summary>
        /// <param name="entity">要移除的 Entity</param>
        public void RemoveEntity<T>() where T : Entity
        {
            var title = Utility.GetAttributeValueByClass<T>("Entity", "Title") as string;
            if (string.IsNullOrEmpty(title))
                return;

            _titleToForm.Remove(title);
            _titleToEditForm.Remove(title);
        }

        public string GetFormName(string title)
        {
            if (_titleToForm.ContainsKey(title))
                return _titleToForm[title];
            else
                return null;
        }

        public string GetEditFormName(string title)
        {
            if (_titleToEditForm.ContainsKey(title))
                return _titleToEditForm[title];
            else
                return null;
        }
    }
}
