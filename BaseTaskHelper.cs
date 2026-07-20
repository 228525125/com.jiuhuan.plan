using com.jiuhuan.plan.tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.jiuhuan.plan
{
    public abstract class BaseTaskHelper
    {
        protected Task task = null;
        protected CancellationTokenSource tokenSource = null;

        protected DateTime dateTime = DateTime.Now;

        /// <summary>
        /// 循环执行间隔时间
        /// </summary>
        /// <returns></returns>
        protected virtual int GetInterval()
        {
            return Config.Default.task_helper_interval_default;
        }

        /// <summary>
        /// 业务方法
        /// </summary>
        protected abstract void handle();

        /// <summary>
        /// 打印消息
        /// </summary>
        /// <param name="msg"></param>
        protected virtual void printMessage(string msg)
        {

        }

        public Task GetTask()
        {
            return task;
        }

        public bool IsWorking()
        {
            if (null == task)
                return false;

            if (task.IsCanceled || task.IsCompleted || task.IsFaulted)
                return false;
            else
                return true;
        }

        public virtual void start()
        {
            tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            task = new Task(() =>
            {
                while (true)
                {
                    if (token.IsCancellationRequested) return;
                    handle();
                    Thread.Sleep(GetInterval());
                }
            });
            task.Start();
        }

        public void stop()
        {
            if (null != task && null != tokenSource)
                tokenSource.Cancel();
        }

        public void handleException(string msg, Action action)
        {
            int num = 0;
            while (null != DaoTemplate.SqlException && num < 5)    //发生异常
            {
                num++;
                msg += (null != DaoTemplate.SqlException ? DaoTemplate.SqlException.ToString() + "\r\n ErorrSql:" + DaoTemplate.SqlString : "") + " " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n";
                msg += "--------------------------------";
                printMessage(msg);
                msg = $"5秒后重新尝试连接数据库...第{num}次";
                printMessage(msg);
                for (int i = 5; i > 0; i--)
                {
                    msg = i.ToString();
                    printMessage(msg);
                    Thread.Sleep(1000);
                }
                action.Invoke();
            }

            if(num >= 5)
            {
                stop();
            }
        }
    }
}
