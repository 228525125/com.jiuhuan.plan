using QFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using com.jiuhuan.plan.tools;

namespace com.jiuhuan.plan.domain
{
    [Entity(entirety: true, title: "员工")]
    public class User : Entity
    {
        public static string IsStartupOperationPlanStartDate = "IsStartupOperationPlanStartDate";
        public static string FOperationPlanStartDate = "FOperationPlanStartDate";
        public static string FPageSize = "FPageSize";

        [Keyword]
        [Column(width: 100)]
        [TextBox(width: 100, ui: true)]
        [Field("帐号")]
        public string FName { get; set; }

        [Column(width : 100)]
        [TextBox(width: 100, ui: true)]
        [Field("姓名")]
        public string FDescription { get; set; }

        public string FPassword { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, readOnly: false)]
        [Popup("请选择主要部门")]
        [Field("主要部门")]
        public string FDepartment { get; set; }

        [Ignore]
        [ManyToMany(typeof(Department), "Department_User", mappedBy: "FNumber", joinColumn: "FName", title: "部门列表")]
        [Popup("请选择部门", multipleRowSelection: true, isMultipleColumnReturn: true)]
        public List<Department> DepartmentList { get; set; } = new List<Department>();

        [Ignore]
        [ManyToMany(typeof(Role), "Role_User", mappedBy: "FNumber", joinColumn: "FName", title: "角色列表")]
        [Popup("请选择角色", multipleRowSelection: true, isMultipleColumnReturn: true)]
        public List<Role> RoleList { get; set; } = new List<Role>();

        [Serialization("序列化为JSON字符串，再保存到数据库，默认类型：Dictionary<string, object>")]
        public Dictionary<string, object> FBuffer { get; set; }

        /// <summary>
        /// 判断当前用户是否对指定单据的指定操作拥有权限。
        /// 计算逻辑：
        /// 1. 将 RoleList 按 FOperator 分为"包含"和"排除"两组；
        /// 2. 对每组角色，先通过 FCondition 判断角色是否有效（为空则默认有效，非空则作为SQL执行，返回true有效，false无效）；
        /// 3. 先收集所有有效"包含"角色的 PermissionList，得到初始权限集合；
        /// 4. 再收集所有有效"排除"角色的 PermissionList，从初始集合中移除；
        /// 5. 最终权限集合中，检查是否存在匹配指定 document 和 operation 的权限记录。
        /// </summary>
        /// <param name="documentType">单据类型</param>
        /// <param name="operation">操作名称，例如：新建、删除、修改、浏览等</param>
        /// <returns>拥有权限返回 true，否则返回 false</returns>
        public bool HasPermission(string documentType, string operation)
        {
            // 如果用户没有分配任何角色，则无权限
            if (RoleList == null || !RoleList.Any())
            {
                return false;
            }

            // 按 FOperator 分组：包含（添加权限）和排除（移除权限）
            var includeRoles = RoleList.Where(r => r.FOperator == "包含").ToList();
            var excludeRoles = RoleList.Where(r => r.FOperator == "排除").ToList();

            // 收集所有有效"包含"角色的权限，构建初始权限集合
            List<Permission> permissions = new List<Permission>();
            foreach (var role in includeRoles)
            {
                if (_isRoleEffective(role))
                {
                    if (role.PermissionList != null)
                    {
                        permissions.AddRange(role.PermissionList);
                    }
                }
            }

            // 收集所有有效"排除"角色的权限，从集合中逐一移除
            foreach (var role in excludeRoles)
            {
                if (_isRoleEffective(role))
                {
                    if (role.PermissionList != null)
                    {
                        foreach (var perm in role.PermissionList)
                        {
                            // 找到单据编号匹配的权限，将其操作字段按排除角色对应字段取反
                            var matched = permissions.FindAll(p => p.FDocumentNumber == perm.FDocumentNumber);
                            foreach (var p in matched)
                            {
                                if (perm.FIsCreate)  p.FIsCreate  = !p.FIsCreate;
                                if (perm.FIsDelete)  p.FIsDelete  = !p.FIsDelete;
                                if (perm.FIsModify)  p.FIsModify  = !p.FIsModify;
                                if (perm.FIsQuery)   p.FIsQuery   = !p.FIsQuery;
                                if (perm.FIsExport)  p.FIsExport  = !p.FIsExport;
                                if (perm.FIsPrint)   p.FIsPrint   = !p.FIsPrint;
                                if (perm.FIsPass)    p.FIsPass    = !p.FIsPass;
                                if (perm.FIsInvalid) p.FIsInvalid = !p.FIsInvalid;
                            }
                        }
                    }
                }
            }

            // 在最终权限集合中，检查是否存在匹配指定单据编号和操作的权限
            return permissions.Any(p => _matchesDocument(p, documentType) && _matchesOperation(p, operation));
        }

        /// <summary>
        /// 判断角色是否有效。
        /// 当 FCondition 为空时，默认有效；
        /// 当 FCondition 不为空时，将其作为SQL语句执行，执行结果有行返回则有效，否则无效。
        /// </summary>
        /// <param name="role">待判断的角色对象</param>
        /// <returns>角色有效返回 true，否则返回 false</returns>
        private bool _isRoleEffective(Role role)
        {
            // FCondition 为空，表示默认通过
            if (string.IsNullOrEmpty(role.FCondition))
            {
                return true;
            }

            try
            {
                // FCondition 是一段SQL语句，执行结果必须为bool类型返回
                // 使用 IsExist 判断SQL查询是否有返回行，作为bool结果
                string conditionSql = role.FCondition.Trim();

                // 如果不是以 SELECT 开头的完整查询语句，则包装为 SELECT 1 WHERE (...) 形式
                if (!conditionSql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                {
                    conditionSql = $"SELECT 1 WHERE ({conditionSql})";
                }

                return DaoTemplate.IsExist(conditionSql);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"角色 [{role.FNumber}] FCondition 执行异常: {ex.Message}");
                // 条件执行异常时，视为无效角色，不参与计算
                return false;
            }
        }

        /// <summary>
        /// 判断权限记录的单据编号是否与目标单据匹配。
        /// </summary>
        /// <param name="permission">权限记录</param>
        /// <param name="documentType">目标单据类型</param>
        /// <returns>匹配返回 true，否则返回 false</returns>
        private bool _matchesDocument(Permission permission, string documentType)
        {
            return permission.FDocumentType == documentType;
        }

        /// <summary>
        /// 根据操作名称，判断权限记录中对应的操作标志位是否为 true。
        /// 操作名称与 Permission 字段的映射关系：
        ///   新建 -> FIsCreate, 删除 -> FIsDelete, 修改 -> FIsModify,
        ///   浏览 -> FIsQuery, 导出 -> FIsExport, 打印 -> FIsPrint,
        ///   审核 -> FIsPass, 作废 -> FIsInvalid
        /// </summary>
        /// <param name="permission">权限记录</param>
        /// <param name="operation">操作名称</param>
        /// <returns>该操作被授权返回 true，否则返回 false</returns>
        private bool _matchesOperation(Permission permission, string operation)
        {
            switch (operation)
            {
                case "新建": return permission.FIsCreate;
                case "删除": return permission.FIsDelete;
                case "修改": return permission.FIsModify;
                case "浏览": return permission.FIsQuery;
                case "导出": return permission.FIsExport;
                case "打印": return permission.FIsPrint;
                case "审核": return permission.FIsPass;
                case "作废": return permission.FIsInvalid;
                default: return false;
            }
        }

        public object GetSettings(string key)
        {
            if (null == FBuffer || !FBuffer.Any())
                return null;

            if (!FBuffer.ContainsKey(key))
                return null;

            return FBuffer[key];
        }

        public void SetSettings(string key, object value)
        {
            if (null == FBuffer)
                FBuffer = new Dictionary<string, object>();

            FBuffer[key] = value;
        }
    }
}
