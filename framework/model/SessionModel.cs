using com.jiuhuan.plan.domain;
using QFramework;

namespace com.jiuhuan.plan.models
{
    /// <summary>
    /// 用户登录信息
    /// </summary>
    public interface ISessionModel : IModel
    {
        /// <summary>
        /// 设置User到Session中
        /// </summary>
        void SetUser(User user);

        /// <summary>
        /// 获取User
        /// </summary>
        User GetUser();
    }

    /// <summary>
    /// 
    /// </summary>
    public class SessionModel : AbstractModel,ISessionModel
    {
        private User _user = null;

        public User GetUser()
        {
            return _user;
        }

        public void SetUser(User user)
        {
            _user = user;
        }

        /// <summary>
        /// 初始化方法，在模型初始化时调用
        /// </summary>
        protected override void OnInit()
        {
            // 可以在这里进行初始化操作
        }

        
    }
}
