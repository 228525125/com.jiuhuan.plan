using com.jiuhuan.plan.commands;
using com.jiuhuan.plan.domain;
using com.jiuhuan.plan.models;
using com.jiuhuan.plan.tools;
using QFramework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.jiuhuan.plan.view
{
    /// <summary>
    /// 所有在 tabControl1 中打开的 Form 的基类，支持一对一、一对多、多对多的实体
    /// </summary>
    public class GridViewForm<T> : BaseForm, IPagination<T> where T : Entity, new()
    {
        protected List<T> selectedRecords = new List<T>();
        protected List<T> filteredRecords1 = new List<T>();
        protected List<Workday> workdays = new List<Workday>();
        protected User user = null;

        // 分页相关变量
        protected int _pageSize = 200;          // 每页显示行数
        protected int _currentPage = 1;         // 当前页码
        protected int _totalPages = 0;          // 总页数
        protected int _totalCount = 0;          // 总记录数
        private List<T> _allRecords = new List<T>(); // 所有记录

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // GridViewForm
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "GridViewForm";
            this.Load += new System.EventHandler(this.GridViewForm_Load);
            this.ResumeLayout(false);

        }

        private void GridViewForm_Load(object sender, EventArgs e)
        {

        }

        protected void InitializeData()
        {
            this.workdays = DaoTemplate.FindAll<Workday>();
            GetSplitContainer().Panel2Collapsed =  true;

            UV.InitializationDataGridView<T>(GetDataGridView1(), this);
            UV.InitializationDataGridView2<T>(GetDataGridView2());
            // 设置筛选DataGridView
            UV.SetupFilterDataGridView<T>(GetDataGridView2());

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

            user = this.GetModel<ISessionModel>().GetUser();

            // 从User.FBuffer获取配置并应用到DataGridView           
            string configKey = typeof(T).FullName;
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

            var num = user.GetSettings(User.FPageSize);
            if ((bool)Utility.GetAttributeValueByClass<T>("Entity", "Pagination"))
                _pageSize = null != num ? int.Parse(num.ToString()) : 200;
            else
                _pageSize = 2000;

            LoadData();
        }

        public void UpdateData()
        {
            workdays = DaoTemplate.FindAll<Workday>();
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

        /// <summary>
        /// 用于查询1
        /// </summary>
        protected virtual void FilterBills1()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 验证合法性
        /// </summary>
        protected virtual bool ValidateRecord()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 用于加载数据的SQL语句
        /// </summary>
        /// <returns></returns>
        protected virtual string Sql()
        {
            return null;
        }

        /// <summary>
        /// 同时刷新两个表格
        /// </summary>
        protected void UpdateUI()
        {
            //Query1();
            //Query2();

            GetDataGridView1().Refresh();
        }

        public List<T> GetRecords()
        {
            return selectedRecords;
        }

        public const string AND_FUser = " AND FUser='{0}' ";
        public const string ORDER_BY_FDate = " ORDER BY FDate ";

        /// <summary>
        /// 使用 User.hasPermission 方法判断当前用户是否拥有指定操作权限
        /// </summary>
        /// <param name="operation">操作</param>
        /// <returns></returns>
        private bool hasPermission(string operation)
        {
            //return Utils.HasPermission(operation, this.GetType(), user);
            return true;
        }

        /// <summary>
        /// 根据实体配置和用户信息构建最终SQL语句
        /// </summary>
        /// <param name="baseSql">基础SQL语句</param>
        /// <returns>处理后的SQL语句</returns>
        private string BuildFinalSql(string baseSql)
        {
            string sqlTemplate = baseSql;
            bool isEntirety = (bool)Utility.GetAttributeValueByClass<T>("Entity", "Entirety");

            // 根据 Entirety 属性值拼接 SQL
            if (isEntirety)
            {
                // 如果为 true，只添加排序
                sqlTemplate += ORDER_BY_FDate;
                return sqlTemplate;
            }
            else
            {
                // 如果为 false，添加用户过滤和排序
                sqlTemplate += AND_FUser + ORDER_BY_FDate;
                return string.Format(sqlTemplate, user.FName);
            }
        }

        /// <summary>
        /// 从数据库加载数据
        /// </summary>
        public async void LoadData()
        {
            if (!hasPermission("浏览")) return;

            // 显示进度窗口
            ProgressWindow progressWindow = new ProgressWindow();
            progressWindow.Show(this);

            await Task.Run(() =>
            {
                // 更新进度
                progressWindow.UpdateProgress(10, "正在执行SQL语句...");

                if (null == Sql())
                {
                    if ((bool)Utility.GetAttributeValueByClass<T>("Entity", "Entirety"))
                    {
                        selectedRecords = DaoTemplate.FindAll<T>();

                        // 更新进度
                        progressWindow.UpdateProgress(50, "执行SQL语句成功...");
                    }
                    else
                    {
                        selectedRecords = DaoTemplate.FindAllByUser<T>(user.FName);

                        // 更新进度
                        progressWindow.UpdateProgress(50, "执行SQL语句成功...");
                    }
                }
                else
                {
                    string ss = Sql();
                    string sql = BuildFinalSql(ss);
                    selectedRecords = UV.LoadData<T>(sql);

                    // 更新进度
                    progressWindow.UpdateProgress(50, "执行SQL语句成功...");
                }
            });

            // 更新进度
            progressWindow.UpdateProgress(70, "数据准备完毕...");

            // 更新进度
            progressWindow.UpdateProgress(80, "正在将数据导入表格...");

            UpdatePagination(selectedRecords);

            // 更新进度
            progressWindow.UpdateProgress(100, "完成显示！");

            // 关闭进度窗口
            progressWindow.Close();
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
        /// 清空表格内容
        /// </summary>
        protected void Clear1()
        {
            UV.ClearDataSourceForGridView(GetDataGridView1(), selectedRecords, filteredRecords1);
        }

        /// <summary>
        /// 恢复底稿
        /// </summary>
        protected void ReloadRecords()
        {
            UpdatePagination(selectedRecords);
        }

        /// <summary>
        /// 删除选中行，包括底稿和数据库
        /// </summary>
        protected void DeleteSelectedRows()
        {
            if (!hasPermission("删除")) return;

            var list = UV.DeleteSelectedRows<T>(GetDataGridView1(), selectedRecords, this);

            UpdatePagination(list);
        }

        /// <summary>
        /// 保存底稿到数据库
        /// </summary>
        protected void Save()
        {
            if (!hasPermission("修改")) return;

            if (selectedRecords.Count == 0)
            {
                MessageBox.Show("没有需要保存的数据，请检查！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DaoTemplate.DeleteAll<T>(user.FName);
            UV.SaveAsync(selectedRecords, this);
        }

        /// <summary>
        /// 重置，清楚所有内容，包括数据库
        /// </summary>
        protected void Reset()
        {
            if (!hasPermission("删除")) return;

            var result = MessageBox.Show($"确定要删除已保存的所有日历数据吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                DaoTemplate.DeleteAll<T>(user.FName);
                Clear1();
                MessageBox.Show("删除成功！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 导出Excel
        /// </summary>
        protected void Export()
        {
            if (!hasPermission("导出")) return;

            //var list = UV.GetSelectedRows<T>(GetDataGridView1());
            ExcelHelper.Export(selectedRecords);
        }

        /// <summary>
        /// 导入Excel
        /// </summary>
        protected async void Import()
        {
            if (!hasPermission("新增")) return;

            var list = await ExcelHelper.Import<T>();
            DaoTemplate.Save(list);
        }

        /// <summary>
        /// 分屏的开关
        /// </summary>
        public void ShowExtraGridView()
        {
            if (!hasPermission("浏览")) return;

            GetSplitContainer().Panel2Collapsed = GetSplitContainer().Panel2Collapsed ? false : true;
        }

        /// <summary>
        /// 创建并显示EditForm
        /// </summary>
        /// <typeparam name="EditForm"></typeparam>
        protected void CreateRecord<EditForm>() where EditForm : EditPopup<T>, new()
        {
            if (!hasPermission("浏览")) return;

            var record = new T();

            // 创建编辑窗体实例
            using (var editForm = new EditForm())
            {
                // 设置编辑窗体的数据源
                editForm.SetRecord(record);

                // 显示编辑窗体（模态对话框）
                DialogResult result = editForm.ShowDialog();

                // 根据用户操作决定是否更新数据源
                if (result == DialogResult.OK)
                {
                    selectedRecords.Add(record);

                    // 保存数据
                    UV.SaveAsync(record, this, true, () => {
                        // 检查T类型的所有属性字段是否包含OneToMany特性，如果有则单独保存子实体
                        var property = Utility.GetPropertyWithAttribute<T>("OneToMany");
                        if (null != property)
                        {
                            // 获取属性上的OneToMany特性
                            var oneToManyAttr = property.GetCustomAttribute<OneToManyAttribute>(true);

                            if (oneToManyAttr != null)
                            {
                                try
                                {
                                    // 获取子实体列表的值
                                    var childList = property.GetValue(record) as System.Collections.IList;

                                    if (childList != null && childList.Count > 0)
                                    {
                                        // 遍历子实体列表并保存
                                        foreach (var childItem in childList)
                                        {
                                            if (childItem is Entity childEntity)
                                            {
                                                // 设置外键关联（如果需要）
                                                // 这里假设子实体有指向父实体的外键，通常由ORM或业务逻辑处理
                                                // 如果需要手动设置，可根据OneToOneAttr或约定进行设置

                                                // 保存子实体
                                                UV.SaveAsync(childEntity, this);
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"保存关联数据 {property.Name} 时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }

                        // 检查T类型的所有属性字段是否包含ManyToMany特性，如果有则保存多对多关联数据
                        var manyToManyProperty = Utility.GetPropertyWithAttribute<T>("ManyToMany");
                        if (null != manyToManyProperty)
                        {
                            // 获取属性上的ManyToMany特性
                            var manyToManyAttr = manyToManyProperty.GetCustomAttribute<ManyToManyAttribute>(true);

                            if (manyToManyAttr != null)
                            {
                                try
                                {
                                    // 保存多对多关联数据到中间表
                                    SaveManyToManyRelationships(record, manyToManyProperty, manyToManyAttr);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"保存多对多关联数据 {manyToManyProperty.Name} 时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }

                        ReloadRecords();
                    });

                    
                }
            }
        }

        /// <summary>
        /// 双击dataGridView时，创建并显示EditForm，将选中行的Record数据填充到对应控件
        /// </summary>
        /// <typeparam name="EditForm"></typeparam>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void CreateEditForm<EditForm>(object sender, DataGridViewCellEventArgs e) where EditForm : EditPopup<T>, new()
        {
            if (!hasPermission("浏览")) return;

            // 验证点击位置是否有效
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            DataGridView dataGridView = sender as DataGridView;
            if (dataGridView == null)
            {
                return;
            }

            // 获取选中的行
            DataGridViewRow selectedRow = dataGridView.Rows[e.RowIndex];

            // 获取该行的数据对象
            var record = selectedRow.DataBoundItem as T;

            // 验证数据对象是否存在
            if (record == null)
            {
                MessageBox.Show("无法获取工序记录数据，请重试。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 创建编辑窗体实例
            using (var editForm = new EditForm())
            {
                // 设置编辑窗体的数据源
                editForm.SetRecord(record);

                // 显示编辑窗体（模态对话框）
                DialogResult result = editForm.ShowDialog();

                // 根据用户操作决定是否更新数据源
                if (result == DialogResult.OK)
                {
                    // 更新原始数据
                    UpdateDataSourceWithEditedRecord(record);

                    // 刷新DataGridView显示
                    UpdateUI();
                }
            }
        }

        /// <summary>
        /// 将编辑后的Record更新到数据源中
        /// </summary>
        /// <param name="editedRecord">编辑后的工单记录</param>
        private void UpdateDataSourceWithEditedRecord(T editedRecord)
        {
            // 在实际应用中，这里应该根据具体的业务逻辑来更新数据源
            // 例如：在selectedRecords列表中找到对应的记录并更新
            var recordToUpdate = selectedRecords.FirstOrDefault(r => r.FID == editedRecord.FID);

            if (recordToUpdate != null)
            {
                // 使用反射复制所有公共属性
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var property in properties)
                {
                    // 跳过只读属性和特殊处理的属性
                    if (!property.CanWrite || property.Name == "FID")
                    {
                        continue;
                    }

                    try
                    {
                        var value = property.GetValue(editedRecord);
                        property.SetValue(recordToUpdate, value);
                    }
                    catch (Exception ex)
                    {
                        // 记录可能的异常但不中断操作
                        Console.WriteLine($"更新属性 {property.Name} 时出错: {ex.Message}");
                    }
                }

                // 保存数据
                UV.SaveAsync(recordToUpdate, this, true, () => {

                    // 检查T类型的所有属性字段是否包含OneToMany特性，如果有则单独保存子实体
                    var property = Utility.GetPropertyWithAttribute<T>("OneToMany");
                    if (null != property)
                    {
                        // 获取属性上的OneToMany特性
                        var oneToManyAttr = property.GetCustomAttribute<OneToManyAttribute>(true);

                        if (oneToManyAttr != null)
                        {
                            try
                            {
                                // 删除旧的子实体数据
                                DeleteOldChildEntities(recordToUpdate, property, oneToManyAttr);

                                // 获取子实体列表的值
                                var childList = property.GetValue(recordToUpdate) as System.Collections.IList;

                                if (childList != null && childList.Count > 0)
                                {
                                    // 遍历子实体列表并保存
                                    foreach (var childItem in childList)
                                    {
                                        if (childItem is Entity childEntity)
                                        {
                                            // 设置外键关联（如果需要）
                                            // 这里假设子实体有指向父实体的外键，通常由ORM或业务逻辑处理
                                            // 如果需要手动设置，可根据OneToOneAttr或约定进行设置

                                            // 保存子实体
                                            UV.SaveAsync(childEntity, this, false);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"保存关联数据 {property.Name} 时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }

                    // 检查T类型的所有属性字段是否包含ManyToMany特性，如果有则保存多对多关联数据
                    var manyToManyProperty = Utility.GetPropertyWithAttribute<T>("ManyToMany");
                    if (null != manyToManyProperty)
                    {
                        // 获取属性上的ManyToMany特性
                        var manyToManyAttr = manyToManyProperty.GetCustomAttribute<ManyToManyAttribute>(true);

                        if (manyToManyAttr != null)
                        {
                            try
                            {
                                // 先删除旧的中间表关系数据，再保存新的关系数据
                                DeleteManyToManyRelationships(recordToUpdate, manyToManyProperty, manyToManyAttr);
                                SaveManyToManyRelationships(recordToUpdate, manyToManyProperty, manyToManyAttr);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"保存多对多关联数据 {manyToManyProperty.Name} 时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                });
            }
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

        /// <summary>
        /// 排序，固定查询结果的顺序
        /// </summary>
        protected void FixedSequence()
        {
            foreach (var record in selectedRecords)
                record.FStatus = 0;

            if(!filteredRecords1.Any())
                MessageBox.Show("请先进行查询！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);

            // 获取dataGridView1的所有行
            DataGridView dataGridView = GetDataGridView1();
            dataGridView.Columns["FStatus"].Visible = true;
            
            // 遍历所有行，设置复选框为选中状态
            int index = 10;
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                var rowData = row.DataBoundItem as Entity;
                rowData.FStatus = index;
                index += 10;
            }

            dataGridView.Refresh();
        }

        /// <summary>
        /// 取消排序
        /// </summary>
        protected void CannelSequence()
        {
            foreach (var record in selectedRecords)
                record.FStatus = 0;

            // 获取dataGridView1的所有行
            DataGridView dataGridView = GetDataGridView1();
            dataGridView.Columns["FStatus"].Visible = false;

            // 遍历所有行，设置复选框为选中状态
            //foreach (DataGridViewRow row in dataGridView.Rows)
            //{
            //    var rowData = row.DataBoundItem as Entity;
            //    rowData.FStatus = 0;
            //}

            dataGridView.Refresh();
        }

        protected void MoveUp()
        {
            // 获取当前选中的行
            if (GetDataGridView1().SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要上移的行。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 获取选中行的索引（假设只处理第一个选中行，避免多行选择时的复杂逻辑）
            int selectedIndex = GetDataGridView1().SelectedRows[0].Index;

            // 如果已经是第一行，则无法上移
            if (selectedIndex <= 0)
            {
                MessageBox.Show("已到达顶部，无法继续上移。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 获取数据源列表
            List<T> records = GetDataGridView1().DataSource as List<T>;
            if (records == null || selectedIndex >= records.Count)
            {
                MessageBox.Show("数据源无效或索引越界。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var record = GetDataGridView1().SelectedRows[0].DataBoundItem as T;

            if(record.FStatus == 0)
            {
                MessageBox.Show("请先进行排序！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 使用 UV.MoveUp 方法交换当前行与上一行的数据
            UV.MoveUp(records, record);

            // 刷新 DataGridView 显示
            GetDataGridView1().DataSource = null;
            GetDataGridView1().DataSource = records;

            // 重新选中移动后的行
            GetDataGridView1().ClearSelection();
            GetDataGridView1().Rows[selectedIndex - 1].Selected = true;
        }

        protected void MoveDown()
        {
            // 获取当前选中的行
            if (GetDataGridView1().SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要下移的行。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 获取选中行的索引（假设只处理第一个选中行，避免多行选择时的复杂逻辑）
            int selectedIndex = GetDataGridView1().SelectedRows[0].Index;

            // 获取数据源列表
            List<T> records = GetDataGridView1().DataSource as List<T>;
            if (records == null || selectedIndex < 0 || selectedIndex >= records.Count)
            {
                MessageBox.Show("数据源无效或索引越界。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 如果已经是最后一行，则无法下移
            if (selectedIndex >= records.Count - 1)
            {
                MessageBox.Show("已到达底部，无法继续下移。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var record = GetDataGridView1().SelectedRows[0].DataBoundItem as T;

            if (record.FStatus == 0)
            {
                MessageBox.Show("请先进行排序！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 使用 UV.MoveDown 方法交换当前行与下一行的数据
            UV.MoveDown(records, record);

            // 刷新 DataGridView 显示
            GetDataGridView1().DataSource = null;
            GetDataGridView1().DataSource = records;

            // 重新选中移动后的行
            GetDataGridView1().ClearSelection();
            GetDataGridView1().Rows[selectedIndex + 1].Selected = true;
        }

        /// <summary>
        /// 配置列信息
        /// </summary>
        protected void ColumnSettings()
        {
            if (!hasPermission("浏览")) return;

            UV.ColumnSettings<T>(GetDataGridView1(), user, this);
        }

        /// <summary>
        /// 查询实体（数据字典）
        /// </summary>
        //protected void QueryDictionary()
        //{
        //    string className = typeof(T).Name;
        //    string templatePath = Utils.GetTemplateDirectory() + className + ".txt";
        //    Utils.OpenFile(templatePath);
        //}

        /// <summary>
        /// 打开默认打印样式
        /// </summary>
        protected void OpenDefaultPrintTemplate()
        {
            if (!hasPermission("打印")) return;

            // 获取泛型类型 T 的类名（不含命名空间）
            string className = typeof(T).Name;
            string templatePath = Utils.GetTemplateDirectory() + className + ".xls";
            Utils.OpenFile(templatePath);
        }

        //protected void PrintDefaultPrintTemplate(List<T> records, int times, string printer)
        //{
        //    // 获取泛型类型 T 的类名（不含命名空间）
        //    string className = typeof(T).Name;
        //    string templatePath = Utils.GetTemplateDirectory() + className + ".xls";
        //    ExcelHelperEx.Print(templatePath, records, times, printer);
        //}

        /// <summary>
        /// 打印默认样式
        /// </summary>
        protected void PrintDefaultPrintTemplate()
        {
            if (!hasPermission("打印")) return;

            var records = UV.GetSelectedRows<T>(GetDataGridView1());

            if (0 == records.Count)
            {
                MessageBox.Show("没有选择要打印数据行，请检查！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 获取泛型类型 T 的类名（不含命名空间）
            string className = typeof(T).Name;
            string templatePath = Utils.GetTemplateDirectory() + className + ".xls";
            ExcelHelperEx.Print(templatePath, records, 1, "Default");
        }

        /// <summary>
        /// 选择打印模板
        /// </summary>
        protected void SelectPrintTemplate()
        {
            if (!hasPermission("打印")) return;

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
                string configKey = typeof(T).FullName + "." + "PrintTemplate";
                user.SetSettings(configKey, filePath);
                DaoTemplate.Save(user);
            }
        }

        /// <summary>
        /// 打印选择的模版
        /// </summary>
        protected void PrintSeletedTemplate()
        {
            if (!hasPermission("打印")) return;

            var records = UV.GetSelectedRows<T>(GetDataGridView1());

            if(0 == records.Count)
            {
                MessageBox.Show("没有选择要打印数据行，请检查！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string configKey = typeof(T).FullName + "." + "PrintTemplate";
            if(null == user.GetSettings(configKey))
            {
                MessageBox.Show("没有选择打印模板，请检查！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string templatePath = user.GetSettings(configKey).ToString();
            ExcelHelperEx.Print(templatePath, records, 1, "Default");
        }

        /// <summary>
        /// 显示实体信息
        /// </summary>
        protected void ShowEntityInformation()
        {
            if (!hasPermission("浏览")) return;

            // 创建并配置窗体
            using (var infoForm = new Form())
            {
                infoForm.Text = "实体信息 - " + typeof(T).Name;
                infoForm.Size = new Size(800, 600);
                infoForm.StartPosition = FormStartPosition.CenterParent;
                infoForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                infoForm.MaximizeBox = false;
                infoForm.MinimizeBox = false;

                // 创建RichTextBox用于显示内容
                RichTextBox richTextBox = new RichTextBox();
                richTextBox.Dock = DockStyle.Fill;
                richTextBox.ReadOnly = true;
                richTextBox.Font = new Font("Microsoft YaHei", 12f);
                richTextBox.BackColor = Color.White;

                // 构建显示内容
                StringBuilder content = new StringBuilder();
                
                // 第一行：实体名称（类名）
                content.AppendLine($"实体名称：{typeof(T).Name}");
                content.AppendLine(); // 空行

                // 获取T的所有属性及其ColumnAttribute
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                
                foreach (var property in properties)
                {
                    // 获取ColumnAttribute
                    var columnAttr = property.GetCustomAttribute<ColumnAttribute>();
                    var fieldAttr = property.GetCustomAttribute<FieldAttribute>();

                    if (columnAttr != null && !string.IsNullOrEmpty(columnAttr.Title))
                    {
                        // 格式：属性名 -> ColumnAttribute.Name
                        content.AppendLine($"{property.Name,-20} -> {columnAttr.Title}");
                        continue;
                    }

                    if(fieldAttr != null && !string.IsNullOrEmpty(fieldAttr.Name))
                    {
                        content.AppendLine($"{property.Name,-20} -> {fieldAttr.Name}");
                    }
                }

                // 设置内容
                richTextBox.Text = content.ToString();

                // 创建按钮面板
                FlowLayoutPanel buttonPanel = new FlowLayoutPanel();
                buttonPanel.Dock = DockStyle.Bottom;
                buttonPanel.Height = 50;
                buttonPanel.Padding = new Padding(10);
                buttonPanel.FlowDirection = FlowDirection.RightToLeft;
                buttonPanel.WrapContents = false;

                // 关闭按钮
                Button btnClose = new Button();
                btnClose.Text = "关闭";
                btnClose.Size = new Size(80, 30);
                btnClose.Click += (s, e) => infoForm.Close();

                buttonPanel.Controls.Add(btnClose);

                // 添加控件到窗体
                infoForm.Controls.Add(richTextBox);
                infoForm.Controls.Add(buttonPanel);

                // 显示窗体
                infoForm.ShowDialog(this);
            }

        }

        /// <summary>
        /// 根据T的特性、属性等信息初始化数据库
        /// </summary>
        protected void InitializeDatabase()
        {
            if (!hasPermission("浏览")) return;

            // 获取当前登录用户的密码，要求输入密码才能执行初始化操作
            //User currentUser = this.GetModel<ISessionModel>().GetUser();
            //if (currentUser == null)
            //{
            //    MessageBox.Show("无法获取当前用户信息，请重新登录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}
            
            // 创建密码输入对话框
            using (Form passwordForm = new Form())
            {
                passwordForm.Text = "确认操作";
                passwordForm.Size = new Size(350, 180);
                passwordForm.StartPosition = FormStartPosition.CenterParent;
                passwordForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                passwordForm.MaximizeBox = false;
                passwordForm.MinimizeBox = false;
            
                Label tipLabel = new Label();
                tipLabel.Text = "警告：初始化数据库将删除所有现有数据！\n请输入管理员密码以确认操作：";
                tipLabel.Location = new Point(15, 15);
                tipLabel.Size = new Size(310, 40);
                passwordForm.Controls.Add(tipLabel);
            
                TextBox passwordTextBox = new TextBox();
                passwordTextBox.PasswordChar = '*';
                passwordTextBox.Location = new Point(15, 60);
                passwordTextBox.Size = new Size(300, 25);
                passwordForm.Controls.Add(passwordTextBox);
            
                Button okButton = new Button();
                okButton.Text = "确定";
                okButton.DialogResult = DialogResult.OK;
                okButton.Location = new Point(140, 95);
                okButton.Size = new Size(75, 30);
                passwordForm.Controls.Add(okButton);
            
                Button cancelButton = new Button();
                cancelButton.Text = "取消";
                cancelButton.DialogResult = DialogResult.Cancel;
                cancelButton.Location = new Point(225, 95);
                cancelButton.Size = new Size(75, 30);
                passwordForm.Controls.Add(cancelButton);
            
                passwordForm.AcceptButton = okButton;
                passwordForm.CancelButton = cancelButton;
            
                // 显示密码输入对话框，如果用户取消则返回
                if (passwordForm.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
            
                // 验证输入的密码是否与当前用户密码一致
                string inputPassword = passwordTextBox.Text.Trim();
                if (string.IsNullOrEmpty(inputPassword) || inputPassword != "12123")
                {
                    MessageBox.Show("密码错误，操作已取消！", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            DaoTemplate.CreateTable<T>();

            if (null != DaoTemplate.SqlException)
                MessageBox.Show($"数据库初始化失败！{DaoTemplate.SqlException.Message}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                MessageBox.Show("数据库初始化完成！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // 检查T类型是否包含OneToMany特性，如果有则创建关联表的数据库结构
            // 检查T类型的所有属性字段是否包含OneToMany特性
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                // 获取属性上的OneToMany特性
                var oneToManyAttr = property.GetCustomAttribute<OneToManyAttribute>(true);

                if (oneToManyAttr != null)
                {
                    // 获取OneToMany特性中指定的子实体类型
                    Type childEntityType = oneToManyAttr.ChildType;
                    if (childEntityType != null)
                    {
                        // 创建子实体对应的数据库表
                        var sql = DaoTemplate.CreateTable(childEntityType);

                        // 检查创建过程中是否有异常
                        if (null != DaoTemplate.SqlException)
                        {
                            MessageBox.Show($"创建关联表 {childEntityType.Name} 失败：{DaoTemplate.SqlException.Message}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
            }

            // 检查T类型是否包含ManyToMany特性，如果有则创建中间表
            foreach (var property in properties)
            {
                // 获取属性上的ManyToMany特性
                var manyToManyAttr = property.GetCustomAttribute<ManyToManyAttribute>(true);

                if (manyToManyAttr != null)
                {
                    // 获取中间表名称
                    string mappingTable = manyToManyAttr.MappingTable;
                    Type childType = manyToManyAttr.ChildType;
                    string joinColumn = manyToManyAttr.JoinColumn;
                    string mappedBy = manyToManyAttr.MappedBy;

                    if (!string.IsNullOrEmpty(mappingTable) && !string.IsNullOrEmpty(joinColumn) && !string.IsNullOrEmpty(mappedBy))
                    {
                        // 构建中间表字段名：实体名_字段名
                        string parentEntityFieldName = $"{typeof(T).Name}_{joinColumn}";
                        string childEntityFieldName = $"{childType.Name}_{mappedBy}";

                        // 创建中间表的SQL语句
                        string createMappingTableSql = $@"
                            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '{mappingTable}')
                            CREATE TABLE [{mappingTable}] (
                                [{parentEntityFieldName}] NVARCHAR(200),
                                [{childEntityFieldName}] NVARCHAR(200)
                            )";

                        DaoTemplate.ExecuteNonQuery(createMappingTableSql);

                        if (null != DaoTemplate.SqlException)
                        {
                            MessageBox.Show($"创建中间表 {mappingTable} 失败：{DaoTemplate.SqlException.Message}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
            }

            var formName = Utility.GetAttributeValueByClass<T>("Entity", "FormName") as string;
            var type = "";
            var name = "";
            if (string.IsNullOrEmpty(formName))
            {
                name = typeof(T).Name;
                type = name + "Form";
            }
            else
            {
                type = formName;
            }

            var number = type + ".V01";

            var document = new Document() { FNumber = number, FName = name, FType = type};
            DaoTemplate.Save(document);

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

        /// <summary>
        /// 删除旧的子实体数据
        /// </summary>
        /// <param name="parentRecord">父记录对象</param>
        /// <param name="property">被 OneToMany 特性修饰的属性</param>
        /// <param name="oneToManyAttr">OneToMany 特性对象</param>
        private void DeleteOldChildEntities(T parentRecord, PropertyInfo property, OneToManyAttribute oneToManyAttr)
        {
            if (parentRecord == null || property == null || oneToManyAttr == null)
                return;

            try
            {
                // 获取 OneToMany 特性的配置
                string joinColumn = oneToManyAttr.JoinColumn;
                string mappedBy = oneToManyAttr.MappedBy;
                Type childType = oneToManyAttr.ChildType;

                if (string.IsNullOrEmpty(joinColumn) || string.IsNullOrEmpty(mappedBy))
                {
                    Console.WriteLine($"属性 {property.Name} 的 OneToMany 特性未正确配置 JoinColumn 或 MappedBy");
                    return;
                }

                // 获取父记录中 joinColumn 字段的值
                var joinColumnProperty = typeof(T).GetProperty(joinColumn);
                if (joinColumnProperty == null)
                {
                    Console.WriteLine($"父记录中未找到字段 {joinColumn}");
                    return;
                }

                object parentKeyValue = joinColumnProperty.GetValue(parentRecord);

                if (parentKeyValue == null)
                {
                    Console.WriteLine($"父记录的 {joinColumn} 字段值为空，跳过删除旧数据");
                    return;
                }

                // 构建删除 SQL：删除旧的子实体数据
                string tableName = childType.Name;
                string formattedValue = DaoTemplate.FormatSqlValue(parentKeyValue);
                string deleteSql = $"DELETE FROM [{tableName}] WHERE [{mappedBy}] = {formattedValue}";

                // 执行删除操作
                int deletedCount = DaoTemplate.ExecuteNonQuery(deleteSql);
                Console.WriteLine($"删除旧子实体数据：{deletedCount} 行");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"删除旧子实体数据时出错: {ex.Message}");
                // 不抛出异常，避免阻断主流程
            }
        }

        /// <summary>
        /// 保存多对多关联数据到中间表
        /// 中间表字段命名规则：实体名_关联字段名，如 Department_FNumber、User_FName
        /// </summary>
        /// <param name="parentRecord">父记录对象</param>
        /// <param name="property">被 ManyToMany 特性修饰的属性</param>
        /// <param name="manyToManyAttr">ManyToMany 特性对象</param>
        private void SaveManyToManyRelationships(T parentRecord, PropertyInfo property, ManyToManyAttribute manyToManyAttr)
        {
            if (parentRecord == null || property == null || manyToManyAttr == null)
                return;

            try
            {
                // 获取 ManyToMany 特性的配置
                string mappingTable = manyToManyAttr.MappingTable;
                string joinColumn = manyToManyAttr.JoinColumn;
                string mappedBy = manyToManyAttr.MappedBy;
                Type childType = manyToManyAttr.ChildType;

                if (string.IsNullOrEmpty(mappingTable) || string.IsNullOrEmpty(joinColumn) || string.IsNullOrEmpty(mappedBy))
                {
                    Console.WriteLine($"属性 {property.Name} 的 ManyToMany 特性未正确配置 MappingTable、JoinColumn 或 MappedBy");
                    return;
                }

                // 获取父记录中 joinColumn 字段的值
                var joinColumnProperty = typeof(T).GetProperty(joinColumn);
                if (joinColumnProperty == null)
                {
                    Console.WriteLine($"父记录中未找到字段 {joinColumn}");
                    return;
                }

                object parentKeyValue = joinColumnProperty.GetValue(parentRecord);

                if (parentKeyValue == null)
                {
                    Console.WriteLine($"父记录的 {joinColumn} 字段值为空，跳过保存中间表数据");
                    return;
                }

                // 获取子实体列表
                var childList = property.GetValue(parentRecord) as System.Collections.IList;

                if (childList == null || childList.Count == 0)
                {
                    Console.WriteLine($"属性 {property.Name} 的关联数据为空，跳过保存中间表数据");
                    return;
                }

                // 构建中间表字段名：实体名_字段名
                string parentEntityFieldName = $"{typeof(T).Name}_{joinColumn}";
                string childEntityFieldName = $"{childType.Name}_{mappedBy}";

                string formattedParentValue = DaoTemplate.FormatSqlValue(parentKeyValue);

                // 遍历子实体列表，逐条插入中间表
                int insertedCount = 0;
                foreach (var childItem in childList)
                {
                    // 获取子实体中 mappedBy 字段的值
                    var mappedByProperty = childType.GetProperty(mappedBy);
                    if (mappedByProperty == null)
                    {
                        Console.WriteLine($"关联实体 {childType.Name} 中未找到字段 {mappedBy}");
                        continue;
                    }

                    object childKeyValue = mappedByProperty.GetValue(childItem);

                    if (childKeyValue == null)
                    {
                        continue;
                    }

                    string formattedChildValue = DaoTemplate.FormatSqlValue(childKeyValue);

                    // 插入中间表记录
                    string insertSql = $"INSERT INTO [{mappingTable}] ([{parentEntityFieldName}], [{childEntityFieldName}]) VALUES ({formattedParentValue}, {formattedChildValue})";
                    DaoTemplate.ExecuteNonQuery(insertSql);
                    insertedCount++;
                }

                Console.WriteLine($"保存多对多关联数据到中间表 {mappingTable}：{insertedCount} 条记录");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存多对多关联数据时出错: {ex.Message}");
                // 不抛出异常，避免阻断主流程
            }
        }

        /// <summary>
        /// 删除多对多关联数据（从中间表中删除与父记录关联的所有关系记录）
        /// 中间表字段命名规则：实体名_关联字段名，如 Department_FNumber、User_FName
        /// </summary>
        /// <param name="parentRecord">父记录对象</param>
        /// <param name="property">被 ManyToMany 特性修饰的属性</param>
        /// <param name="manyToManyAttr">ManyToMany 特性对象</param>
        private void DeleteManyToManyRelationships(T parentRecord, PropertyInfo property, ManyToManyAttribute manyToManyAttr)
        {
            if (parentRecord == null || property == null || manyToManyAttr == null)
                return;

            try
            {
                // 获取 ManyToMany 特性的配置
                string mappingTable = manyToManyAttr.MappingTable;
                string joinColumn = manyToManyAttr.JoinColumn;
                Type childType = manyToManyAttr.ChildType;

                if (string.IsNullOrEmpty(mappingTable) || string.IsNullOrEmpty(joinColumn))
                {
                    Console.WriteLine($"属性 {property.Name} 的 ManyToMany 特性未正确配置 MappingTable 或 JoinColumn");
                    return;
                }

                // 获取父记录中 joinColumn 字段的值
                var joinColumnProperty = typeof(T).GetProperty(joinColumn);
                if (joinColumnProperty == null)
                {
                    Console.WriteLine($"父记录中未找到字段 {joinColumn}");
                    return;
                }

                object parentKeyValue = joinColumnProperty.GetValue(parentRecord);

                if (parentKeyValue == null)
                {
                    Console.WriteLine($"父记录的 {joinColumn} 字段值为空，跳过删除中间表数据");
                    return;
                }

                // 构建中间表字段名：实体名_字段名
                string parentEntityFieldName = $"{typeof(T).Name}_{joinColumn}";

                // 构建删除 SQL：删除中间表中与父记录关联的所有记录
                string formattedValue = DaoTemplate.FormatSqlValue(parentKeyValue);
                string deleteSql = $"DELETE FROM [{mappingTable}] WHERE [{parentEntityFieldName}] = {formattedValue}";

                // 执行删除操作
                int deletedCount = DaoTemplate.ExecuteNonQuery(deleteSql);
                Console.WriteLine($"删除中间表 {mappingTable} 关联数据：{deletedCount} 条记录");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"删除多对多关联数据时出错: {ex.Message}");
                // 不抛出异常，避免阻断主流程
            }
        }
    }
}
