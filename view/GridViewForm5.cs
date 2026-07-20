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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.jiuhuan.plan.view
{
    /// <summary>
    /// 报表，带查询、分页等
    /// </summary>
    public class GridViewForm5 : Form, IPagination
    {
        protected List<Dictionary<string, object>> selectedRecords = new List<Dictionary<string, object>>();
        protected List<Dictionary<string, object>> filteredRecords1 = new List<Dictionary<string, object>>();
        private List<string> fieldNames = new List<string>();
        protected User user = null;
        protected string _title = "";

        // 分页相关变量
        protected int _pageSize = 200;          // 每页显示行数
        protected int _currentPage = 1;         // 当前页码
        protected int _totalPages = 0;          // 总页数
        protected int _totalCount = 0;          // 总记录数
        private List<Dictionary<string, object>> _allRecords = new List<Dictionary<string, object>>(); // 所有记录

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(282, 253);
            this.Name = "GridViewForm5";
            this.Load += new EventHandler(this.GridViewForm5_Load);
            this.ResumeLayout(false);
        }

        private void GridViewForm5_Load(object sender, EventArgs e)
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

                filteredRecords1 = UV.ApplyFilter(GetDataGridView1(), GetDataGridView2(), selectedRecords);
                UpdatePagination(filteredRecords1);
            };

            // 监听dataGridView2的按键事件
            GetDataGridView2().KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    filteredRecords1 = UV.ApplyFilter(GetDataGridView1(), GetDataGridView2(), selectedRecords);
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

            _pageSize = 50;

            LoadData();

            UV.InitializationDataGridView(GetDataGridView1(), this, selectedRecords);
            UV.InitializationDataGridView(GetDataGridView2(), selectedRecords);
            // 设置筛选DataGridView
            UV.SetupFilterDataGridView(GetDataGridView2(), selectedRecords);
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

        protected virtual void FilterBills1()
        {
            string queryText = GetTextBox1().Text.Trim();

            // 过滤条件 - bill是Dictionary<string, object>类型
            filteredRecords1 = selectedRecords.Where(bill =>
            {
                // 文本查询过滤
                if (!string.IsNullOrEmpty(queryText))
                {
                    bool found = false;

                    foreach (var field in fieldNames)
                    {
                        if (bill.ContainsKey(field) && bill[field] != null &&
                        bill[field].ToString().Contains(queryText))
                            found = true;
                    }

                    // 如果没有找到匹配的字段，返回false
                    if (!found)
                        return false;
                }

                return true;
            }).ToList();
        }

        public void SetDataSource(List<Dictionary<string, object>> list)
        {
            selectedRecords = list;
            if (null != list && list.Any())
            {
                fieldNames.AddRange(list[0].Keys);
            }
        }

        public void SetTitle(string title)
        {
            this._title = title;
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
            var list = UV.GetSelectedRows(GetDataGridView1());
            if (list == null || list.Count == 0)
            {
                MessageBox.Show("没有选择要导出的数据行，请检查！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ExcelHelper.Export(list);
        }

        /// <summary>
        /// 从数据库加载数据
        /// </summary>
        public void LoadData()
        {
            string sqlFileName = $"Report_{_title}.sql";

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

            // 执行查询
            List<Dictionary<string, object>> queryResult = DaoTemplate.FindAll(sqlFromFile);

            SetDataSource(queryResult);

            UpdatePagination(selectedRecords);
        }

        /// <summary>
        /// 表格1查询
        /// </summary>
        protected void Query1()
        {
            FilterBills1();

            UpdatePagination(filteredRecords1);
        }

        /// <summary>
        /// 恢复底稿
        /// </summary>
        protected void ReloadRecords()
        {
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
            UV.ColumnSettings(GetDataGridView1(), selectedRecords, _title, user, this);
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
        public void UpdatePagination(List<Dictionary<string, object>> records)
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
            List<Dictionary<string, object>> pageRecords = _allRecords.GetRange(startIndex, endIndex - startIndex);

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
