using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.tools
{
    public class Response<T>
    {
        /// <summary>
        /// 错误信息
        /// </summary>
        public Error Error { get; set; }

        /// <summary>
        /// 是否有错误
        /// </summary>
        public bool HasError() {
            return null != Error;
        }

        /// <summary>
        /// 状态码
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// 响应数据
        /// </summary>
        public T RespData { get; set; }
    }
}
