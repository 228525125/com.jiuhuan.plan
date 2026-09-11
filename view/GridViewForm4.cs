using com.jiuhuan.plan.domain;
using com.jiuhuan.plan.models;
using com.jiuhuan.plan.tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Label = System.Windows.Forms.Label;

namespace com.jiuhuan.plan.view
{
    /// <summary>
    /// 报表，带查询、分页等
    /// </summary>
    public class GridViewForm4<T> : Form, IPagination<T> where T : Entity, new()
    {
        protected List<T> selectedRecords = new List<T>();
        protected List<T> filteredRecords1 = new List<T>();
        protected User user = null;
        protected string _title = "";

        // 分页相关变量
        protected int _pageSize = 200;          // 每页显示行数
        protected int _currentPage = 1;         // 当前页码
        protected int _totalPages = 0;          // 总页数
        protected int _totalCount = 0;          // 总记录数
        private List<T> _allRecords = new List<T>(); // 所有记录

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(282, 253);
            this.Name = "GridViewForm4";
            this.Load += new EventHandler(this.GridViewForm4_Load);
            this.ResumeLayout(false);
        }

        private void GridViewForm4_Load(object sender, EventArgs e)
        {

        }

        protected void InitializeData()
        {
            GetSplitContainer().Panel2Collapsed = true;

            

            // 监听dataGridView2的单元格值变化事件
            GetDataGridView2().CellValueChanged += (sender, e) =>
            {
                // 结束编辑模式，确保值被提交
                GetDataGridView2().EndEdit();
                filteredRecords1 = UV.ApplyFilter<T>(GetDataGridView1(), GetDataGridView2(), selectedRecords);
                string sqlFromFile = File.ReadAllText(GetSqlFilePath(), Encoding.Default);
                CalculateSumFromSql(sqlFromFile);
                UpdatePagination(filteredRecords1);
            };

            // 监听dataGridView2的按键事件
            GetDataGridView2().KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    filteredRecords1 = UV.ApplyFilter(GetDataGridView1(), GetDataGridView2(), selectedRecords);
                    string sqlFromFile = File.ReadAllText(GetSqlFilePath(), Encoding.Default);
                    CalculateSumFromSql(sqlFromFile);
                    UpdatePagination(filteredRecords1);
                }
            };

            // 监听主DataGridView列宽变化事件，同步查询行列宽
            GetDataGridView1().ColumnWidthChanged += (sender, e) =>
            {
                if (e.Column != null && !string.IsNullOrEmpty(e.Column.Name))
                {
                    var queryColumn = GetDataGridView2().Columns[e.Column.Name];
                    if (queryColumn != null)
                    {
                        queryColumn.Width = e.Column.Width;
                    }
                }
            };

            // 监听主DataGridView列显示/隐藏变化事件，同步查询行列可见性
            GetDataGridView1().ColumnDisplayIndexChanged += (sender, e) =>
            {
                if (e.Column != null && !string.IsNullOrEmpty(e.Column.Name))
                {
                    var queryColumn = GetDataGridView2().Columns[e.Column.Name];
                    if (queryColumn != null)
                    {
                        queryColumn.DisplayIndex = e.Column.DisplayIndex;
                        queryColumn.Visible = e.Column.Visible;
                    }
                }
            };

            user = Framework.Interface.GetModel<ISessionModel>().GetUser();
            var num = user.GetSettings(User.FPageSize);
            _pageSize = null != num ? int.Parse(num.ToString()) : 200;

            LoadData2(() => {

                UV.InitializationDataGridView<T>(GetDataGridView1(), this);
                UV.InitializationDataGridView<T>(GetDataGridView2());
                // 设置筛选DataGridView
                UV.SetupFilterDataGridView<T>(GetDataGridView2());

                // 从User.FBuffer获取配置并应用到DataGridView           
                string configKey = _title;
                var valueConfig = user.GetSettings(configKey);
                if (valueConfig != null)
                {
                    Dictionary<string, object> savedConfig = null;
                    if (valueConfig is Dictionary<string, object> vc)
                        savedConfig = vc;
                    else
                        savedConfig = JsonHelper.toObject<Dictionary<string, object>>(valueConfig.ToString());
                    UV.ApplyColumnConfiguration(GetDataGridView1(), savedConfig);
                }
            });
        }

        protected virtual SplitContainer GetSplitContainer()
        {
            throw new NotImplementedException();
        }

        protected virtual DataGridView GetDataGridView1()
        {
            throw new NotImplementedException();
        }
        protected virtual DataGridView GetDataGridView2()
        {
            throw new NotImplementedException();
        }

        protected virtual TextBox GetTextBox1()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据T类型的所有字符串属性进行模糊查询过滤
        /// 遍历T类型的所有公共实例属性，对字符串类型的属性值进行Contains匹配
        /// 查询文本从GetTextBox1()获取，过滤结果保存到filteredRecords1中
        /// </summary>
        protected virtual void FilterBills1()
        {
            string queryText = GetTextBox1().Text.Trim();

            // 获取T类型的所有字符串类型属性
            var stringProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(string))
                .ToArray();

            // 过滤条件
            filteredRecords1 = selectedRecords.Where(bill =>
            {
                // 如果没有查询文本，则返回所有记录
                if (string.IsNullOrEmpty(queryText))
                    return true;

                // 遍历所有字符串属性，只要任意一个属性值包含查询文本即匹配
                foreach (var prop in stringProperties)
                {
                    string value = prop.GetValue(bill) as string;
                    if (!string.IsNullOrEmpty(value) && value.Contains(queryText))
                        return true;
                }

                return false;
            }).ToList();
        }

        public void SetDataSource(List<T> list)
        {
            selectedRecords = list;
        }

        public void SetTitle(string title)
        {
            this._title = title;
        }

        private string GetSqlFilePath()
        {
            string sqlFileName = $"Report_{_title}.sql";

            // 获取当前应用程序目录
            string appDirectory = Utils.GetAppDirectory();

            // 构建SQL文件的完整路径：当前目录/sql/文件名
            string sqlFilePath = Path.Combine(appDirectory, "sql", sqlFileName);

            return sqlFilePath;
        }

        /// <summary>
        /// 从SQL内容中解析汇总字段并计算合计，显示到label7
        /// 如果filteredRecords1不为空则合计filteredRecords1，否则合计selectedRecords
        /// </summary>
        /// <param name="sqlContent">包含sum标签定义的SQL内容</param>
        private void CalculateSumFromSql(string sqlContent)
        {
            // 从 SQL 文件中解析 <sum> 标签，表示需要汇总显示的字段
            var sum = Utility.ParseXmlTag(sqlContent, "sum");

            if (string.IsNullOrEmpty(sum))
                return;

            // 确定合计的数据源：filteredRecords1不为空时使用filteredRecords1，否则使用selectedRecords
            List<T> sumSource = (filteredRecords1 != null && filteredRecords1.Count > 0)
                ? filteredRecords1
                : selectedRecords;

            // 解析汇总字段名，多个字段用分号隔开
            string[] sumFields = sum.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            // 对每个汇总字段计算总和
            List<string> sumResults = new List<string>();
            foreach (string fieldName in sumFields)
            {
                string trimmedField = fieldName.Trim();
                if (string.IsNullOrEmpty(trimmedField))
                    continue;

                decimal total = 0;
                // 通过反射按属性名获取实体属性值，在循环外获取PropertyInfo避免重复查找
                var prop = typeof(T).GetProperty(trimmedField, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                {
                    foreach (var record in sumSource)
                    {
                        object rawValue = prop.GetValue(record);
                        if (rawValue != null)
                        {
                            if (decimal.TryParse(rawValue.ToString(), out decimal value))
                            {
                                total += value;
                            }
                        }
                    }
                }

                sumResults.Add($"{trimmedField}：{total}");
            }

            // 将汇总结果显示到 label7
            Label sumLabel = this.Controls.Find("label7", true).FirstOrDefault() as Label;
            if (sumLabel != null)
            {
                sumLabel.Text = "合计 =>   " + string.Join("  ", sumResults);
            }
        }

        /// <summary>
        /// 用于加载数据的SQL语句
        /// </summary>
        /// <returns></returns>
        //protected virtual string Sql()
        //{
        //    return null;
        //}

        /// <summary>
        /// 导出Excel
        /// </summary>
        protected void Export()
        {
            //var list = UV.GetSelectedRows(GetDataGridView1());
            //if (list == null || list.Count == 0)
            //{
            //    MessageBox.Show("没有选择要导出的数据行，请检查！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}
            ExcelHelper.Export(selectedRecords);
        }

        /// <summary>
        /// 从数据库加载数据
        /// </summary>
        private async void LoadData2(Action action = null)
        {
            // 显示进度窗口
            ProgressWindow progressWindow = new ProgressWindow();
            progressWindow.Show(this);

            await Task.Run(() =>
            {
                // 更新进度
                progressWindow.UpdateProgress(10, "正在查找SQL文件...");

                // 构建SQL文件的完整路径：当前目录/sql/文件名
                string sqlFilePath = GetSqlFilePath();

                // 检查文件是否存在
                if (!File.Exists(sqlFilePath))
                {
                    MessageBox.Show($"默认SQL文件不存在: {sqlFilePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 更新进度
                progressWindow.UpdateProgress(20, "正在读取SQL文件...");

                // 读取SQL文件内容
                string sqlFromFile = File.ReadAllText(sqlFilePath, Encoding.Default);

                // 从 SQL 文件中解析 <parameter> 标签并替换为实际参数值
                var param = Utility.ParseXmlTag(sqlFromFile, "parameter");

                // 更新进度
                progressWindow.UpdateProgress(30, "正在解析参数...");

                if (!string.IsNullOrEmpty(param))
                {
                    // 使用 JsonHelper 解析 JSON 格式的参数定义
                    List<SqlParamDefinition> paramDefinitions = JsonHelper.toObject<List<SqlParamDefinition>>(param);

                    if (paramDefinitions != null && paramDefinitions.Count > 0)
                    {
                        // 弹窗让用户输入参数值
                        using (SqlParameterDialog paramDialog = new SqlParameterDialog(paramDefinitions))
                        {
                            if (paramDialog.ShowDialog(this) != DialogResult.OK)
                            {
                                return;
                            }

                            // 根据用户输入替换 SQL 中的占位符 {name}
                            Dictionary<string, string> paramValues = paramDialog.ParamValues;
                            foreach (var kvp in paramValues)
                            {
                                sqlFromFile = sqlFromFile.Replace("{" + kvp.Key + "}", kvp.Value);
                            }
                        }
                    }
                }

                // 更新进度
                progressWindow.UpdateProgress(40, "正在执行查询语句...");

                // 执行查询
                List<T> queryResult = DaoTemplate.FindAll<T>(sqlFromFile);

                // 更新进度
                progressWindow.UpdateProgress(50, "查询语句执行完毕...");

                SetDataSource(queryResult);

                UpdatePagination(selectedRecords);

                // 更新进度
                progressWindow.UpdateProgress(60, "正在汇总结果...");

                // 从 SQL 文件中解析 <sum> 标签并计算汇总
                CalculateSumFromSql(sqlFromFile);

                // 更新进度
                progressWindow.UpdateProgress(70, "数据准备完毕...");
            });

            // 更新进度
            progressWindow.UpdateProgress(80, "正在将数据导入表格...");

            action?.Invoke();

            // 更新进度
            progressWindow.UpdateProgress(100, "完成显示！");

            // 关闭进度窗口
            progressWindow.Close();
        }

        /// <summary>
        /// 报表重新加载数据后，需要重新初始化表格列
        /// </summary>
        protected void LoadData()
        {
            LoadData2(() => {

                UV.InitializationDataGridView<T>(GetDataGridView1(), this);
                UV.InitializationDataGridView<T>(GetDataGridView2());
                // 设置筛选DataGridView
                UV.SetupFilterDataGridView<T>(GetDataGridView2());

                // 从User.FBuffer获取配置并应用到DataGridView           
                string configKey = _title;
                var valueConfig = user.GetSettings(configKey);
                if (valueConfig != null)
                {
                    Dictionary<string, object> savedConfig = null;
                    if (valueConfig is Dictionary<string, object> vc)
                        savedConfig = vc;
                    else
                        savedConfig = JsonHelper.toObject<Dictionary<string, object>>(valueConfig.ToString());
                    UV.ApplyColumnConfiguration(GetDataGridView1(), savedConfig);
                }
            });
        }

        /// <summary>
        /// 表格1查询
        /// </summary>
        protected void Query1()
        {
            FilterBills1();
            string sqlFromFile = File.ReadAllText(GetSqlFilePath(), Encoding.Default);
            CalculateSumFromSql(sqlFromFile);
            UpdatePagination(filteredRecords1);
        }

        /// <summary>
        /// 恢复底稿
        /// </summary>
        protected void ReloadRecords()
        {
            filteredRecords1 = new List<T>();
            string sqlFromFile = File.ReadAllText(GetSqlFilePath(), Encoding.Default);
            CalculateSumFromSql(sqlFromFile);
            UpdatePagination(selectedRecords);
        }

        /// <summary>
        /// 分屏的开关
        /// </summary>
        public void ShowExtraGridView()
        {
            GetSplitContainer().Panel2Collapsed = GetSplitContainer().Panel2Collapsed ? false : true;
        }

        /// <summary>
        /// 全选
        /// </summary>
        protected void SelectAll()
        {
            // 获取dataGridView1的所有行
            DataGridView dataGridView = GetDataGridView1();

            // 遍历所有行，设置复选框为选中状态
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                // 设置Selection列（复选框列）的值为true
                row.Cells["Selection"].Value = true;

                // 更新行的背景色
                UV.UpdateRowSelectionState(dataGridView, row.Index);
            }
        }

        protected void ColumnSettings()
        {
            UV.ColumnSettings<T>(GetDataGridView1(), user, this);
        }

        /// <summary>
        /// 选择打印模板
        /// </summary>
        protected void SelectPrintTemplate(string title)
        {
            // 创建并配置打开文件对话框
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel文件|*.xls;*.xlsx";
            openFileDialog.Title = "请选择Excel打印模板文件";
            openFileDialog.Multiselect = false;

            // 显示对话框并处理用户选择
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // 获取选中的文件路径
                string filePath = openFileDialog.FileName;

                // 使用User.FBuffer保存配置，键名包含类型信息以避免冲突
                string configKey = title + "." + "PrintTemplate";
                user.SetSettings(configKey, filePath);
                DaoTemplate.Save(user);
            }
        }

        /// <summary>
        /// 打印选择的模版
        /// </summary>
        protected void PrintSeletedTemplate(string title)
        {
            var records = UV.GetSelectedRows(GetDataGridView1());

            if (0 == records.Count)
            {
                MessageBox.Show("没有选择要打印数据行，请检查！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string configKey = title + "." + "PrintTemplate";
            if (null == user.GetSettings(configKey))
            {
                MessageBox.Show("没有选择打印模板，请检查！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string templatePath = user.GetSettings(configKey).ToString();
            ExcelHelperEx.Print(templatePath, records, 1, "Default");
        }

        /// <summary>
        /// dataGridView1 单元格双击事件处理
        /// 双击时获取当前行绑定的实体对象
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        protected void DataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // 检查行索引是否有效（排除列头双击）
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            DataGridView dataGridView = sender as DataGridView;
            if (dataGridView == null)
                return;

            // 通过 DataBoundItem 获取当前行绑定的实体对象
            T entity = dataGridView.Rows[e.RowIndex].DataBoundItem as T;
            if (entity == null)
                return;

            // 根据双击列的列名，判断entity对应字段是否有Popup特性
            string columnName = dataGridView.Columns[e.ColumnIndex].Name;
            var prop = typeof(T).GetProperty(columnName, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
                return;

            var popupAttr = Attribute.GetCustomAttribute(prop, typeof(PopupAttribute)) as PopupAttribute;
            if (popupAttr != null)
            {
                OpenPopup(popupAttr, entity);
                return;
            }

            var popupWindowAttr = Attribute.GetCustomAttribute(typeof(T), typeof(DataGridView_RowDoubleClickAttribute)) as DataGridView_RowDoubleClickAttribute;
            if (popupWindowAttr != null)
            {
                OpenPopup(popupWindowAttr, entity);
            }
        }

        /// <summary>
        /// 根据popupAttr.Parameter中保存的属性名，从对应的控件中获取属性值的数组
        /// </summary>
        /// <returns></returns>
        private object[] GetParameter(PopupAttribute popupAttr, T entity)
        {
            if (popupAttr.Parameter == null || popupAttr.Parameter.Length == 0)
                return null;

            object[] parameterValues = new object[popupAttr.Parameter.Length];
            for (int i = 0; i < popupAttr.Parameter.Length; i++)
            {
                string parameterPropertyName = popupAttr.Parameter[i];
                var propertyInfo = typeof(T).GetProperty(parameterPropertyName);
                if (propertyInfo != null)
                {
                    parameterValues[i] = propertyInfo.GetValue(entity);
                }
            }
            return parameterValues;
        }

        private object[] GetParameter(DataGridView_RowDoubleClickAttribute popupAttr, T entity)
        {
            if (popupAttr.Parameter == null || popupAttr.Parameter.Length == 0)
                return null;

            object[] parameterValues = new object[popupAttr.Parameter.Length];
            for (int i = 0; i < popupAttr.Parameter.Length; i++)
            {
                string parameterPropertyName = popupAttr.Parameter[i];
                var propertyInfo = typeof(T).GetProperty(parameterPropertyName);
                if (propertyInfo != null)
                {
                    parameterValues[i] = propertyInfo.GetValue(entity);
                }
            }
            return parameterValues;
        }



        /// <summary>
        /// 返回类型弹窗，返回数据
        /// </summary>
        /// <param name="popupAttr"></param>
        /// <param name="action"></param>
        protected void OpenPopup(PopupAttribute popupAttr, T entity, Action<Form> action = null)
        {
            // 获取弹窗类型
            Type popupType = popupAttr.Type;
            if (popupType == null)
            {
                MessageBox.Show("未配置弹窗类型", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 实例化弹窗表单
            if (!(Activator.CreateInstance(popupType) is Form popupTypeForm))
            {
                MessageBox.Show("无法创建弹窗实例", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 检查弹窗类型是否继承自GridViewForm3
            if (!typeof(GridViewForm3).IsAssignableFrom(popupType))
            {
                MessageBox.Show($"弹窗类型 {popupAttr.Type} 必须继承自 GridViewForm3", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 创建弹窗实例
            Form popupForm = Activator.CreateInstance(popupType) as Form;
            if (popupForm == null)
            {
                MessageBox.Show($"无法创建弹窗实例: {popupAttr.Type}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DataGridView dataGridView = UV.FindDataGridViewWithForm(this);
            popupForm.Text = popupAttr.Name;

            // 如果指定了SQL，执行查询并设置数据源
            if (!string.IsNullOrEmpty(popupAttr.Sql))
            {
                // 替换SQL中的参数（如果需要）
                // 检查SQL中是否包含占位符 {0} 以及是否有参数
                var sql = popupAttr.Sql;
                var parameters = GetParameter(popupAttr, entity);

                if (sql.Contains("{0}") && parameters != null && parameters.Length > 0)
                {
                    sql = string.Format(sql, parameters);
                }

                // 这里可以根据需要替换SQL中的占位符，例如：
                // sql = sql.Replace("@FID", this.record.FID.ToString());

                // 执行查询
                List<Dictionary<string, object>> queryResult = DaoTemplate.FindAll(sql);

                // 获取弹窗的SetDataSource方法
                MethodInfo setDataSourceMethod = popupType.GetMethod("SetDataSource");
                if (setDataSourceMethod != null)
                {
                    // 调用SetDataSource方法设置数据
                    setDataSourceMethod.Invoke(popupForm, new object[] { queryResult });
                }
            }
            else if (!string.IsNullOrEmpty(popupAttr.SqlFile))
            {
                // 从SqlFile指定的文件中读取SQL语句
                try
                {
                    // 获取当前应用程序目录
                    string appDirectory = Utils.GetAppDirectory();

                    // 构建SQL文件的完整路径：当前目录/sql/文件名
                    string sqlFilePath = Path.Combine(appDirectory, "sql", popupAttr.SqlFile);

                    // 检查文件是否存在
                    if (!File.Exists(sqlFilePath))
                    {
                        MessageBox.Show($"SQL文件不存在: {sqlFilePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 读取SQL文件内容
                    string sqlFromFile = File.ReadAllText(sqlFilePath, Encoding.Default);

                    // 从 SQL 文件中解析 <parameter> 标签并替换为实际参数值
                    var param = Utility.ParseXmlTag(sqlFromFile, "parameter");

                    if (!string.IsNullOrEmpty(param))
                    {
                        popupAttr.Parameter = param.Split(';');
                    }

                    var parameters = GetParameter(popupAttr, entity);

                    if (sqlFromFile.Contains("{0}") && parameters != null && parameters.Length > 0)
                    {
                        sqlFromFile = string.Format(sqlFromFile, parameters);
                    }

                    // 执行查询
                    List<Dictionary<string, object>> queryResult = DaoTemplate.FindAll(sqlFromFile);

                    // 获取弹窗的SetDataSource方法
                    MethodInfo setDataSourceMethod = popupType.GetMethod("SetDataSource");
                    if (setDataSourceMethod != null)
                    {
                        // 调用SetDataSource方法设置数据
                        setDataSourceMethod.Invoke(popupForm, new object[] { queryResult });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"读取或执行SQL文件时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                // 默认SQL文件路径逻辑：Popup_{实体类名}_{属性名}.sql
                try
                {
                    string propertyName = null;
                    Type entityType = typeof(T);
                    foreach (var prop in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var attr = Attribute.GetCustomAttribute(prop, typeof(PopupAttribute)) as PopupAttribute;
                        if (null != attr && attr.Name == popupAttr.Name) // 引用比较，确保是同一个特性实例
                        {
                            propertyName = prop.Name;
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(propertyName))
                    {
                        MessageBox.Show("无法确定触发弹窗的属性名，无法加载默认SQL文件", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string entityTypeName = typeof(T).Name;
                    string sqlFileName = $"Popup_{entityTypeName}_{propertyName}.sql";

                    // 获取当前应用程序目录
                    string appDirectory = Utils.GetAppDirectory();

                    // 构建SQL文件的完整路径：当前目录/sql/文件名
                    string sqlFilePath = Path.Combine(appDirectory, "sql", sqlFileName);

                    // 检查文件是否存在
                    if (!File.Exists(sqlFilePath))
                    {
                        MessageBox.Show($"默认SQL文件不存在: {sqlFilePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 读取SQL文件内容
                    string sqlFromFile = File.ReadAllText(sqlFilePath, Encoding.Default);

                    // 从 SQL 文件中解析 <parameter> 标签并替换为实际参数值
                    var param = Utility.ParseXmlTag(sqlFromFile, "parameter");

                    if (!string.IsNullOrEmpty(param))
                    {
                        popupAttr.Parameter = param.Split(';');
                    }

                    var parameters = GetParameter(popupAttr, entity);

                    if (sqlFromFile.Contains("{0}") && parameters != null && parameters.Length > 0)
                    {
                        sqlFromFile = string.Format(sqlFromFile, parameters);
                    }

                    // 执行查询
                    List<Dictionary<string, object>> queryResult = DaoTemplate.FindAll(sqlFromFile);

                    // 获取弹窗的SetDataSource方法
                    MethodInfo setDataSourceMethod = popupType.GetMethod("SetDataSource");
                    if (setDataSourceMethod != null)
                    {
                        // 调用SetDataSource方法设置数据
                        setDataSourceMethod.Invoke(popupForm, new object[] { queryResult });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"读取或执行默认SQL文件时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // 显示弹窗
            DialogResult result = popupForm.ShowDialog(this);
            if (result == DialogResult.OK && !string.IsNullOrEmpty(popupAttr.Return))
            {
                action?.Invoke(popupForm);
            }
        }

        protected void OpenPopup(DataGridView_RowDoubleClickAttribute popupAttr, T entity)
        {
            // 获取弹窗类型
            Type popupType = popupAttr.Type;
            if (popupType == null)
            {
                MessageBox.Show("未配置弹窗类型", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 实例化弹窗表单
            if (!(Activator.CreateInstance(popupType) is Form popupTypeForm))
            {
                MessageBox.Show("无法创建弹窗实例", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 检查弹窗类型是否继承自GridViewForm3
            if (!typeof(GridViewForm3).IsAssignableFrom(popupType))
            {
                MessageBox.Show($"弹窗类型 {popupAttr.Type} 必须继承自 GridViewForm3", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 创建弹窗实例
            Form popupForm = Activator.CreateInstance(popupType) as Form;
            if (popupForm == null)
            {
                MessageBox.Show($"无法创建弹窗实例: {popupAttr.Type}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DataGridView dataGridView = UV.FindDataGridViewWithForm(this);
            popupForm.Text = popupAttr.Name;

            // 如果指定了SQL，执行查询并设置数据源
            if (!string.IsNullOrEmpty(popupAttr.Sql))
            {
                // 替换SQL中的参数（如果需要）
                // 检查SQL中是否包含占位符 {0} 以及是否有参数
                var sql = popupAttr.Sql;
                var parameters = GetParameter(popupAttr, entity);

                if (sql.Contains("{0}") && parameters != null && parameters.Length > 0)
                {
                    sql = string.Format(sql, parameters);
                }

                // 这里可以根据需要替换SQL中的占位符，例如：
                // sql = sql.Replace("@FID", this.record.FID.ToString());

                // 执行查询
                List<Dictionary<string, object>> queryResult = DaoTemplate.FindAll(sql);

                // 获取弹窗的SetDataSource方法
                MethodInfo setDataSourceMethod = popupType.GetMethod("SetDataSource");
                if (setDataSourceMethod != null)
                {
                    // 调用SetDataSource方法设置数据
                    setDataSourceMethod.Invoke(popupForm, new object[] { queryResult });
                }
            }
            else if (!string.IsNullOrEmpty(popupAttr.SqlFile))
            {
                // 从SqlFile指定的文件中读取SQL语句
                try
                {
                    // 获取当前应用程序目录
                    string appDirectory = Utils.GetAppDirectory();

                    // 构建SQL文件的完整路径：当前目录/sql/文件名
                    string sqlFilePath = Path.Combine(appDirectory, "sql", popupAttr.SqlFile);

                    // 检查文件是否存在
                    if (!File.Exists(sqlFilePath))
                    {
                        MessageBox.Show($"SQL文件不存在: {sqlFilePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 读取SQL文件内容
                    string sqlFromFile = File.ReadAllText(sqlFilePath, Encoding.Default);

                    // 从 SQL 文件中解析 <parameter> 标签并替换为实际参数值
                    var param = Utility.ParseXmlTag(sqlFromFile, "parameter");

                    if (!string.IsNullOrEmpty(param))
                    {
                        popupAttr.Parameter = param.Split(';');
                    }

                    var parameters = GetParameter(popupAttr, entity);

                    if (sqlFromFile.Contains("{0}") && parameters != null && parameters.Length > 0)
                    {
                        sqlFromFile = string.Format(sqlFromFile, parameters);
                    }

                    // 执行查询
                    List<Dictionary<string, object>> queryResult = DaoTemplate.FindAll(sqlFromFile);

                    // 获取弹窗的SetDataSource方法
                    MethodInfo setDataSourceMethod = popupType.GetMethod("SetDataSource");
                    if (setDataSourceMethod != null)
                    {
                        // 调用SetDataSource方法设置数据
                        setDataSourceMethod.Invoke(popupForm, new object[] { queryResult });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"读取或执行SQL文件时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                // 默认SQL文件路径逻辑：Popup_{实体类名}.sql
                try
                {
                    Type entityType = typeof(T);
                    
                    string entityTypeName = typeof(T).Name;
                    string sqlFileName = $"Popup_{entityTypeName}.sql";

                    // 获取当前应用程序目录
                    string appDirectory = Utils.GetAppDirectory();

                    // 构建SQL文件的完整路径：当前目录/sql/文件名
                    string sqlFilePath = Path.Combine(appDirectory, "sql", sqlFileName);

                    // 检查文件是否存在
                    if (!File.Exists(sqlFilePath))
                    {
                        MessageBox.Show($"默认SQL文件不存在: {sqlFilePath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 读取SQL文件内容
                    string sqlFromFile = File.ReadAllText(sqlFilePath, Encoding.Default);

                    // 从 SQL 文件中解析 <parameter> 标签并替换为实际参数值
                    var param = Utility.ParseXmlTag(sqlFromFile, "parameter");

                    if (!string.IsNullOrEmpty(param))
                    {
                        popupAttr.Parameter = param.Split(';');
                    }

                    var parameters = GetParameter(popupAttr, entity);

                    if (sqlFromFile.Contains("{0}") && parameters != null && parameters.Length > 0)
                    {
                        sqlFromFile = string.Format(sqlFromFile, parameters);
                    }

                    // 执行查询
                    List<Dictionary<string, object>> queryResult = DaoTemplate.FindAll(sqlFromFile);

                    // 获取弹窗的SetDataSource方法
                    MethodInfo setDataSourceMethod = popupType.GetMethod("SetDataSource");
                    if (setDataSourceMethod != null)
                    {
                        // 调用SetDataSource方法设置数据
                        setDataSourceMethod.Invoke(popupForm, new object[] { queryResult });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"读取或执行默认SQL文件时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 显示弹窗
                DialogResult result = popupForm.ShowDialog(this);
            }
        }

        public int GetPageSize()
        {
            return _pageSize;
        }

        public int GetCurrentPage()
        {
            return _currentPage;
        }

        /// <summary>
        /// 更新分页信息并显示第一页数据
        /// </summary>
        /// <param name="records">所有记录</param>
        public void UpdatePagination(List<T> records)
        {
            _allRecords = records;
            _totalCount = records.Count;
            _totalPages = (_totalCount + _pageSize - 1) / _pageSize; // 向上取整计算总页数
            _currentPage = 1;

            // 如果总页数为0，至少显示第1页
            if (_totalPages == 0)
                _totalPages = 1;

            DisplayCurrentPage();
            UpdatePageInfo();
        }

        /// <summary>
        /// 显示当前页的数据
        /// </summary>
        public void DisplayCurrentPage()
        {
            if (_allRecords == null || _allRecords.Count == 0)
            {
                GetDataGridView1().DataSource = null;
                return;
            }

            // 计算起始和结束索引
            int startIndex = (_currentPage - 1) * _pageSize;
            int endIndex = Math.Min(startIndex + _pageSize, _allRecords.Count);

            // 提取当前页的记录
            List<T> pageRecords = _allRecords.GetRange(startIndex, endIndex - startIndex);

            // 绑定到DataGridView
            UV.SetDataSourceForGridView(GetDataGridView1(), pageRecords);
        }

        /// <summary>
        /// 更新页码信息显示
        /// </summary>
        public virtual void UpdatePageInfo()
        {
            // 假设button5是"首页"，button6是"上一页"，button7是"下一页"，button8是"末页"
            // 如果有Label显示页码信息，可以在这里更新

            // 例如：如果有lblPageInfo标签
            // lblPageInfo.Text = $"第 {_currentPage} 页 / 共 {_totalPages} 页 ({_totalCount} 条记录)";
        }

        /// <summary>
        /// 跳转到首页
        /// </summary>
        public void GoToFirstPage()
        {
            if (_currentPage != 1)
            {
                _currentPage = 1;
                DisplayCurrentPage();
                UpdatePageInfo();
            }
        }

        /// <summary>
        /// 跳转到上一页
        /// </summary>
        public void GoToPreviousPage()
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                DisplayCurrentPage();
                UpdatePageInfo();
            }
        }

        /// <summary>
        /// 跳转到下一页
        /// </summary>
        public void GoToNextPage()
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                DisplayCurrentPage();
                UpdatePageInfo();
            }
        }

        /// <summary>
        /// 跳转到末页
        /// </summary>
        public void GoToLastPage()
        {
            if (_currentPage != _totalPages)
            {
                _currentPage = _totalPages;
                DisplayCurrentPage();
                UpdatePageInfo();
            }
        }

        /// <summary>
        /// 跳转到指定页
        /// </summary>
        /// <param name="pageNumber">目标页码</param>
        public void GoToPage(int pageNumber)
        {
            if (pageNumber >= 1 && pageNumber <= _totalPages && pageNumber != _currentPage)
            {
                _currentPage = pageNumber;
                DisplayCurrentPage();
                UpdatePageInfo();
            }
        }
    }
}
