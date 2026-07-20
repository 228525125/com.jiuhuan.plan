using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan
{
    /// <summary>
    /// 分页功能接口，提供分页数据管理和导航能力
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public interface IPagination<T>
    {
        /// <summary>
        /// 获取每页显示行数
        /// </summary>
        /// <returns>每页行数</returns>
        int GetPageSize();

        /// <summary>
        /// 获取当前页码
        /// </summary>
        /// <returns>当前页码</returns>
        int GetCurrentPage();

        /// <summary>
        /// 更新分页信息并显示第一页数据
        /// </summary>
        /// <param name="records">所有记录</param>
        void UpdatePagination(List<T> records);

        /// <summary>
        /// 显示当前页的数据
        /// </summary>
        void DisplayCurrentPage();

        /// <summary>
        /// 更新页码信息显示
        /// </summary>
        void UpdatePageInfo();

        /// <summary>
        /// 跳转到首页
        /// </summary>
        void GoToFirstPage();

        /// <summary>
        /// 跳转到上一页
        /// </summary>
        void GoToPreviousPage();

        /// <summary>
        /// 跳转到下一页
        /// </summary>
        void GoToNextPage();

        /// <summary>
        /// 跳转到末页
        /// </summary>
        void GoToLastPage();

        /// <summary>
        /// 跳转到指定页
        /// </summary>
        /// <param name="pageNumber">目标页码</param>
        void GoToPage(int pageNumber);
    }

    /// <summary>
    /// 分页功能接口（非泛型版本），适用于基于 Dictionary 的数据源
    /// </summary>
    public interface IPagination
    {
        /// <summary>
        /// 获取每页显示行数
        /// </summary>
        /// <returns>每页行数</returns>
        int GetPageSize();

        /// <summary>
        /// 获取当前页码
        /// </summary>
        /// <returns>当前页码</returns>
        int GetCurrentPage();

        /// <summary>
        /// 更新分页信息并显示第一页数据
        /// </summary>
        /// <param name="records">所有记录</param>
        void UpdatePagination(List<Dictionary<string, object>> records);

        /// <summary>
        /// 显示当前页的数据
        /// </summary>
        void DisplayCurrentPage();

        /// <summary>
        /// 更新页码信息显示
        /// </summary>
        void UpdatePageInfo();

        /// <summary>
        /// 跳转到首页
        /// </summary>
        void GoToFirstPage();

        /// <summary>
        /// 跳转到上一页
        /// </summary>
        void GoToPreviousPage();

        /// <summary>
        /// 跳转到下一页
        /// </summary>
        void GoToNextPage();

        /// <summary>
        /// 跳转到末页
        /// </summary>
        void GoToLastPage();

        /// <summary>
        /// 跳转到指定页
        /// </summary>
        /// <param name="pageNumber">目标页码</param>
        void GoToPage(int pageNumber);
    }
}
