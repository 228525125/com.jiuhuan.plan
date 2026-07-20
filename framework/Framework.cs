using com.jiuhuan.plan.domain;
using com.jiuhuan.plan.framework.model;
using com.jiuhuan.plan.framework.system;
using com.jiuhuan.plan.models;
using com.jiuhuan.plan.tools;
using QFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan
{
    /// <summary>
    /// 定义一个架构（提供 MVC、分层、模块管理等）
    /// </summary>
    public class Framework : Architecture<Framework>
    {
        protected override void Init()
        {
            // 注册 System 
            RegisterSystem<IMenuTreeSystem>(new MenuTreeSystem());

            // 注册 Model
            RegisterModel<IFormModel>(new FormModel());
            RegisterModel<ISessionModel>(new SessionModel());

            var menuModel = new MenuModel();
            menuModel.AddEntity<Workday>();
            menuModel.AddEntity<OperationRecord>();
            menuModel.AddEntity<ScheduleRecord>();
            menuModel.AddEntity<ScheduleDetailsRecord>();
            menuModel.AddEntity<ScheduleFinalRecord>();
            menuModel.AddEntity<StampRecordSilianData>();
            menuModel.AddEntity<ValveAccessoryScanRecord>();
            menuModel.AddEntity<ValveAutoProcess>();
            menuModel.AddEntity<Bom>();

            menuModel.AddEntity("员工工时统计", "ReportForm");
            menuModel.AddEntity("合格证打印记录", "ReportForm");
            RegisterModel<IMenuModel>(menuModel);

            // 注册Utility
            RegisterUtility<ILogUtility>(new LogUtility());
            RegisterUtility<IScheduleUtility>(new ScheduleUtility()); 
        }

        protected override void ExecuteCommand(ICommand command)
        {
            //Debug.Log("Before " + command.GetType().Name + "Execute");
            base.ExecuteCommand(command);
            //Debug.Log("After " + command.GetType().Name + "Execute");
        }

        protected override TResult ExecuteCommand<TResult>(ICommand<TResult> command)
        {
            //Debug.Log("Before " + command.GetType().Name + "Execute");
            var result = base.ExecuteCommand(command);
            //Debug.Log("After " + command.GetType().Name + "Execute");
            return result;
        }
    }
}
