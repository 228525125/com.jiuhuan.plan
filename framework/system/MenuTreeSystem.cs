using com.jiuhuan.plan.commands;
using com.jiuhuan.plan.domain;
using com.jiuhuan.plan.events;
using com.jiuhuan.plan.framework.model;
using QFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.framework.system
{
    public interface IMenuTreeSystem : ISystem
    {
        /// <summary>
        /// 添加菜单
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="menu">菜单标题</param>
        void AddMenu<T>(string menu) where T : Entity;

        /// <summary>
        /// 触发点击菜单
        /// </summary>
        /// <param name="menu">菜单标题</param>
        void ClickMenu(string menu);
    }

    public class MenuTreeSystem : AbstractSystem, IMenuTreeSystem
    {
        public void AddMenu<T>(string menu) where T : Entity
        {
            
        }

        public void ClickMenu(string menu)
        {
            string formName = "";
            string menuTitle = menu;

            var entityModel = this.GetModel<IMenuModel>();
            formName = entityModel.GetFormName(menu);

            if (string.IsNullOrEmpty(formName))
                return;

            this.SendEvent(new ClickMenuEvent() { formName = formName, menuTitle = menuTitle });
        }

        protected override void OnInit()
        {
            // 初始化
        }
    }
}
