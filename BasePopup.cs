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
    /// 所有在 tabControl1 中打开的 Form 的基类
    /// </summary>
    public class BasePopup : Form, IController
    {

        public BasePopup()
        {
            
        }

        public IArchitecture GetArchitecture()
        {
            return Framework.Interface;
        }
    }
}
