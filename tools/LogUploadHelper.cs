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
    public class LogUploadHelper : BaseTaskHelper
    {

        protected override int GetInterval()
        {
            int interval = 0;
            if (Logger.IsException())
                interval = 60000;
            else
                interval = Config.Default.log_upload_interval;

            return interval;
        }

        protected override void handle()
        {
            Logger.Upload();
        }
    }
}
