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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.jiuhuan.plan
{
    /// <summary>
    /// 所有在 tabControl1 中打开的 Form 的基类，支持一对多的实体
    /// </summary>
    public class GridViewForm4<T> : BaseForm, IPagination<T> where T : Entity, new()
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
            // GridViewForm4
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "GridViewForm4";
            this.Load += new System.EventHandler(this.GridViewForm4_Load);
            this.ResumeLayout(false);

        }

        private void GridViewForm4_Load(object sender, EventArgs e)
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
                ApplyColumnConfiguration(GetDataGridView1(), savedConfig);
            }

            var num = user.GetSettings(User.FPageSize);
            if ((bool)Utility.GetAttributeValueByClass<T>("Entity", "Pagination"))
                _pageSize = null != num ? int.Parse(num.ToString()) : 200;
            else
                _pageSize = 2000;
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
        public void LoadData()
        {
            if(null == Sql())
            {
                if ((bool)Utility.GetAttributeValueByClass<T>("Entity", "Entirety"))
                {
                    selectedRecords = DaoTemplate.FindAll<T>();
                }
                else
                {
                    selectedRecords = DaoTemplate.FindAllByUser<T>(user.FName);
                }
            }
            else
            {
                string ss = Sql();
                string sql = BuildFinalSql(ss);
                selectedRecords = UV.LoadData<T>(sql);
            }

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
        /// 清空表格内容
        /// </summary>
        protected void Clear1()
        {
            //UV.ClearDataSourceForGridView(GetDataGridView1(), selectedRecords, filteredRecords1);
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
            var list = UV.DeleteSelectedRows<T>(GetDataGridView1(), selectedRecords, this);

            UpdatePagination(list);
        }

        /// <summary>
        /// 保存底稿到数据库
        /// </summary>
        protected void Save()
        {
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
            var list = UV.GetSelectedRows<T>(GetDataGridView1());
            ExcelHelper.Export(list);
        }

        /// <summary>
        /// 导入Excel
        /// </summary>
        protected async void Import()
        {
            var list = await ExcelHelper.Import<T>();
            DaoTemplate.Save(list);
        }

        /// <summary>
        /// 分屏的开关
        /// </summary>
        public void ShowExtraGridView()
        {
            GetSplitContainer().Panel2Collapsed = GetSplitContainer().Panel2Collapsed ? false : true;
        }

        /// <summary>
        /// 创建并显示EditForm
        /// </summary>
        /// <typeparam name="EditForm"></typeparam>
        protected void CreateRecord<EditForm>() where EditForm : EditPopup<T>, new()
        {
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
                    UV.SaveAsync(record, this, false, () => {

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
        protected void UpdateDataSourceWithEditedRecord(T editedRecord)
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
                UV.SaveAsync(recordToUpdate, this, true, ()=> {

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

        protected void ColumnSettings()
        {
            // 创建列配置窗体
            using (var columnConfigForm = new Form())
            {
                columnConfigForm.Text = "列显示配置";
                columnConfigForm.Size = new Size(800, 500);
                columnConfigForm.StartPosition = FormStartPosition.CenterParent;

                // 创建DataGridView
                DataGridView dgvColumns = new DataGridView();
                dgvColumns.Dock = DockStyle.Fill;
                dgvColumns.AllowUserToAddRows = false;
                dgvColumns.AllowUserToDeleteRows = false;
                dgvColumns.ReadOnly = false;
                dgvColumns.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvColumns.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // 添加四列 - 修正：在添加列时直接指定正确的列类型
                DataGridViewTextBoxColumn columnNameColumn = new DataGridViewTextBoxColumn();
                columnNameColumn.Name = "ColumnName";
                columnNameColumn.HeaderText = "列名";
                columnNameColumn.ReadOnly = true;
                dgvColumns.Columns.Add(columnNameColumn);

                DataGridViewTextBoxColumn orderColumn = new DataGridViewTextBoxColumn();
                orderColumn.Name = "Order";
                orderColumn.HeaderText = "顺序";
                orderColumn.ValueType = typeof(int);
                dgvColumns.Columns.Add(orderColumn);

                DataGridViewCheckBoxColumn visibleColumn = new DataGridViewCheckBoxColumn();
                visibleColumn.Name = "Visible";
                visibleColumn.HeaderText = "显示";
                visibleColumn.ReadOnly = false;
                dgvColumns.Columns.Add(visibleColumn);

                DataGridViewTextBoxColumn widthColumn = new DataGridViewTextBoxColumn();
                widthColumn.Name = "Width";
                widthColumn.HeaderText = "列宽";
                widthColumn.ValueType = typeof(int);
                dgvColumns.Columns.Add(widthColumn);

                // 获取T的所有属性及其ColumnAttribute
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var property in properties)
                {
                    // 获取ColumnAttribute
                    var columnAttr = property.GetCustomAttribute<ColumnAttribute>();

                    // 如果没有ColumnAttribute，跳过
                    if (columnAttr == null)
                        continue;

                    // 添加行数据
                    int rowIndex = dgvColumns.Rows.Add();
                    DataGridViewRow row = dgvColumns.Rows[rowIndex];

                    row.Cells["ColumnName"].Value = (string.IsNullOrEmpty(columnAttr.Title) ? Utility.GetAttributeValueByField<T>("Field", "Name", property.Name) as string : columnAttr.Title) ?? property.Name;
                    row.Cells["Order"].Value = columnAttr.Index;
                    row.Cells["Visible"].Value = columnAttr.Visible;
                    row.Cells["Width"].Value = columnAttr.Width > 0 ? columnAttr.Width : 100;

                    // 存储属性名称以便后续使用
                    row.Tag = property.Name;
                }

                // 创建按钮面板 - 使用FlowLayoutPanel自动排列按钮
                FlowLayoutPanel buttonPanel = new FlowLayoutPanel();
                buttonPanel.Dock = DockStyle.Bottom;
                buttonPanel.Height = 50;
                buttonPanel.Padding = new Padding(10);
                buttonPanel.FlowDirection = FlowDirection.RightToLeft;
                buttonPanel.WrapContents = false;
                buttonPanel.AutoSize = true;

                // 加载设置按钮
                Button btnLoad = new Button();
                btnLoad.Text = "加载设置";
                btnLoad.BackColor = Color.LightYellow;
                btnLoad.Size = new Size(80, 30);
                btnLoad.Click += (s, args) =>
                {
                    LoadColumnConfiguration(dgvColumns);
                };

                // 保存设置按钮
                Button btnSave = new Button();
                btnSave.Text = "保存设置";
                btnSave.BackColor = Color.LightYellow;
                btnSave.Size = new Size(80, 30);
                btnSave.Click += (s, args) =>
                {
                    SaveColumnConfiguration(dgvColumns);
                    MessageBox.Show("列配置已保存！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };

                // 取消按钮
                Button btnCancel = new Button();
                btnCancel.Text = "取消";
                btnCancel.Size = new Size(80, 30);
                btnCancel.Click += (s, args) => columnConfigForm.Close();

                // 确定按钮
                Button btnOK = new Button();
                btnOK.Text = "确定";
                btnOK.Size = new Size(80, 30);
                btnOK.Click += (s, args) =>
                {
                    // 应用列配置
                    ApplyColumnConfiguration(GetDataGridView1(), dgvColumns);
                    columnConfigForm.Close();
                };

                // 按从右到左的顺序添加按钮
                buttonPanel.Controls.Add(btnCancel);
                buttonPanel.Controls.Add(btnOK);
                buttonPanel.Controls.Add(btnSave);
                //buttonPanel.Controls.Add(btnLoad);

                // 添加控件到窗体
                columnConfigForm.Controls.Add(dgvColumns);
                columnConfigForm.Controls.Add(buttonPanel);

                // 窗体显示后自动加载已保存的列配置
                columnConfigForm.Load += (s, e1) =>
                {
                    LoadColumnConfiguration(dgvColumns);
                    // 按 Order 列升序排序
                    dgvColumns.Sort(dgvColumns.Columns["Order"], System.ComponentModel.ListSortDirection.Ascending);
                };

                dgvColumns.CellValueChanged += (sender, e) =>
                {
                    // 按 Order 列升序排序
                    dgvColumns.Sort(dgvColumns.Columns["Order"], System.ComponentModel.ListSortDirection.Ascending);
                };

                // 显示窗体
                columnConfigForm.ShowDialog(this);
            }
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
            // 获取泛型类型 T 的类名（不含命名空间）
            string className = typeof(T).Name;
            string templatePath = Utils.GetTemplateDirectory() + className + ".xls";
            Utils.OpenFile(templatePath);
        }

        protected void PrintDefaultPrintTemplate(List<T> records, int times, string printer)
        {
            // 获取泛型类型 T 的类名（不含命名空间）
            string className = typeof(T).Name;
            string templatePath = Utils.GetTemplateDirectory() + className + ".xls";
            ExcelHelperEx.Print(templatePath, records, times, printer);
        }

        /// <summary>
        /// 打印默认样式
        /// </summary>
        protected void PrintDefaultPrintTemplate()
        {
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
        /// 应用列配置到DataGridView
        /// </summary>
        /// <param name="targetDataGridView">目标DataGridView</param>
        /// <param name="configDataGridView">配置DataGridView</param>
        private void ApplyColumnConfiguration(DataGridView targetDataGridView, DataGridView configDataGridView)
        {
            // 创建列表存储列配置信息
            List<ColumnConfigInfo> columnConfigs = new List<ColumnConfigInfo>();

            foreach (DataGridViewRow configRow in configDataGridView.Rows)
            {
                string propertyName = configRow.Tag as string;
                if (string.IsNullOrEmpty(propertyName))
                    continue;

                int order = Convert.ToInt32(configRow.Cells["Order"].Value);
                bool visible = Convert.ToBoolean(configRow.Cells["Visible"].Value);
                int width = Convert.ToInt32(configRow.Cells["Width"].Value);

                // 在目标DataGridView中查找对应的列
                DataGridViewColumn column = targetDataGridView.Columns.Cast<DataGridViewColumn>()
                    .FirstOrDefault(c => c.DataPropertyName == propertyName);

                if (column != null)
                {
                    columnConfigs.Add(new ColumnConfigInfo
                    {
                        Column = column,
                        Order = order,
                        Visible = visible,
                        Width = width
                    });
                }
            }

            // 根据Order排序
            columnConfigs = columnConfigs.OrderBy(c => c.Order).ToList();

            // 应用配置
            foreach (var config in columnConfigs)
            {
                config.Column.DisplayIndex = columnConfigs.IndexOf(config);
                config.Column.Visible = config.Visible;
                if (config.Width > 0)
                {
                    config.Column.Width = config.Width;
                }
            }

            // 确保 Selection 和 RowNumber 列显示在最前面
            if (targetDataGridView.Columns.Contains("Selection"))
            {
                targetDataGridView.Columns["Selection"].DisplayIndex = 0;
            }

            if (targetDataGridView.Columns.Contains("RowNumber"))
            {
                targetDataGridView.Columns["RowNumber"].DisplayIndex = 1;
            }
        }

        /// <summary>
        /// 列配置信息类
        /// </summary>
        private class ColumnConfigInfo
        {
            public DataGridViewColumn Column { get; set; }
            public int Order { get; set; }
            public bool Visible { get; set; }
            public int Width { get; set; }
        }

        /// <summary>
        /// 应用列配置到DataGridView（从User.FBuffer加载）
        /// </summary>
        /// <param name="targetDataGridView">目标DataGridView</param>
        /// <param name="config">列配置字典</param>
        private void ApplyColumnConfiguration(DataGridView targetDataGridView, Dictionary<string, object> config)
        {
            if (config == null || !config.Any())
            {
                return;
            }

            // 创建列表存储列配置信息
            List<ColumnConfigInfo> columnConfigs = new List<ColumnConfigInfo>();

            // 按属性名分组处理
            var propertyNames = config.Keys
                .Where(k => k.EndsWith("_Visible") || k.EndsWith("_Width") || k.EndsWith("_Order"))
                .Select(k => k.Substring(0, k.LastIndexOf('_')))
                .Distinct();

            foreach (string propertyName in propertyNames)
            {
                // 在目标DataGridView中查找对应的列
                DataGridViewColumn column = targetDataGridView.Columns.Cast<DataGridViewColumn>()
                    .FirstOrDefault(c => c.DataPropertyName == propertyName);

                if (column == null)
                {
                    continue;
                }

                int order = 0;
                bool visible = true;
                int width = 100;

                // 获取Order
                string orderKey = propertyName + "_Order";
                if (config.ContainsKey(orderKey))
                {
                    order = Convert.ToInt32(config[orderKey]);
                }

                // 获取Visible
                string visibleKey = propertyName + "_Visible";
                if (config.ContainsKey(visibleKey))
                {
                    visible = Convert.ToBoolean(config[visibleKey]);
                }

                // 获取Width
                string widthKey = propertyName + "_Width";
                if (config.ContainsKey(widthKey))
                {
                    width = Convert.ToInt32(config[widthKey]);
                }

                columnConfigs.Add(new ColumnConfigInfo
                {
                    Column = column,
                    Order = order,
                    Visible = visible,
                    Width = width
                });
            }

            // 根据Order排序
            columnConfigs = columnConfigs.OrderBy(c => c.Order).ToList();

            // 应用配置
            for (int i = 0; i < columnConfigs.Count; i++)
            {
                columnConfigs[i].Column.DisplayIndex = i;
                columnConfigs[i].Column.Visible = columnConfigs[i].Visible;
                if (columnConfigs[i].Width > 0)
                {
                    columnConfigs[i].Column.Width = columnConfigs[i].Width;
                }
            }

            // 确保 Selection 和 RowNumber 列显示在最前面
            if (targetDataGridView.Columns.Contains("Selection"))
            {
                targetDataGridView.Columns["Selection"].DisplayIndex = 0;
            }

            if (targetDataGridView.Columns.Contains("RowNumber"))
            {
                targetDataGridView.Columns["RowNumber"].DisplayIndex = 1;
            }
        }

        /// <summary>
        /// 保存列配置到User.FBuffer
        /// </summary>
        /// <param name="configDataGridView">配置DataGridView</param>
        private void SaveColumnConfiguration(DataGridView configDataGridView)
        {
            try
            {
                // 构建配置字典
                Dictionary<string, object> columnConfig = new Dictionary<string, object>();

                foreach (DataGridViewRow row in configDataGridView.Rows)
                {
                    string propertyName = row.Tag as string;
                    if (string.IsNullOrEmpty(propertyName))
                        continue;

                    // 保存顺序、可见性和列宽
                    columnConfig[propertyName + "_Order"] = Convert.ToInt32(row.Cells["Order"].Value);
                    columnConfig[propertyName + "_Visible"] = Convert.ToBoolean(row.Cells["Visible"].Value);
                    columnConfig[propertyName + "_Width"] = Convert.ToInt32(row.Cells["Width"].Value);
                }

                // 使用User.FBuffer保存配置，键名包含类型信息以避免冲突
                string configKey = typeof(T).FullName;
                user.SetSettings(configKey, columnConfig);
                DaoTemplate.Save(user);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存列配置失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 从User.FBuffer加载列配置
        /// </summary>
        /// <param name="configDataGridView">配置DataGridView</param>
        private void LoadColumnConfiguration(DataGridView configDataGridView)
        {
            try
            {
                // 从User.FBuffer获取配置
                string configKey = typeof(T).FullName;
                var valueConfig = user.GetSettings(configKey);

                if (valueConfig == null)
                {
                    //MessageBox.Show("没有找到保存的列配置！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Dictionary<string, object> savedConfig = null;
                if (valueConfig is Dictionary<string, object> vc)
                    savedConfig = vc;
                else
                    savedConfig = JsonHelper.toObject<Dictionary<string, object>>(valueConfig.ToString());

                // 应用配置到DataGridView
                foreach (DataGridViewRow row in configDataGridView.Rows)
                {
                    string propertyName = row.Tag as string;
                    if (string.IsNullOrEmpty(propertyName))
                        continue;

                    // 加载顺序
                    string orderKey = propertyName + "_Order";
                    if (savedConfig.ContainsKey(orderKey))
                    {
                        row.Cells["Order"].Value = Convert.ToInt32(savedConfig[orderKey]);
                    }

                    // 加载可见性
                    string visibleKey = propertyName + "_Visible";
                    if (savedConfig.ContainsKey(visibleKey))
                    {
                        row.Cells["Visible"].Value = Convert.ToBoolean(savedConfig[visibleKey]);
                    }

                    // 加载列宽
                    string widthKey = propertyName + "_Width";
                    if (savedConfig.ContainsKey(widthKey))
                    {
                        row.Cells["Width"].Value = Convert.ToInt32(savedConfig[widthKey]);
                    }
                }

                //MessageBox.Show("列配置已加载！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载列配置失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 选择打印模板
        /// </summary>
        protected void SelectPrintTemplate()
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
            // 弹出确认对话框，提示用户初始化将导致数据丢失
            var result = MessageBox.Show(
                "警告：初始化数据库将删除该实体类型的所有现有数据！\n\n确定要继续吗？",
                "确认初始化",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            // 如果用户选择“否”或关闭对话框，则取消操作
            if (result != DialogResult.Yes)
            {
                return;
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
        /// 格式化 SQL 参数值
        /// </summary>
        /// <param name="value">要格式化的值</param>
        /// <returns>格式化后的 SQL 字符串</returns>
        private string FormatSqlValue(object value)
        {
            if (value == null)
                return "NULL";

            Type valueType = value.GetType();

            // 处理字符串类型
            if (valueType == typeof(string))
            {
                return $"'{value.ToString().Replace("'", "''")}'";
            }
            // 处理日期时间类型
            else if (valueType == typeof(DateTime) || valueType == typeof(DateTime?))
            {
                DateTime dt = Convert.ToDateTime(value);
                return $"'{dt:yyyy-MM-dd HH:mm:ss}'";
            }
            // 处理布尔类型
            else if (valueType == typeof(bool))
            {
                return (bool)value ? "1" : "0";
            }
            // 处理数值类型（int, decimal, double, float等）
            else if (valueType == typeof(int) || valueType == typeof(int?) ||
                     valueType == typeof(long) || valueType == typeof(long?) ||
                     valueType == typeof(decimal) || valueType == typeof(decimal?) ||
                     valueType == typeof(double) || valueType == typeof(double?) ||
                     valueType == typeof(float) || valueType == typeof(float?))
            {
                return value.ToString();
            }
            // 其他类型，转换为字符串并加引号
            else
            {
                return $"'{value.ToString().Replace("'", "''")}'";
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
    }
}
