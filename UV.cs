using com.jiuhuan.plan.domain;
using com.jiuhuan.plan.expression;
using com.jiuhuan.plan.tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.jiuhuan.plan
{
    class ColumnConfigInfo
    {
        public DataGridViewColumn Column { get; set; }
        public int Order { get; set; }
        public bool Visible { get; set; }
        public int Width { get; set; }
    }

    public class UV
    {
        public static void ColumnSettings(DataGridView dataGridView, List<Dictionary<string, object>> list, string title, User user, IWin32Window win)
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
                //var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var item = list[0];

                for (int i = 0; i < item.Keys.Count; i++)
                {
                    var key = item.Keys.ElementAt(i);
                    // 添加行数据
                    int rowIndex = dgvColumns.Rows.Add();
                    DataGridViewRow row = dgvColumns.Rows[rowIndex];

                    row.Cells["ColumnName"].Value = key;
                    row.Cells["Order"].Value = i;
                    row.Cells["Visible"].Value = true;
                    row.Cells["Width"].Value = 120;

                    // 存储属性名称以便后续使用
                    row.Tag = key;
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
                    LoadColumnConfiguration(dgvColumns, title, user);
                };

                // 保存设置按钮
                Button btnSave = new Button();
                btnSave.Text = "保存设置";
                btnSave.BackColor = Color.LightYellow;
                btnSave.Size = new Size(80, 30);
                btnSave.Click += (s, args) =>
                {
                    SaveColumnConfiguration(dgvColumns, title, user);
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
                    ApplyColumnConfiguration(dataGridView, dgvColumns);
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
                    LoadColumnConfiguration(dgvColumns, title, user);
                    // 按 Order 列升序排序
                    dgvColumns.Sort(dgvColumns.Columns["Order"], ListSortDirection.Ascending);
                };

                dgvColumns.CellValueChanged += (sender, e) =>
                {
                    // 按 Order 列升序排序
                    dgvColumns.Sort(dgvColumns.Columns["Order"], ListSortDirection.Ascending);
                };

                // 显示窗体
                columnConfigForm.ShowDialog(win);
            }
        }

        /// <summary>
        /// 打开列配置弹窗
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataGridView"></param>
        /// <param name="user"></param>
        /// <param name="win"></param>
        public static void ColumnSettings<T>(DataGridView dataGridView, User user, IWin32Window win)
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
                    LoadColumnConfiguration<T>(dgvColumns, user);
                };

                // 保存设置按钮
                Button btnSave = new Button();
                btnSave.Text = "保存设置";
                btnSave.BackColor = Color.LightYellow;
                btnSave.Size = new Size(80, 30);
                btnSave.Click += (s, args) =>
                {
                    SaveColumnConfiguration<T>(dgvColumns, user);
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
                    ApplyColumnConfiguration(dataGridView, dgvColumns);
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
                    LoadColumnConfiguration<T>(dgvColumns, user);
                    // 按 Order 列升序排序
                    dgvColumns.Sort(dgvColumns.Columns["Order"], ListSortDirection.Ascending);
                };

                dgvColumns.CellValueChanged += (sender, e) =>
                {
                    // 按 Order 列升序排序
                    dgvColumns.Sort(dgvColumns.Columns["Order"], ListSortDirection.Ascending);
                };

                // 显示窗体
                columnConfigForm.ShowDialog(win);
            }
        }

        /// <summary>
        /// 应用列配置到DataGridView
        /// </summary>
        /// <param name="targetDataGridView">目标DataGridView</param>
        /// <param name="configDataGridView">配置DataGridView</param>
        private static void ApplyColumnConfiguration(DataGridView targetDataGridView, DataGridView configDataGridView)
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
        /// 应用列配置到DataGridView（从User.FBuffer加载）
        /// </summary>
        /// <param name="targetDataGridView">目标DataGridView</param>
        /// <param name="config">列配置字典</param>
        public static void ApplyColumnConfiguration(DataGridView targetDataGridView, Dictionary<string, object> config)
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
        private static void SaveColumnConfiguration(DataGridView configDataGridView, string configKey, User user)
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
                //string configKey = typeof(T).FullName;
                user.SetSettings(configKey, columnConfig);
                DaoTemplate.Save(user);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存列配置失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 保存列配置到User.FBuffer
        /// </summary>
        /// <param name="configDataGridView">配置DataGridView</param>
        private static void SaveColumnConfiguration<T>(DataGridView configDataGridView, User user)
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
        private static void LoadColumnConfiguration(DataGridView configDataGridView, string configKey, User user)
        {
            try
            {
                // 从User.FBuffer获取配置
                //string configKey = typeof(T).FullName;
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
        /// 从User.FBuffer加载列配置
        /// </summary>
        /// <param name="configDataGridView">配置DataGridView</param>
        private static void LoadColumnConfiguration<T>(DataGridView configDataGridView, User user)
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
        /// 获取实体类型对应的表名
        /// </summary>
        /// <param name="entityType">实体类型</param>
        /// <returns>表名，如果无法获取则返回null</returns>
        public static string GetTableName(Type entityType)
        {
            if (entityType == null)
                return null;

            return entityType.Name;
        }

        public static T CreateEntityFromDataGridViewRow<T>(DataGridViewRow row)
        {
            return (T)CreateEntityFromDataGridViewRow(row, typeof(T));
        }

        /// <summary>
        /// 从DataGridView行创建实体对象
        /// </summary>
        /// <param name="row">DataGridView行</param>
        /// <param name="entityType">实体类型</param>
        /// <returns>创建的实体对象，如果创建失败则返回null</returns>
        public static object CreateEntityFromDataGridViewRow(DataGridViewRow row, Type entityType)
        {
            if (row == null || entityType == null)
                return null;

            try
            {
                // 创建实体实例
                object entity = Activator.CreateInstance(entityType);

                // 遍历实体的所有属性
                foreach (var property in entityType.GetProperties())
                {
                    // 查找匹配的列（优先使用属性名，其次使用 FieldAttribute.Name）
                    string columnName = "";

                    // 尝试从 FieldAttribute 获取列名
                    var fieldAttr = Attribute.GetCustomAttribute(property, typeof(FieldAttribute)) as FieldAttribute;
                    if (fieldAttr != null && !string.IsNullOrEmpty(fieldAttr.Name))
                    {
                        // 查找与 FieldAttribute.Name 匹配的列
                        foreach (DataGridViewColumn column in row.DataGridView.Columns)
                        {
                            if (string.Equals(column.HeaderText, fieldAttr.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                columnName = column.Name;
                                break;
                            }
                        }
                    }

                    // 检查行中是否包含该列
                    if (!string.IsNullOrEmpty(columnName) && row.Cells[columnName] != null && row.Cells[columnName].Value != null)
                    {
                        object value = row.Cells[columnName].Value;

                        // 尝试转换值到属性类型
                        try
                        {
                            if (property.PropertyType.IsAssignableFrom(value.GetType()))
                            {
                                // 类型匹配，直接赋值
                                property.SetValue(entity, value);
                            }
                            else
                            {
                                // 尝试类型转换
                                object convertedValue = Convert.ChangeType(value, property.PropertyType);
                                property.SetValue(entity, convertedValue);
                            }
                        }
                        catch
                        {
                            // 转换失败，跳过该属性
                            continue;
                        }
                    }
                }

                return entity;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"从DataGridView行创建实体 {entityType.Name} 时出错: {ex.Message}");
                return null;
            }
        }

        public static T ConvertDictionaryToEntity<T>(Dictionary<string, object> dict)
        {
            return (T)ConvertDictionaryToEntity(dict, typeof(T));
        }

        /// <summary>
        /// 将Dictionary转换为实体对象
        /// </summary>
        /// <param name="dict">字典数据</param>
        /// <param name="entityType">实体类型</param>
        /// <returns>转换后的实体对象，如果转换失败则返回null</returns>
        public static object ConvertDictionaryToEntity(Dictionary<string, object> dict, Type entityType)
        {
            if (dict == null || entityType == null)
                return null;

            try
            {
                // 创建实体实例
                object entity = Activator.CreateInstance(entityType);

                // 遍历实体的所有属性
                foreach (var property in entityType.GetProperties())
                {
                    // 查找匹配的字典键（优先使用属性名，其次使用 FieldAttribute.Name）
                    string key = property.Name;

                    // 尝试从 FieldAttribute 获取列名
                    var fieldAttr = Attribute.GetCustomAttribute(property, typeof(FieldAttribute)) as FieldAttribute;
                    if (fieldAttr != null && !string.IsNullOrEmpty(fieldAttr.Name))
                    {
                        // 如果字典中包含 FieldAttribute.Name 对应的键，优先使用
                        if (dict.ContainsKey(fieldAttr.Name))
                        {
                            key = fieldAttr.Name;
                        }
                    }

                    // 检查字典中是否包含该键
                    if (dict.ContainsKey(key) && dict[key] != null)
                    {
                        object value = dict[key];

                        // 尝试转换值到属性类型
                        try
                        {
                            if (property.PropertyType.IsAssignableFrom(value.GetType()))
                            {
                                // 类型匹配，直接赋值
                                property.SetValue(entity, value);
                            }
                            else
                            {
                                // 尝试类型转换
                                object convertedValue = Convert.ChangeType(value, property.PropertyType);
                                property.SetValue(entity, convertedValue);
                            }
                        }
                        catch
                        {
                            // 转换失败，跳过该属性
                            continue;
                        }
                    }
                }

                return entity;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"将Dictionary转换为实体 {entityType.Name} 时出错: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 将字典数据添加到 DataGridView（支持 DataTable 和 List<Entity> 两种数据源）
        /// </summary>
        /// <param name="dataGridView">DataGridView</param>
        /// <param name="dataTable">如果数据源是 DataTable，则为该对象，否则为 null</param>
        /// <param name="listDataSource">如果数据源是 List<T>，则为该对象，否则为 null</param>
        /// <param name="listElementType">List<T> 中元素的类型</param>
        /// <param name="itemData">字典数据</param>
        /// <param name="backMapping">字段映射关系，格式如："主件品号=FItemCode;元件品号=FSubCode"</param>
        public static void AddItemToDataGridView(DataGridView dataGridView, DataTable dataTable,
            IList listDataSource, Type listElementType,
            Dictionary<string, object> itemData, string backMapping)
        {
            if (itemData == null || string.IsNullOrEmpty(backMapping))
                return;

            try
            {
                if (dataTable != null)
                {
                    // 数据源是 DataTable，向 DataTable 添加行
                    AddItemToDataTable(dataTable, itemData, backMapping);
                }
                else if (listDataSource != null && listElementType != null)
                {
                    // 数据源是 List<T>，创建实体并添加到列表
                    object entity = ConvertDictionaryToEntity(itemData, listElementType);
                    if (entity != null)
                    {
                        // 获取 List<T> 的 Add 方法
                        var addMethod = listDataSource.GetType().GetMethod("Add");
                        if (addMethod != null)
                        {
                            addMethod.Invoke(listDataSource, new object[] { entity });
                        }
                    }

                    SetDataSourceForGridView(dataGridView, listDataSource);
                }
                else
                {
                    AddItemToDataGridView(dataGridView, itemData, backMapping);
                }

                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"向 DataGridView 添加数据时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 将字典数据添加到 DataTable 中
        /// </summary>
        /// <param name="dataTable">目标 DataTable</param>
        /// <param name="itemData">字典数据</param>
        /// <param name="backMapping">字段映射关系，格式如："主件品号=FItemCode;元件品号=FSubCode"</param>
        public static void AddItemToDataTable(DataTable dataTable, Dictionary<string, object> itemData, string backMapping)
        {
            if (dataTable == null || itemData == null || string.IsNullOrEmpty(backMapping))
                return;

            try
            {
                // 创建新行
                DataRow newRow = dataTable.NewRow();

                // 解析字段映射关系
                string[] mappings = backMapping.Split(';');

                // 遍历映射关系，设置对应列的值
                foreach (string mapping in mappings)
                {
                    if (string.IsNullOrWhiteSpace(mapping))
                        continue;

                    // 分割显示名称和数据列名，例如 "主件品号=FItemCode"
                    string[] parts = mapping.Split('=');
                    if (parts.Length != 2)
                        continue;

                    string displayLabel = parts[0].Trim(); // 例如 "主件品号"
                    string dataColumn = parts[1].Trim();   // 例如 "FItemCode"

                    // 检查 DataTable 中是否存在该列
                    if (dataTable.Columns.Contains(displayLabel) && itemData.ContainsKey(displayLabel))
                    {
                        object value = itemData[displayLabel];

                        // 处理 DBNull
                        if (value == null)
                        {
                            newRow[displayLabel] = DBNull.Value;
                        }
                        else
                        {
                            newRow[displayLabel] = value;
                        }
                    }
                }

                // 将新行添加到 DataTable
                dataTable.Rows.Add(newRow);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"向 DataTable 添加行时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 将字典数据添加到DataGridView中
        /// </summary>
        /// <param name="dataGridView">目标DataGridView</param>
        /// <param name="itemData">字典数据</param>
        /// <param name="backMapping">字段映射关系，格式如："主件品号=FItemCode;元件品号=FSubCode"</param>
        public static void AddItemToDataGridView(DataGridView dataGridView, Dictionary<string, object> itemData, string backMapping)
        {
            if (dataGridView == null || itemData == null || string.IsNullOrEmpty(backMapping))
                return;

            // 解析字段映射关系
            string[] mappings = backMapping.Split(';');

            // 创建新行
            int rowIndex = dataGridView.Rows.Add();
            DataGridViewRow newRow = dataGridView.Rows[rowIndex];

            // 遍历映射关系，设置对应列的值
            foreach (string mapping in mappings)
            {
                if (string.IsNullOrWhiteSpace(mapping))
                    continue;

                // 分割显示名称和数据列名，例如 "主件品号=FItemCode"
                string[] parts = mapping.Split('=');
                if (parts.Length != 2)
                    continue;

                string displayLabel = parts[0].Trim(); // 例如 "主件品号"
                string dataColumn = parts[1].Trim();   // 例如 "FItemCode"

                // 查找DataGridView中对应的列
                DataGridViewColumn column = FindColumnByHeaderText(dataGridView, displayLabel);
                if (column != null && itemData.ContainsKey(displayLabel))
                {
                    object value = itemData[displayLabel];
                    newRow.Cells[column.Index].Value = value;
                }
            }
        }

        /// <summary>
        /// 根据列标题文本查找DataGridView列
        /// </summary>
        /// <param name="dataGridView">DataGridView控件</param>
        /// <param name="headerText">列标题文本</param>
        /// <returns>找到的列，如果未找到则返回null</returns>
        public static DataGridViewColumn FindColumnByHeaderText(DataGridView dataGridView, string headerText)
        {
            if (dataGridView == null || string.IsNullOrEmpty(headerText))
                return null;

            foreach (DataGridViewColumn column in dataGridView.Columns)
            {
                if (string.Equals(column.HeaderText, headerText, StringComparison.OrdinalIgnoreCase))
                {
                    return column;
                }
            }

            return null;
        }

        /// <summary>
        /// 在TabPage中递归查找DataGridView控件
        /// </summary>
        /// <param name="tabPage">要搜索的TabPage</param>
        /// <returns>找到的DataGridView控件，如果未找到则返回null</returns>
        public static DataGridView FindDataGridViewInTabPage(TabPage tabPage)
        {
            if (tabPage == null)
                return null;

            // 递归查找所有子控件中的DataGridView
            return FindControlRecursive<DataGridView>(tabPage);
        }

        /// <summary>
        /// 递归查找指定类型的控件
        /// </summary>
        /// <typeparam name="T">控件类型</typeparam>
        /// <param name="parent">父控件</param>
        /// <returns>找到的控件，如果未找到则返回null</returns>
        public static T FindControlRecursive<T>(Control parent) where T : Control
        {
            if (parent is T control)
                return control;

            foreach (Control child in parent.Controls)
            {
                T found = FindControlRecursive<T>(child);
                if (found != null)
                    return found;
            }

            return null;
        }

        public static DataGridView FindDataGridViewWithForm(Form form)
        {
            // 在整个表单中递归查找TabControl
            TabControl tabControl = FindControlRecursive<TabControl>(form);
            if (tabControl == null || tabControl.SelectedTab == null)
            {
                MessageBox.Show("未找到TabControl或未选中TabPage", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            TabPage currentPage = tabControl.SelectedTab;
            string currentTabTitle = currentPage.Text;

            if (string.IsNullOrEmpty(currentTabTitle))
            {
                MessageBox.Show("当前TabPage标题为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            // 从当前TabPage中查找DataGridView控件
            DataGridView dataGridView = FindDataGridViewInTabPage(currentPage);
            if (dataGridView == null)
            {
                MessageBox.Show($"在TabPage '{currentTabTitle}' 中未找到DataGridView控件", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            return dataGridView;
        }


        /// <summary>
        /// 异步保存单个实体对象到数据库
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entity">要保存的实体对象</param>
        /// <param name="owner">父窗体，用于显示进度窗口</param>
        /// <param name="action">保存完成后的回调动作</param>
        public static async void SaveAsync<T>(T entity, IWin32Window owner, bool isFeedback = true, Action action = null) where T : Entity, new()
        {
            // 显示进度窗口
            ProgressWindow progressWindow = new ProgressWindow();
            progressWindow.Show(owner);

            await Task.Run(() =>
            {
                // 执行保存操作
                int result = DaoTemplate.Save(entity);

                if (DaoTemplate.SqlException != null)
                {
                    var msg = DaoTemplate.SqlException.ToString() + "\r\n ErorrSql:" + DaoTemplate.SqlString;
                    MessageBox.Show($"保存失败！{msg}", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 更新进度
                progressWindow.UpdateProgress(100, "正在保存数据... 1/1");
            });

            // 关闭进度窗口
            progressWindow.Close();

            if(isFeedback)
                MessageBox.Show("保存成功！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            action?.Invoke();
        }

        public static async void SaveAsync<T>(List<T> data, IWin32Window owner, Action action = null) where T : Entity, new ()
        {
            // 显示进度窗口
            ProgressWindow progressWindow = new ProgressWindow();
            progressWindow.Show(owner);

            var total = data.Count;
            var count = 0;

            await Task.Run(() =>
            {
                foreach (var t in data)
                {
                    count += DaoTemplate.Save(t);

                    if (DaoTemplate.SqlException != null)
                    {
                        var msg = DaoTemplate.SqlException.ToString() + "\r\n ErorrSql:" + DaoTemplate.SqlString;
                        MessageBox.Show($"保存失败！{msg}", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 更新进度
                    int percentComplete = (int)(count / (float)total * 100);
                    progressWindow.UpdateProgress(percentComplete, $"正在保存数据... {count}/{total}");
                }
            });

            // 关闭进度窗口
            progressWindow.Close();

            MessageBox.Show($"保存成功，共{count}条数据！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            action?.Invoke();
        }

        public static async void DeleteAsync<T>(List<T> data, IWin32Window owner) where T : Entity, new ()
        {
            // 显示进度窗口
            ProgressWindow progressWindow = new ProgressWindow();
            progressWindow.Show(owner);

            var total = data.Count;
            var count = 0;

            await Task.Run(() =>
            {
                foreach (var t in data)
                {
                    count += DaoTemplate.Delete(t);

                    if (DaoTemplate.SqlException != null)
                    {
                        var msg = DaoTemplate.SqlException.ToString() + "\r\n ErorrSql:" + DaoTemplate.SqlString;
                        MessageBox.Show($"删除失败！{msg}", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 更新进度
                    int percentComplete = (int)(count / (float)total * 100);
                    progressWindow.UpdateProgress(percentComplete, $"正在删除数据... {count}/{total}");
                }
            });

            // 关闭进度窗口
            progressWindow.Close();

            MessageBox.Show($"删除成功，共{count}条数据！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static int ProgressWindow_PercentComplete = 0;

        /// <summary>
        /// 显示一个带进度条的窗口，当到达100%后自动关闭(暂停使用)
        /// </summary>
        /// <param name="owner">父窗体，用于显示进度窗口</param>
        /// <param name="message">进度提示消息，默认为"正在处理..."</param>
        public static async void ShowProgressWindowAsync(IWin32Window owner, string message = "正在处理...")
        {
            // 显示进度窗口
            ProgressWindow progressWindow = new ProgressWindow();
            progressWindow.Show(owner);

            try
            {
                // 异步执行进度更新
                await Task.Run(() =>
                {
                    while (true)
                    {
                        // 更新进度
                        int percentComplete = ProgressWindow_PercentComplete;
                        progressWindow.UpdateProgress(percentComplete, $"{message} {percentComplete}%");
                        if (100 <= percentComplete)
                            break;
                    }
                });
            }
            catch (Exception ex)
            {
                // 如果发生异常，记录日志但不中断流程
                MessageBox.Show($"进度窗口显示异常: {ex.Message}", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 确保进度窗口被关闭
                if (progressWindow != null && !progressWindow.IsDisposed)
                {
                    progressWindow.Close();
                    progressWindow.Dispose();
                }
            }
        }

        /// <summary>
        /// 显示一个自动进度的窗口，在指定时间内从0%到100%，然后自动关闭
        /// </summary>
        /// <param name="owner">父窗体，用于显示进度窗口</param>
        /// <param name="durationSeconds">进度完成的总时长（秒），默认为10秒</param>
        /// <param name="message">进度提示消息，默认为"正在处理..."</param>
        public static async void ShowAutoProgressWindowAsync(IWin32Window owner, int durationSeconds = 10, string message = "正在处理...")
        {
            // 显示进度窗口
            ProgressWindow progressWindow = new ProgressWindow();
            progressWindow.Show(owner);

            // 计算每次更新的间隔时间（毫秒）
            int totalSteps = 100;
            int intervalMs = (durationSeconds * 1000) / totalSteps;

            try
            {
                // 异步执行进度更新
                await Task.Run(async () =>
                {
                    for (int i = 1; i <= totalSteps; i++)
                    {
                        // 更新进度
                        int percentComplete = i;
                        progressWindow.UpdateProgress(percentComplete, $"{message} {percentComplete}%");

                        // 等待指定的时间间隔
                        await Task.Delay(intervalMs);
                    }
                });
            }
            catch (Exception ex)
            {
                // 如果发生异常，记录日志但不中断流程
                MessageBox.Show($"进度窗口显示异常: {ex.Message}", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 确保进度窗口被关闭
                if (progressWindow != null && !progressWindow.IsDisposed)
                {
                    progressWindow.Close();
                    progressWindow.Dispose();
                }
            }
        }

        /// <summary>
        /// 给数据添加上用户名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="records"></param>
        /// <param name="user"></param>
        public static void Signature<T>(List<T> records, User user) where T : Entity, new()
        {
            if (null == records || !records.Any())
                return;

            foreach (var record in records)
                record.FUser = user.FName;
        }

        /// <summary>
        /// 根据list创建DataGridView中的列
        /// </summary>
        /// <param name="dataGridView"></param>
        /// <param name="list">数据集，Dictionary中的key作为列名</param>
        public static void InitializeColumnsWithList(DataGridView dataGridView, List<Dictionary<string, object>> list)
        {
            // 清除现有的列（除了选择列和行号列）
            var columnsToRemove = dataGridView.Columns.Cast<DataGridViewColumn>()
                .Where(c => c.Name != "Selection" && c.Name != "RowNumber")
                .ToList();

            foreach (var column in columnsToRemove)
            {
                dataGridView.Columns.Remove(column);
            }

            if (list == null || !list.Any())
            {
                return;
            }

            // 获取第一个字典的键作为列名，假设所有字典具有相同的键结构
            var firstRecord = list.First();
            var columnNames = firstRecord.Keys.ToList();

            // 根据键创建列，根据值的实际类型选择对应的列类型
            foreach (var columnName in columnNames)
            {
                // 获取第一个非null值的实际类型，用于决定列类型
                Type valueType = null;
                foreach (var record in list)
                {
                    if (record.ContainsKey(columnName) && record[columnName] != null)
                    {
                        valueType = record[columnName].GetType();
                        break;
                    }
                }

                DataGridViewColumn column = CreateColumnByValueType(columnName, valueType);

                dataGridView.Columns.Add(column);
            }
        }

        /// <summary>
        /// 根据值的实际类型创建对应的DataGridViewColumn
        /// </summary>
        /// <param name="columnName">列名</param>
        /// <param name="valueType">值的实际类型，为null时使用默认文本列</param>
        /// <returns>对应类型的DataGridViewColumn</returns>
        private static DataGridViewColumn CreateColumnByValueType(string columnName, Type valueType)
        {
            DataGridViewColumn column;

            if (valueType == typeof(bool))
            {
                // 布尔类型使用复选框列
                DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
                checkBoxColumn.Name = columnName;
                checkBoxColumn.DataPropertyName = columnName;
                checkBoxColumn.HeaderText = columnName;
                checkBoxColumn.Width = 60;
                checkBoxColumn.ReadOnly = false;
                checkBoxColumn.Visible = true;
                checkBoxColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column = checkBoxColumn;
            }
            else if (valueType == typeof(DateTime) || valueType == typeof(DateTime?))
            {
                // 日期类型使用文本列，设置日期格式
                DataGridViewTextBoxColumn textColumn = new DataGridViewTextBoxColumn();
                textColumn.Name = columnName;
                textColumn.DataPropertyName = columnName;
                textColumn.HeaderText = columnName;
                textColumn.Width = 120;
                textColumn.ReadOnly = true;
                textColumn.Visible = true;
                textColumn.DefaultCellStyle.Format = "yyyy-MM-dd";
                textColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                textColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column = textColumn;
            }
            else if (valueType == typeof(int) || valueType == typeof(int?)
                || valueType == typeof(long) || valueType == typeof(long?)
                || valueType == typeof(short) || valueType == typeof(short?))
            {
                // 整数类型使用文本列，右对齐
                DataGridViewTextBoxColumn textColumn = new DataGridViewTextBoxColumn();
                textColumn.Name = columnName;
                textColumn.DataPropertyName = columnName;
                textColumn.HeaderText = columnName;
                textColumn.Width = 100;
                textColumn.ReadOnly = true;
                textColumn.Visible = true;
                textColumn.DefaultCellStyle.Format = "N0";
                textColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                textColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column = textColumn;
            }
            else if (valueType == typeof(decimal) || valueType == typeof(decimal?)
                || valueType == typeof(double) || valueType == typeof(double?)
                || valueType == typeof(float) || valueType == typeof(float?))
            {
                // 小数类型使用文本列，右对齐，保留2位小数
                DataGridViewTextBoxColumn textColumn = new DataGridViewTextBoxColumn();
                textColumn.Name = columnName;
                textColumn.DataPropertyName = columnName;
                textColumn.HeaderText = columnName;
                textColumn.Width = 120;
                textColumn.ReadOnly = true;
                textColumn.Visible = true;
                textColumn.DefaultCellStyle.Format = "N2";
                textColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                textColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column = textColumn;
            }
            else
            {
                // 默认使用文本列，居中对齐
                DataGridViewTextBoxColumn textColumn = new DataGridViewTextBoxColumn();
                textColumn.Name = columnName;
                textColumn.DataPropertyName = columnName;
                textColumn.HeaderText = columnName;
                textColumn.Width = 100;
                textColumn.ReadOnly = true;
                textColumn.Visible = true;
                textColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                textColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column = textColumn;
            }

            return column;
        }

        /// <summary>
        /// 根据ColumnAttribute属性的描述创建DataGridView中的列
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="dataGridView">DataGridView控件</param>
        public static void InitializeColumnsWithAttributes<T>(DataGridView dataGridView) where T : new()
        {
            Type entityType = typeof(T);
            var properties = entityType.GetProperties();

            // 清除现有的列（除了选择列和行号列）
            var columnsToRemove = dataGridView.Columns.Cast<DataGridViewColumn>()
                .Where(c => c.Name != "Selection" && c.Name != "RowNumber")
                .ToList();

            foreach (var column in columnsToRemove)
            {
                dataGridView.Columns.Remove(column);
            }

            // 根据属性上的ColumnAttribute创建列，并按照Index排序
            var columnInfoList = new List<Tuple<PropertyInfo, ColumnAttribute>>();

            foreach (var property in properties)
            {
                var columnAttribute = Attribute.GetCustomAttribute(property, typeof(ColumnAttribute)) as ColumnAttribute;

                if (columnAttribute != null)
                {
                    columnInfoList.Add(new Tuple<PropertyInfo, ColumnAttribute>(property, columnAttribute));
                }
            }

            // 按照Index属性排序，Index值越小位置越靠前
            var sortedColumnInfoList = columnInfoList.OrderBy(tuple => tuple.Item2.Index).ToList();

            // 根据排序后的信息创建列
            foreach (var tuple in sortedColumnInfoList)
            {
                var property = tuple.Item1;
                var columnAttribute = tuple.Item2;

                DataGridViewColumn column;

                // 根据属性类型确定列类型
                if (property.PropertyType == typeof(bool))
                {
                    column = new DataGridViewCheckBoxColumn();
                }
                else
                {
                    column = new DataGridViewTextBoxColumn();
                }

                // 设置列属性
                column.Name = property.Name;
                column.DataPropertyName = property.Name;
                column.HeaderText = string.IsNullOrEmpty(columnAttribute.Title) ? Utility.GetAttributeValueByField<T>("Field", "Name", property.Name) as string : columnAttribute.Title;
                column.Width = columnAttribute.Width;
                column.ReadOnly = columnAttribute.ReadOnly;
                column.Visible = columnAttribute.Visible;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

                // 添加FillWeight属性（注意：只有在特定的AutoSize模式下才有效）
                if (column is DataGridViewTextBoxColumn textBoxColumn)
                {
                    // 注意：FillWeight是float类型，且只在列的AutoSizeMode为Fill时有效
                    column.FillWeight = columnAttribute.Width * 0.8f;
                }

                dataGridView.Columns.Add(column);
            }

            // 添加列头点击事件处理程序以实现排序功能
            dataGridView.ColumnHeaderMouseClick += (sender, e) =>
            {
                DataGridView dgv = sender as DataGridView;
                if (dgv == null || dgv.DataSource == null) return;

                // 获取当前数据源
                var dataSource = dgv.DataSource as List<T>;
                if (dataSource == null) return;

                // 获取被点击的列
                DataGridViewColumn clickedColumn = dgv.Columns[e.ColumnIndex];
                if (clickedColumn == null) return;

                // 获取要排序的属性
                PropertyInfo sortProperty = typeof(T).GetProperty(clickedColumn.Name);
                if (sortProperty == null) return;

                // 检查当前排序状态，实现升序/降序切换
                ListSortDirection direction = ListSortDirection.Ascending;
                if (dgv.Tag != null && dgv.Tag.ToString().Split('_')[0] == clickedColumn.Name)
                {
                    // 如果再次点击同一列，则切换排序方向
                    direction = dgv.Tag.ToString().EndsWith("DESC") ? ListSortDirection.Ascending : ListSortDirection.Descending;
                }
                else
                {
                    // 第一次点击该列，默认为升序
                    direction = ListSortDirection.Ascending;
                }

                // 执行排序
                if (direction == ListSortDirection.Ascending)
                {
                    dataSource = dataSource.OrderBy(item => sortProperty.GetValue(item)).ToList();
                    dgv.Tag = clickedColumn.Name + "_ASC";
                }
                else
                {
                    dataSource = dataSource.OrderByDescending(item => sortProperty.GetValue(item)).ToList();
                    dgv.Tag = clickedColumn.Name + "_DESC";
                }

                // 更新DataGridView的数据源
                dgv.DataSource = null;
                dgv.DataSource = dataSource;
            };
        }

        /// <summary>
        /// 与InitializeColumnsWithAttributes作用相同，但用于dataGridView2
        /// 它与dataGridView1的区别在于，所有column = new DataGridViewTextBoxColumn()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataGridView"></param>
        public static void InitializeColumnsWithAttributes2<T>(DataGridView dataGridView) where T : new()
        {
            Type entityType = typeof(T);
            var properties = entityType.GetProperties();

            // 清除现有的列（除了选择列和行号列）
            var columnsToRemove = dataGridView.Columns.Cast<DataGridViewColumn>()
                .Where(c => c.Name != "Selection" && c.Name != "RowNumber")
                .ToList();

            foreach (var column in columnsToRemove)
            {
                dataGridView.Columns.Remove(column);
            }

            // 根据属性上的ColumnAttribute创建列，并按照Index排序
            var columnInfoList = new List<Tuple<PropertyInfo, ColumnAttribute>>();

            foreach (var property in properties)
            {
                var columnAttribute = Attribute.GetCustomAttribute(property, typeof(ColumnAttribute)) as ColumnAttribute;

                if (columnAttribute != null)
                {
                    columnInfoList.Add(new Tuple<PropertyInfo, ColumnAttribute>(property, columnAttribute));
                }
            }

            // 按照Index属性排序，Index值越小位置越靠前
            var sortedColumnInfoList = columnInfoList.OrderBy(tuple => tuple.Item2.Index).ToList();

            // 根据排序后的信息创建列
            foreach (var tuple in sortedColumnInfoList)
            {
                var property = tuple.Item1;
                var columnAttribute = tuple.Item2;

                DataGridViewColumn column;

                // 根据属性类型确定列类型
                column = new DataGridViewTextBoxColumn();

                // 设置列属性
                column.Name = property.Name;
                column.DataPropertyName = property.Name;
                column.HeaderText = string.IsNullOrEmpty(columnAttribute.Title) ? Utility.GetAttributeValueByField<T>("Field", "Name", property.Name) as string : columnAttribute.Title;
                column.Width = columnAttribute.Width;
                column.ReadOnly = columnAttribute.ReadOnly;
                column.Visible = columnAttribute.Visible;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

                // 添加FillWeight属性（注意：只有在特定的AutoSize模式下才有效）
                if (column is DataGridViewTextBoxColumn textBoxColumn)
                {
                    // 注意：FillWeight是float类型，且只在列的AutoSizeMode为Fill时有效
                    column.FillWeight = columnAttribute.Width * 0.8f;
                }

                dataGridView.Columns.Add(column);
            }

            // 添加列头点击事件处理程序以实现排序功能
            dataGridView.ColumnHeaderMouseClick += (sender, e) =>
            {
                DataGridView dgv = sender as DataGridView;
                if (dgv == null || dgv.DataSource == null) return;

                // 获取当前数据源
                var dataSource = dgv.DataSource as List<T>;
                if (dataSource == null) return;

                // 获取被点击的列
                DataGridViewColumn clickedColumn = dgv.Columns[e.ColumnIndex];
                if (clickedColumn == null) return;

                // 获取要排序的属性
                PropertyInfo sortProperty = typeof(T).GetProperty(clickedColumn.Name);
                if (sortProperty == null) return;

                // 检查当前排序状态，实现升序/降序切换
                ListSortDirection direction = ListSortDirection.Ascending;
                if (dgv.Tag != null && dgv.Tag.ToString().Split('_')[0] == clickedColumn.Name)
                {
                    // 如果再次点击同一列，则切换排序方向
                    direction = dgv.Tag.ToString().EndsWith("DESC") ? ListSortDirection.Ascending : ListSortDirection.Descending;
                }
                else
                {
                    // 第一次点击该列，默认为升序
                    direction = ListSortDirection.Ascending;
                }

                // 执行排序
                if (direction == ListSortDirection.Ascending)
                {
                    dataSource = dataSource.OrderBy(item => sortProperty.GetValue(item)).ToList();
                    dgv.Tag = clickedColumn.Name + "_ASC";
                }
                else
                {
                    dataSource = dataSource.OrderByDescending(item => sortProperty.GetValue(item)).ToList();
                    dgv.Tag = clickedColumn.Name + "_DESC";
                }

                // 更新DataGridView的数据源
                dgv.DataSource = null;
                dgv.DataSource = dataSource;
            };
        }

        public static List<T> LoadData<T>(string sql) where T :Entity, new()
        {
            List<T> selectedBills = DaoTemplate.FindAll<T>(sql);
            // 按 selectedBills 顺序写入 FStatus 字段
            //for (int i = 0; i < selectedBills.Count; i++)
            //{
            //    selectedBills[i].FStatus = i + 1;
            //}
            return selectedBills;
        }

        public static List<Dictionary<string, object>> LoadData(string sql)
        {
            var selectedBills = DaoTemplate.FindAll(sql);
            return selectedBills;
        }

        /// <summary>
        /// 上移列表中指定元素的顺序（减小 FStatus）
        /// </summary>
        /// <param name="list">需要排序的列表</param>
        /// <param name="entity">需要移动的元素</param>
        /// <returns>排序后的列表</returns>
        public static List<T> MoveUp<T>(List<T> list, T entity) where T : Entity, new()
        {
            if (list == null || list.Count <= 1 || entity == null)
            {
                return list;
            }

            int index = list.IndexOf(entity);
            if (index <= 0)
            {
                return list;
            }

            Entity prevEntity = list[index - 1];
            int tempOrder = entity.FStatus;
            entity.FStatus = prevEntity.FStatus;
            prevEntity.FStatus = tempOrder;

            list.Sort((x, y) => x.FStatus.CompareTo(y.FStatus));

            return list;
        }

        /// <summary>
        /// 下移列表中指定元素的顺序（增大 FStatus）
        /// </summary>
        /// <param name="list">需要排序的列表</param>
        /// <param name="entity">需要移动的元素</param>
        /// <returns>排序后的列表</returns>
        public static List<T> MoveDown<T>(List<T> list, T entity) where T : Entity, new()
        {
            if (list == null || list.Count <= 1 || entity == null)
            {
                return list;
            }

            int index = list.IndexOf(entity);
            if (index < 0 || index >= list.Count - 1)
            {
                return list;
            }

            Entity nextEntity = list[index + 1];
            int tempOrder = entity.FStatus;
            entity.FStatus = nextEntity.FStatus;
            nextEntity.FStatus = tempOrder;

            list.Sort((x, y) => x.FStatus.CompareTo(y.FStatus));

            return list;
        }

        public static void ClearDataSourceForGridView<T>(DataGridView dataGridView, List<T> selectedBills, List<T> filteredBills) where T : Entity
        {
            selectedBills.Clear();
            filteredBills.Clear();
            dataGridView.DataSource = null;
        }

        public static void SetDataSourceForGridView<T>(DataGridView dataGridView, List<T> data) where T : new()
        {
            dataGridView.AutoGenerateColumns = false;

            dataGridView.DataSource = null;
            dataGridView.DataSource = data;

            dataGridView.Refresh();
        }

        public static void SetDataSourceForGridView(DataGridView dataGridView, IList data)
        {
            dataGridView.AutoGenerateColumns = false;

            dataGridView.DataSource = null;
            dataGridView.DataSource = data;

            dataGridView.Refresh();
        }

        /// <summary>
        /// 专门为Dictionary<string, object>类型的列表设置数据源
        /// 将Dictionary列表转换为DataTable以便DataGridView能够正确显示
        /// </summary>
        /// <param name="dataGridView">目标DataGridView控件</param>
        /// <param name="data">Dictionary类型的数据列表</param>
        public static void SetDataSourceForGridView(DataGridView dataGridView, List<Dictionary<string, object>> data)
        {
            if (dataGridView == null)
                return;

            dataGridView.AutoGenerateColumns = false;

            // 清除现有数据源
            dataGridView.DataSource = null;

            if (data == null || data.Count == 0)
            {
                dataGridView.Refresh();
                return;
            }

            // 将Dictionary列表转换为DataTable
            DataTable dataTable = ConvertDictionaryListToDataTable(data);

            // 绑定DataTable到DataGridView
            dataGridView.DataSource = dataTable;

            dataGridView.Refresh();
        }

        /// <summary>
        /// 将Dictionary列表转换为DataTable
        /// </summary>
        /// <param name="dictList">Dictionary列表</param>
        /// <returns>转换后的DataTable</returns>
        private static DataTable ConvertDictionaryListToDataTable(List<Dictionary<string, object>> dictList)
        {
            DataTable table = new DataTable();

            if (dictList == null || dictList.Count == 0)
                return table;

            // 获取所有列名（从第一个Dictionary的Key）
            var firstDict = dictList.First();
            var columnNames = firstDict.Keys.ToList();

            // 创建列
            foreach (var columnName in columnNames)
            {
                // 根据实际数据类型设置列类型
                Type columnType = typeof(string); // 默认为string

                // 尝试推断数据类型
                foreach (var dict in dictList)
                {
                    if (dict.ContainsKey(columnName) && dict[columnName] != null)
                    {
                        columnType = dict[columnName].GetType();
                        break;
                    }
                }

                table.Columns.Add(new DataColumn(columnName, columnType));
            }

            // 填充行数据
            foreach (var dict in dictList)
            {
                DataRow row = table.NewRow();
                foreach (var columnName in columnNames)
                {
                    if (dict.ContainsKey(columnName))
                    {
                        row[columnName] = dict[columnName] ?? DBNull.Value;
                    }
                    else
                    {
                        row[columnName] = DBNull.Value;
                    }
                }
                table.Rows.Add(row);
            }

            return table;
        }

        public static void SetDataSourceForGridView<T>(DataGridView dataGridView, BindingList<T> data) where T : new()
        {
            dataGridView.AutoGenerateColumns = false;

            dataGridView.DataSource = null;
            dataGridView.DataSource = data;

            dataGridView.Refresh();
        }

        public static void RefreshDataGridViewBackColor(DataGridView dataGridView)
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                var entity = GetEntityByRow<Entity>(row);
                row.DefaultCellStyle.BackColor = entity.RowBackColor;
            }
        }

        /// <summary>
        /// 根据提供的行号集，将对应行背景改为红色
        /// </summary>
        /// <param name="rowNumbers">行号的集合</param>
        /// <param name="dataGridView">目标DataGridView控件</param>
        public static void HighlightRowsWithRedBackground(List<ScheduleRecord> scheduleRecords, DataGridView dataGridView)
        {
            List<int> rowNumbers = GetAllRowNumbers(scheduleRecords, dataGridView);

            // 检查参数有效性
            if (rowNumbers == null || dataGridView == null)
            {
                throw new ArgumentNullException("行号集合或DataGridView控件不能为空");
            }

            // 遍历所有行号
            foreach (int rowNumber in rowNumbers)
            {
                // 行号从1开始，而索引从0开始，所以需要减1
                int rowIndex = rowNumber - 1;

                // 检查行索引是否有效
                if (rowIndex >= 0 && rowIndex < dataGridView.Rows.Count)
                {
                    // 设置行背景色为红色
                    dataGridView.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Red;
                }
            }
        }

        /// <summary>
        /// 删除选中行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataGridView"></param>
        /// <returns>返回删除数据</returns>
        public static List<T> DeleteSelectedRows<T>(DataGridView dataGridView) where T : Entity, new()
        {
            List<T> selectedBills = dataGridView.DataSource as List<T>;
            List<T> deleteList = new List<T>();
            var copyList = Utility.CopyList(selectedBills);

            List<T> selectedRows = GetSelectedRows<T>(dataGridView);

            if (selectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要删除的行！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return deleteList;
            }

            var result = MessageBox.Show($"确定要删除选中的{selectedRows.Count}条数据吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                foreach(var item in selectedRows)
                {
                    copyList.Remove(item);
                    deleteList.Add(item);
                }

                //foreach (DataGridViewRow row in dataGridView.SelectedRows)
                //{
                //    var item = row.DataBoundItem as T;
                //    if (item != null)
                //    {
                //        copyList.Remove(item);
                //        deleteList.Add(item);
                //    }
                //}

                // 更新DataGridView显示
                //MessageBox.Show("删除成功！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);

                SetDataSourceForGridView(dataGridView, copyList);
                return deleteList;
            }

            return deleteList;
        }

        /// <summary>
        /// 删除选中行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataGridView"></param>
        /// <param name="records"></param>
        /// <param name="owner"></param>
        /// <returns>返回剩余数据</returns>
        public static List<T> DeleteSelectedRows<T>(DataGridView dataGridView, List<T> records, IWin32Window owner) where T : Entity, new()
        {
            List<T> selectedBills = dataGridView.DataSource as List<T>;
            List<T> deleteList = new List<T>();
            var copyList = Utility.CopyList(selectedBills);

            List<T> selectedRows = GetSelectedRows<T>(dataGridView);

            if (selectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要删除的行！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return deleteList;
            }

            var result = MessageBox.Show($"确定要删除选中的{selectedRows.Count}条数据吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                foreach (var item in selectedRows)
                {
                    copyList.Remove(item);
                    deleteList.Add(item);
                }

                if (deleteList.Any())
                {
                    records.RemoveAll(t => deleteList.Contains(t));

                    DeleteAsync(deleteList, owner);
                }

                return copyList;
            }

            return copyList;
        }

        /// <summary>
        /// 初始化DataGridView，带分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataGridView">主表格</param>
        /// <param name="form"></param>
        public static void InitializationDataGridView<T>(DataGridView dataGridView, IPagination<T> form) where T : Entity, new()
        {
            AddRowNumberColumn(dataGridView, form);
            EnableMultiSelectWithCheckbox(dataGridView);
            InitializeColumnsWithAttributes<T>(dataGridView);
        }

        /// <summary>
        /// 初始化DataGridView，带分页
        /// </summary>
        /// <param name="dataGridView"></param>
        /// <param name="form"></param>
        /// <param name="list"></param>
        public static void InitializationDataGridView(DataGridView dataGridView, IPagination form, List<Dictionary<string, object>> list)
        {
            AddRowNumberColumn(dataGridView, form);
            EnableMultiSelectWithCheckbox(dataGridView);
            InitializeColumnsWithList(dataGridView, list);
        }

        /// <summary>
        /// 初始化DataGridView，不带分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataGridView">主表格</param>
        /// <param name="form"></param>
        public static void InitializationDataGridView<T>(DataGridView dataGridView) where T : Entity, new()
        {
            AddRowNumberColumn(dataGridView);
            EnableMultiSelectWithCheckbox(dataGridView);
            InitializeColumnsWithAttributes<T>(dataGridView);
        }

        /// <summary>
        /// 初始化DataGridView，不带分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataGridView">用于筛选功能的表格</param>
        public static void InitializationDataGridView2<T>(DataGridView dataGridView) where T : Entity, new()
        {
            AddRowNumberColumn(dataGridView);
            EnableMultiSelectWithCheckbox(dataGridView);
            InitializeColumnsWithAttributes2<T>(dataGridView);
        }

        /// <summary>
        /// 初始化DataGridView，不带分页
        /// </summary>
        /// <param name="dataGridView">用于筛选功能的表格</param>
        /// <param name="list"></param>
        public static void InitializationDataGridView(DataGridView dataGridView, List<Dictionary<string, object>> list)
        {
            AddRowNumberColumn(dataGridView);
            EnableMultiSelectWithCheckbox(dataGridView);
            InitializeColumnsWithList(dataGridView, list);
        }

        /// <summary>
        /// 为DataGridView添加多选功能，支持复选框选择行并改变背景色
        /// </summary>
        /// <param name="dataGridView">目标DataGridView控件</param>
        /// <param name="selectionColumnName">复选框列的名称，默认为"Selection"</param>
        public static void EnableMultiSelectWithCheckbox(DataGridView dataGridView, string selectionColumnName = "Selection")
        {
            // 确保DataGridView已初始化
            if (dataGridView == null)
            {
                throw new ArgumentNullException(nameof(dataGridView), "DataGridView不能为null");
            }

            // 添加复选框列
            AddCheckboxColumn(dataGridView, selectionColumnName);

            // 设置DataGridView的多选模式
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView.MultiSelect = true;

            // 设置行高以适应复选框
            dataGridView.RowTemplate.Height = 25;

            // 事件处理：当单元格值改变时更新行状态
            dataGridView.CellValueChanged += (sender, e) =>
            {
                if (e.ColumnIndex == dataGridView.Columns[selectionColumnName].Index)
                {
                    UpdateRowSelectionState(dataGridView, e.RowIndex);
                }
            };

            // 事件处理：当行被选中或取消选中时更新复选框状态
            //dataGridView.SelectionChanged += (sender, e) =>
            //{
            //    UpdateSelectionCheckboxes(dataGridView);
            //};
        }

        /// <summary>
        /// 添加复选框列到DataGridView
        /// </summary>
        /// <param name="dataGridView">目标DataGridView控件</param>
        /// <param name="columnName">列名称</param>
        private static void AddCheckboxColumn(DataGridView dataGridView, string columnName)
        {
            // 检查是否已存在该列
            if (dataGridView.Columns.Contains(columnName))
            {
                return;
            }

            // 创建复选框列
            DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
            checkBoxColumn.Name = columnName;
            checkBoxColumn.HeaderText = "选择";
            checkBoxColumn.Width = 40;
            checkBoxColumn.TrueValue = true;
            checkBoxColumn.FalseValue = false;
            checkBoxColumn.ReadOnly = false;
            checkBoxColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

            // 添加到DataGridView
            dataGridView.Columns.Insert(0, checkBoxColumn);
        }

        /// <summary>
        /// 更新指定行的选择状态，包括背景色和复选框
        /// </summary>
        /// <param name="dataGridView">目标DataGridView控件</param>
        /// <param name="rowIndex">行索引</param>
        public static void UpdateRowSelectionState(DataGridView dataGridView, int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dataGridView.Rows.Count)
            {
                return;
            }

            DataGridViewRow row = dataGridView.Rows[rowIndex];
            
            // 获取复选框列的值
            bool isSelected = Convert.ToBoolean(row.Cells["Selection"].Value ?? false);
            
            // 设置行背景色
            if (isSelected)
            {
                row.DefaultCellStyle.BackColor = Color.LightCyan;
                //row.DefaultCellStyle.ForeColor = Color.Black;
            }
            else
            {
                var entity = GetEntityByRow<Entity>(row);
                row.DefaultCellStyle.BackColor = null != entity ? entity.RowBackColor : row.DefaultCellStyle.BackColor;
                //row.DefaultCellStyle.ForeColor = Color.Black;
            }
        }

        /// <summary>
        /// 设置筛选DataGridView
        /// </summary>
        public static void SetupFilterDataGridView<T>(DataGridView dataGridView) where T : Entity, new()
        {
            // 配置dataGridView2作为筛选器
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.ReadOnly = false;
            dataGridView.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dataGridView.MultiSelect = false;

            // 创建一个空的T对象用于筛选
            T filterRecord = new T();

            // 将filterRecord转换为Dictionary<string, string>，Key为T属性名（需带有[Column]特性），Value默认为空字符串
            var filterDict = new Dictionary<string, string>();
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                // 检查属性是否带有Column特性
                var columnAttribute = property.GetCustomAttributes(typeof(ColumnAttribute), false);
                if (columnAttribute != null && columnAttribute.Length > 0)
                {
                    // Key为属性名，Value默认为空字符串
                    filterDict[property.Name] = string.Empty;

                    // 尝试将Dictionary中的值设置到filterRecord对应的属性中（如果需要双向绑定）
                    // 此处仅根据指令生成Dictionary，若需同步回对象可后续处理
                }
            }

            // 使用BindingList支持双向数据绑定
            var filterList = new BindingList<Dictionary<string, string>> { filterDict };

            // 将筛选记录绑定到dataGridView2
            SetDataSourceForGridView(dataGridView, filterList);

            // 确保所有列都是可编辑的
            foreach (DataGridViewColumn column in dataGridView.Columns)
            {
                // 当列名为"Selection"或"RowNumber"时设置为只读，其他列设置为可编辑
                if (column.Name == "Selection" || column.Name == "RowNumber")
                {
                    column.ReadOnly = true;
                }
                else
                {
                    column.ReadOnly = false;
                }
            }
        }

        /// <summary>
        /// 设置筛选DataGridView
        /// </summary>
        /// <param name="dataGridView"></param>
        /// <param name="list">数据集，Dictionary中的key作为列名</param>
        public static void SetupFilterDataGridView(DataGridView dataGridView, List<Dictionary<string, object>> list)
        {
            // 配置dataGridView作为筛选器
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.ReadOnly = false;
            dataGridView.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dataGridView.MultiSelect = false;

            // 根据list中的第一个字典的Key创建列，Value默认为空字符串用于输入筛选条件
            if (list == null || !list.Any())
            {
                return;
            }

            // 获取第一个字典的键作为列名
            var firstRecord = list.First();
            var columnNames = firstRecord.Keys.ToList();

            // 清除现有的列（除了选择列和行号列）
            var columnsToRemove = dataGridView.Columns.Cast<DataGridViewColumn>()
                .Where(c => c.Name != "Selection" && c.Name != "RowNumber")
                .ToList();

            foreach (var column in columnsToRemove)
            {
                dataGridView.Columns.Remove(column);
            }

            // 创建用于绑定的列表，包含一个空的字典用于输入筛选条件
            var filterDict = new Dictionary<string, object>();
            foreach (var columnName in columnNames)
            {
                filterDict[columnName] = string.Empty;
            }

            var filterList = new BindingList<Dictionary<string, object>> { filterDict };

            // 初始化列
            InitializeColumnsWithList(dataGridView, list);

            // 将筛选记录绑定到dataGridView
            SetDataSourceForGridView(dataGridView, filterList);

            // 确保所有业务列都是可编辑的，系统列只读
            foreach (DataGridViewColumn column in dataGridView.Columns)
            {
                if (column.Name == "Selection" || column.Name == "RowNumber")
                {
                    column.ReadOnly = true;
                }
                else
                {
                    column.ReadOnly = false;
                }
            }
        }

        /// <summary>
        /// 应用筛选条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataGridView1"></param>
        /// <param name="dataGridView2"></param>
        /// <param name="selectedRecords"></param>
        /// <returns></returns>
        public static List<T> ApplyFilter<T>(DataGridView dataGridView1, DataGridView dataGridView2, List<T> selectedRecords) where T : Entity, new()
        {
            List<T> filteredRecords = new List<T>();

            // 确保编辑已提交
            dataGridView2.EndEdit();

            // 直接从DataGridView读取筛选条件
            Dictionary<string, FilterCondition> filterConditions = new Dictionary<string, FilterCondition>();

            if (dataGridView2.Rows.Count > 0)
            {
                DataGridViewRow filterRow = dataGridView2.Rows[0];

                foreach (DataGridViewColumn column in dataGridView2.Columns)
                {
                    // 跳过选择列和行号列
                    if (column.Name == "Selection" || column.Name == "RowNumber")
                        continue;

                    var cellValue = filterRow.Cells[column.Name].Value;
                    if (cellValue != null && !string.IsNullOrWhiteSpace(cellValue.ToString()))
                    {
                        string filterValue = cellValue.ToString().Trim();

                        // 解析筛选条件
                        FilterCondition condition = Utility.ParseFilterCondition(filterValue);
                        if (condition != null)
                        {
                            filterConditions[column.DataPropertyName] = condition;
                        }
                    }
                }
            }

            // 如果没有筛选条件，显示所有数据
            if (!filterConditions.Any())
            {
                filteredRecords = new List<T>(selectedRecords);
                return filteredRecords;
            }

            // 应用筛选条件
            filteredRecords = selectedRecords.Where(record =>
            {
                foreach (var condition in filterConditions)
                {
                    // 获取属性值
                    var property = typeof(T).GetProperty(condition.Key);
                    if (property == null)
                        continue;

                    var value = property.GetValue(record);
                    if (value == null)
                        return false;

                    // 根据运算符类型进行匹配
                    if (!Utility.MatchCondition(value, condition.Value))
                        return false;
                }
                return true;
            }).ToList();

            return filteredRecords;
        }

        /// <summary>
        /// 应用筛选条件
        /// </summary>
        /// <param name="dataGridView1"></param>
        /// <param name="dataGridView2"></param>
        /// <param name="selectedRecords">数据集，Dictionary中的key作为列名</param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> ApplyFilter(DataGridView dataGridView1, DataGridView dataGridView2, List<Dictionary<string, object>> selectedRecords)
        {
            List<Dictionary<string, object>> filteredRecords = new List<Dictionary<string, object>>();

            // 确保编辑已提交
            dataGridView2.EndEdit();

            // 直接从DataGridView读取筛选条件
            Dictionary<string, FilterCondition> filterConditions = new Dictionary<string, FilterCondition>();

            if (dataGridView2.Rows.Count > 0)
            {
                DataGridViewRow filterRow = dataGridView2.Rows[0];

                foreach (DataGridViewColumn column in dataGridView2.Columns)
                {
                    // 跳过选择列和行号列
                    if (column.Name == "Selection" || column.Name == "RowNumber")
                        continue;

                    var cellValue = filterRow.Cells[column.Name].Value;
                    if (cellValue != null && !string.IsNullOrWhiteSpace(cellValue.ToString()))
                    {
                        string filterValue = cellValue.ToString().Trim();

                        // 解析筛选条件
                        FilterCondition condition = Utility.ParseFilterCondition(filterValue);
                        if (condition != null)
                        {
                            filterConditions[column.DataPropertyName] = condition;
                        }
                    }
                }
            }

            // 如果没有筛选条件，显示所有数据
            if (!filterConditions.Any())
            {
                filteredRecords = new List<Dictionary<string, object>>(selectedRecords);
                return filteredRecords;
            }

            // 应用筛选条件
            filteredRecords = selectedRecords.Where(record =>
            {
                foreach (var condition in filterConditions)
                {
                    // 获取字典中的值
                    if (!record.ContainsKey(condition.Key))
                        return false;

                    var value = record[condition.Key];
                    if (value == null)
                        return false;

                    // 根据运算符类型进行匹配
                    if (!Utility.MatchCondition(value, condition.Value))
                        return false;
                }
                return true;
            }).ToList();

            return filteredRecords;
        }

        /// <summary>
        /// 更新所有复选框的状态，使其与当前选中的行保持一致
        /// </summary>
        /// <param name="dataGridView">目标DataGridView控件</param>
            //private static void UpdateSelectionCheckboxes(DataGridView dataGridView)
            //{
            //    foreach (DataGridViewRow row in dataGridView.Rows)
            //    {
            //        // 检查复选框是否被选中
            //        if (Convert.ToBoolean(row.Cells["Selection"].Value ?? false))
            //        {
            //            row.DefaultCellStyle.BackColor = Color.LightCyan;
            //            //row.DefaultCellStyle.ForeColor = Color.Black;
            //        }
            //        else
            //        {
            //            row.DefaultCellStyle.BackColor = GetEntityByRow<Entity>(row).RowBackColor;
            //            //row.DefaultCellStyle.ForeColor = Color.Black;
            //        }
            //    }

            //    //foreach (DataGridViewRow row in dataGridView.Rows)
            //    //{
            //    //    // 如果行被选中，则设置复选框为选中状态
            //    //    if (row.Selected)
            //    //    {
            //    //        row.Cells["Selection"].Value = true;
            //    //        row.DefaultCellStyle.BackColor = Color.Blue;
            //    //        row.DefaultCellStyle.ForeColor = Color.White;
            //    //    }
            //    //    else
            //    //    {
            //    //        row.Cells["Selection"].Value = false;
            //    //        row.DefaultCellStyle.BackColor = Color.Empty;
            //    //        row.DefaultCellStyle.ForeColor = Color.Black;
            //    //    }
            //    //}
            //}

            /// <summary>
            /// 获取所有被选中的行数据
            /// </summary>
            /// <typeparam name="T">数据类型</typeparam>
            /// <param name="dataGridView">目标DataGridView控件</param>
            /// <returns>被选中的行数据列表</returns>
        public static List<T> GetSelectedRows<T>(DataGridView dataGridView) where T : Entity, new()
        {
            List<T> selectedData = new List<T>();
            
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                // 检查复选框是否被选中
                if (Convert.ToBoolean(row.Cells["Selection"].Value ?? false))
                {
                    var dataItem = row.DataBoundItem as T;
                    if (dataItem != null)
                    {
                        selectedData.Add(dataItem);
                    }
                }
            }
            
            return selectedData;
        }

        /// <summary>
        /// 获取所有被选中的行数据
        /// </summary>
        /// <param name="dataGridView">目标DataGridView控件</param>
        /// <returns>被选中的行数据列表，每行数据以Dictionary形式存储，Key为列名，Value为单元格值</returns>
        public static List<Dictionary<string, object>> GetSelectedRows(DataGridView dataGridView)
        {
            List<Dictionary<string, object>> selectedData = new List<Dictionary<string, object>>();
            
            if (dataGridView == null || dataGridView.Rows.Count == 0)
                return selectedData;
            
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                // 检查复选框是否被选中
                if (Convert.ToBoolean(row.Cells["Selection"].Value ?? false))
                {
                    // 创建字典存储当前行的数据
                    var rowData = new Dictionary<string, object>();
                    
                    // 遍历所有列（除了Selection和RowNumber列）
                    foreach (DataGridViewColumn column in dataGridView.Columns)
                    {
                        // 跳过选择列和行号列
                        if (column.Name == "Selection" || column.Name == "RowNumber")
                            continue;
                        
                        // 获取单元格值
                        object cellValue = row.Cells[column.Name].Value;
                        
                        // 将列名作为Key，单元格值作为Value添加到字典中
                        // 使用DataPropertyName作为Key，如果DataPropertyName为空则使用Name
                        string key = !string.IsNullOrEmpty(column.DataPropertyName) ? column.DataPropertyName : column.Name;
                        rowData[key] = cellValue;
                    }
                    
                    // 将当前行数据添加到结果列表中
                    selectedData.Add(rowData);
                }
            }
            
            return selectedData;
        }

        /// <summary>
        /// 获取所有被选中的行数据,这里的选中是指dataGridView默认操作行为
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="dataGridView">目标DataGridView控件</param>
        /// <returns>被选中的行数据列表</returns>
        public static List<T> GetSelectedRows2<T>(DataGridView dataGridView) where T : Entity, new()
        {
            List<T> selectedData = new List<T>();

            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                var dataItem = row.DataBoundItem as T;
                if (dataItem != null)
                {
                    selectedData.Add(dataItem);
                }
            }

            return selectedData;
        }

        /// <summary>
        /// 根据提供的数据集获取所有行号
        /// </summary>
        /// <param name="data">数据集，Entity.FID是主键</param>
        /// <param name="dataGridView">DataGridView控件</param>
        /// <returns>行号的集合</returns>
        public static List<int> GetAllRowNumbers<T>(List<T> data, DataGridView dataGridView) where T : Entity, new()
        {
            List<int> rowNumbers = new List<int>();
            
            if (data == null || dataGridView == null || dataGridView.Rows.Count == 0)
            {
                return rowNumbers;
            }

            foreach (var entity in data)
            {
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    var rowData = row.DataBoundItem as Entity;
                    if (rowData != null && rowData.FID == entity.FID)
                    {
                        rowNumbers.Add(row.Index + 1); // 行号从1开始
                        break;
                    }
                }
            }

            return rowNumbers;
        }

        public static T GetEntityByRow<T>(DataGridViewRow row) where T : Entity, new()
        {
            var dataItem = row.DataBoundItem as T;
            return dataItem;
        }

        /// <summary>
        /// 反选所有行
        /// </summary>
        /// <param name="dataGridView">目标DataGridView控件</param>
        public static void ToggleAllSelection(DataGridView dataGridView)
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                // 切换复选框状态
                bool currentSelection = Convert.ToBoolean(row.Cells["Selection"].Value ?? false);
                row.Cells["Selection"].Value = !currentSelection;
                
                // 更新行背景色
                UpdateRowSelectionState(dataGridView, row.Index);
            }
        }

        /// <summary>
        /// 为DataGridView添加行号列，用于显示当前行数，带分页
        /// </summary>
        /// <param name="dataGridView">目标DataGridView控件</param>
        /// <param name="rowNumberColumnName">行号列名称，默认为"RowNumber"</param>
        public static void AddRowNumberColumn<T>(DataGridView dataGridView, IPagination<T> form, string rowNumberColumnName = "RowNumber") where T : Entity, new()
        {
            // 检查参数有效性
            if (dataGridView == null)
            {
                throw new ArgumentNullException(nameof(dataGridView), "DataGridView不能为null");
            }

            // 如果已存在行号列则先移除
            if (dataGridView.Columns.Contains(rowNumberColumnName))
            {
                dataGridView.Columns.Remove(rowNumberColumnName);
            }

            // 创建行号列
            DataGridViewTextBoxColumn rowNumberColumn = new DataGridViewTextBoxColumn();
            rowNumberColumn.Name = rowNumberColumnName;
            rowNumberColumn.HeaderText = "行号";
            rowNumberColumn.Width = 50;
            rowNumberColumn.ReadOnly = true;
            rowNumberColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

            // 将行号列插入到第一列位置
            dataGridView.Columns.Insert(0, rowNumberColumn);

            // 更新现有行的行号
            UpdateRowNumbers(dataGridView, rowNumberColumnName, form.GetPageSize(), form.GetCurrentPage());

            // 监听行状态变化事件，动态更新行号
            dataGridView.RowsAdded += (sender, e) => UpdateRowNumbers(dataGridView, rowNumberColumnName, form.GetPageSize(), form.GetCurrentPage());
            dataGridView.RowsRemoved += (sender, e) => UpdateRowNumbers(dataGridView, rowNumberColumnName, form.GetPageSize(), form.GetCurrentPage());
        }

        /// <summary>
        /// 为DataGridView添加行号列，用于显示当前行数，带分页
        /// </summary>
        /// <param name="dataGridView">目标DataGridView控件</param>
        /// <param name="rowNumberColumnName">行号列名称，默认为"RowNumber"</param>
        public static void AddRowNumberColumn(DataGridView dataGridView, IPagination form, string rowNumberColumnName = "RowNumber")
        {
            // 检查参数有效性
            if (dataGridView == null)
            {
                throw new ArgumentNullException(nameof(dataGridView), "DataGridView不能为null");
            }

            // 如果已存在行号列则先移除
            if (dataGridView.Columns.Contains(rowNumberColumnName))
            {
                dataGridView.Columns.Remove(rowNumberColumnName);
            }

            // 创建行号列
            DataGridViewTextBoxColumn rowNumberColumn = new DataGridViewTextBoxColumn();
            rowNumberColumn.Name = rowNumberColumnName;
            rowNumberColumn.HeaderText = "行号";
            rowNumberColumn.Width = 50;
            rowNumberColumn.ReadOnly = true;
            rowNumberColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

            // 将行号列插入到第一列位置
            dataGridView.Columns.Insert(0, rowNumberColumn);

            // 更新现有行的行号
            UpdateRowNumbers(dataGridView, rowNumberColumnName, form.GetPageSize(), form.GetCurrentPage());

            // 监听行状态变化事件，动态更新行号
            dataGridView.RowsAdded += (sender, e) => UpdateRowNumbers(dataGridView, rowNumberColumnName, form.GetPageSize(), form.GetCurrentPage());
            dataGridView.RowsRemoved += (sender, e) => UpdateRowNumbers(dataGridView, rowNumberColumnName, form.GetPageSize(), form.GetCurrentPage());
        }

        /// <summary>
        /// 为DataGridView添加行号列，用于显示当前行数，带分页
        /// </summary>
        /// <param name="dataGridView">目标DataGridView控件</param>
        /// <param name="rowNumberColumnName">行号列名称，默认为"RowNumber"</param>
        //public static void AddRowNumberColumn<T>(DataGridView dataGridView, IPagination<T> form, string rowNumberColumnName = "RowNumber") where T : Entity, new()
        //{
        //    // 检查参数有效性
        //    if (dataGridView == null)
        //    {
        //        throw new ArgumentNullException(nameof(dataGridView), "DataGridView不能为null");
        //    }

        //    // 如果已存在行号列则先移除
        //    if (dataGridView.Columns.Contains(rowNumberColumnName))
        //    {
        //        dataGridView.Columns.Remove(rowNumberColumnName);
        //    }

        //    // 创建行号列
        //    DataGridViewTextBoxColumn rowNumberColumn = new DataGridViewTextBoxColumn();
        //    rowNumberColumn.Name = rowNumberColumnName;
        //    rowNumberColumn.HeaderText = "行号";
        //    rowNumberColumn.Width = 50;
        //    rowNumberColumn.ReadOnly = true;
        //    rowNumberColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

        //    // 将行号列插入到第一列位置
        //    dataGridView.Columns.Insert(0, rowNumberColumn);

        //    // 更新现有行的行号
        //    UpdateRowNumbers(dataGridView, rowNumberColumnName, form.GetPageSize(), form.GetCurrentPage());

        //    // 监听行状态变化事件，动态更新行号
        //    dataGridView.RowsAdded += (sender, e) => UpdateRowNumbers(dataGridView, rowNumberColumnName, form.GetPageSize(), form.GetCurrentPage());
        //    dataGridView.RowsRemoved += (sender, e) => UpdateRowNumbers(dataGridView, rowNumberColumnName, form.GetPageSize(), form.GetCurrentPage());
        //}

        /// <summary>
        /// 为DataGridView添加行号列，用于显示当前行数，不带分页
        /// </summary>
        /// <param name="dataGridView">目标DataGridView控件</param>
        /// <param name="rowNumberColumnName">行号列名称，默认为"RowNumber"</param>
        public static void AddRowNumberColumn(DataGridView dataGridView, string rowNumberColumnName = "RowNumber")
        {
            // 检查参数有效性
            if (dataGridView == null)
            {
                throw new ArgumentNullException(nameof(dataGridView), "DataGridView不能为null");
            }

            // 如果已存在行号列则先移除
            if (dataGridView.Columns.Contains(rowNumberColumnName))
            {
                dataGridView.Columns.Remove(rowNumberColumnName);
            }

            // 创建行号列
            DataGridViewTextBoxColumn rowNumberColumn = new DataGridViewTextBoxColumn();
            rowNumberColumn.Name = rowNumberColumnName;
            rowNumberColumn.HeaderText = "行号";
            rowNumberColumn.Width = 50;
            rowNumberColumn.ReadOnly = true;
            rowNumberColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

            // 将行号列插入到第一列位置
            dataGridView.Columns.Insert(0, rowNumberColumn);

            // 更新现有行的行号
            UpdateRowNumbers(dataGridView, rowNumberColumnName);

            // 监听行状态变化事件，动态更新行号
            dataGridView.RowsAdded += (sender, e) => UpdateRowNumbers(dataGridView, rowNumberColumnName);
            dataGridView.RowsRemoved += (sender, e) => UpdateRowNumbers(dataGridView, rowNumberColumnName);
        }

        /// <summary>
        /// 更新DataGridView中所有行的行号，带分页
        /// </summary>
        /// <param name="dataGridView">目标DataGridView控件</param>
        /// <param name="rowNumberColumnName">行号列名称</param>
        private static void UpdateRowNumbers(DataGridView dataGridView, string rowNumberColumnName, int pageSize, int currentPage)
        {
            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                dataGridView.Rows[i].Cells[rowNumberColumnName].Value = (currentPage - 1) * pageSize + i + 1;
            }
        }

        /// <summary>
        /// 更新DataGridView中所有行的行号，不带分页
        /// </summary>
        /// <param name="dataGridView">目标DataGridView控件</param>
        /// <param name="rowNumberColumnName">行号列名称</param>
        private static void UpdateRowNumbers(DataGridView dataGridView, string rowNumberColumnName)
        {
            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                dataGridView.Rows[i].Cells[rowNumberColumnName].Value = i + 1;
            }
        }

        /// <summary>
        /// 在DataGridView上方添加查询行DataGridView
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="mainDataGridView">主DataGridView控件</param>
        /// <param name="parentControl">父容器控件（如Panel或Form）</param>
        /// <param name="onSearch">查询回调函数，接收查询条件字典</param>
        /// <returns>查询行DataGridView控件</returns>
        public static DataGridView AddQueryRowDataGridView<T>(DataGridView mainDataGridView, Control parentControl, Action<Dictionary<string, string>> onSearch = null) where T : new()
        {
            if (mainDataGridView == null)
            {
                throw new ArgumentNullException(nameof(mainDataGridView), "主DataGridView不能为null");
            }

            if (parentControl == null)
            {
                throw new ArgumentNullException(nameof(parentControl), "父容器控件不能为null");
            }

            // 创建查询行DataGridView
            DataGridView queryDataGridView = new DataGridView();
            queryDataGridView.Name = "QueryRowDataGridView";
            queryDataGridView.Height = 30;
            queryDataGridView.RowCount = 1;
            queryDataGridView.AllowUserToAddRows = false;
            queryDataGridView.AllowUserToDeleteRows = false;
            queryDataGridView.ReadOnly = false;
            queryDataGridView.SelectionMode = DataGridViewSelectionMode.CellSelect;
            queryDataGridView.MultiSelect = false;
            queryDataGridView.RowHeadersVisible = false;
            queryDataGridView.ColumnHeadersVisible = false;
            queryDataGridView.ScrollBars = ScrollBars.None;
            queryDataGridView.BackgroundColor = Color.FromArgb(240, 240, 240);
            queryDataGridView.BorderStyle = BorderStyle.None;

            // 根据主DataGridView的列创建查询行的列
            foreach (DataGridViewColumn mainColumn in mainDataGridView.Columns)
            {
                // 跳过选择列和行号列
                if (mainColumn.Name == "Selection" || mainColumn.Name == "RowNumber")
                {
                    DataGridViewColumn qColumn = new DataGridViewTextBoxColumn();
                    qColumn.Name = mainColumn.Name;
                    qColumn.Width = mainColumn.Width;
                    qColumn.ReadOnly = true;
                    qColumn.Visible = mainColumn.Visible;
                    queryDataGridView.Columns.Add(qColumn);
                    continue;
                }

                // 创建对应的查询列
                DataGridViewColumn queryColumn;
                if (mainColumn is DataGridViewCheckBoxColumn)
                {
                    queryColumn = new DataGridViewCheckBoxColumn();
                }
                else
                {
                    queryColumn = new DataGridViewTextBoxColumn();
                }

                queryColumn.Name = mainColumn.Name;
                queryColumn.DataPropertyName = mainColumn.DataPropertyName;
                queryColumn.Width = mainColumn.Width;
                queryColumn.Visible = mainColumn.Visible;
                queryColumn.ReadOnly = false;
                queryColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

                queryDataGridView.Columns.Add(queryColumn);
            }

            // 添加空行
            queryDataGridView.Rows.Add();

            // 监听主DataGridView列宽变化事件，同步查询行列宽
            mainDataGridView.ColumnWidthChanged += (sender, e) =>
            {
                if (e.Column != null && !string.IsNullOrEmpty(e.Column.Name))
                {
                    var queryColumn = queryDataGridView.Columns[e.Column.Name];
                    if (queryColumn != null)
                    {
                        queryColumn.Width = e.Column.Width;
                    }
                }
            };

            // 监听主DataGridView列显示/隐藏变化事件，同步查询行列可见性
            mainDataGridView.ColumnDisplayIndexChanged += (sender, e) =>
            {
                if (e.Column != null && !string.IsNullOrEmpty(e.Column.Name))
                {
                    var queryColumn = queryDataGridView.Columns[e.Column.Name];
                    if (queryColumn != null)
                    {
                        queryColumn.DisplayIndex = e.Column.DisplayIndex;
                    }
                }
            };

            // 监听查询行DataGridView的按键事件，实现回车查询
            queryDataGridView.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    
                    // 收集查询条件
                    Dictionary<string, string> queryConditions = new Dictionary<string, string>();
                    foreach (DataGridViewColumn column in queryDataGridView.Columns)
                    {
                        // 跳过选择列和行号列
                        if (column.Name == "Selection" || column.Name == "RowNumber")
                            continue;

                        var cellValue = queryDataGridView.Rows[0].Cells[column.Name].Value;
                        if (cellValue != null && !string.IsNullOrWhiteSpace(cellValue.ToString()))
                        {
                            queryConditions[column.DataPropertyName] = cellValue.ToString();
                        }
                    }

                    // 执行查询回调
                    onSearch?.Invoke(queryConditions);
                }
            };

            // 将查询行DataGridView添加到父容器
            parentControl.Controls.Add(queryDataGridView);
            queryDataGridView.BringToFront();

            // 设置查询行DataGridView的位置（在主DataGridView上方）
            queryDataGridView.Dock = DockStyle.Top;

            // 监听父容器大小变化事件，调整宽度
            if (parentControl is Panel parentForm)
            {
                parentForm.Resize += (sender, e) =>
                {
                    queryDataGridView.Width = mainDataGridView.Width;
                    queryDataGridView.Left = mainDataGridView.Left;
                };
            }

            return queryDataGridView;
        }

        /// <summary>
        /// 移除查询行DataGridView
        /// </summary>
        /// <param name="mainDataGridView">主DataGridView控件</param>
        /// <param name="parentControl">父容器控件</param>
        public static void RemoveQueryRowDataGridView(DataGridView mainDataGridView, Control parentControl)
        {
            if (mainDataGridView == null || parentControl == null)
                return;

            // 查找查询行DataGridView
            DataGridView queryDataGridView = parentControl.Controls.Find("QueryRowDataGridView", true)
                .FirstOrDefault() as DataGridView;

            if (queryDataGridView != null)
            {
                // 恢复主DataGridView的位置和高度
                mainDataGridView.Top -= queryDataGridView.Height;
                mainDataGridView.Height += queryDataGridView.Height;

                // 移除查询行DataGridView
                parentControl.Controls.Remove(queryDataGridView);
                queryDataGridView.Dispose();
            }
        }
    }
}
