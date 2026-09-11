using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.view
{
    /// <summary>
    /// 权限检查接口，通过 StackTrace 查找调用方法上的 PermissionAttribute 特性，
    /// 使用 User.hasPermission 方法判断当前用户是否拥有指定操作权限。
    /// </summary>
    public interface IPermission
    {
    }
}
