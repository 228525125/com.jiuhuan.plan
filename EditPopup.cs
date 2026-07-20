using com.jiuhuan.plan.commands;
using com.jiuhuan.plan.domain;
using com.jiuhuan.plan.models;
using com.jiuhuan.plan.tools;
using QFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.jiuhuan.plan
{
    /// <summary>
    /// 实体编辑弹窗
    /// </summary>
    public class EditPopup<T> : BasePopup where T : Entity, new()
    {

        protected T record;
        protected List<Workday> workdays = new List<Workday>();
        protected User user = null;

        public EditPopup()
        {
            
        }

        private void EditPopup_Load(object sender, EventArgs e)
        {
            
        }

        protected virtual FlowLayoutPanel GetFlowLayoutPanel()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 初始化控件并填充数据
        /// </summary>
        /// <param name="record">要编辑的工单记录</param>
        public virtual void SetRecord(T record)
        {
            this.record = record;
            this.workdays = DaoTemplate.FindAll<Workday>();
            this.user = this.GetModel<ISessionModel>().GetUser();

            // 处理 OneToMany 关联数据的加载
            LoadOneToManyData(record);

            // 填充所有控件的值
            CreateControlsFromAttributes(record, GetFlowLayoutPanel());

            // 绑定PopupAttribute的双击事件
            BindPopupDoubleClickEvents(GetFlowLayoutPanel());
        }

        /// <summary>
        /// 删除选中行，包括底稿和数据库
        /// </summary>
        protected void DeleteSelectedRows()
        {
            try
            {
                // 在当前表单中递归查找 TabControl
                TabControl tabControl = UV.FindControlRecursive<TabControl>(this);
                if (tabControl == null || tabControl.SelectedTab == null)
                {
                    MessageBox.Show("未找到TabControl或未选中TabPage", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                TabPage currentPage = tabControl.SelectedTab;

                // 从当前TabPage中查找DataGridView控件
                DataGridView dataGridView = UV.FindDataGridViewInTabPage(currentPage);
                if (dataGridView == null)
                {
                    MessageBox.Show($"在TabPage '{currentPage.Text}' 中未找到DataGridView控件", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 获取所有选中的行
                var selectedRows = UV.GetSelectedRows(dataGridView);
                if (selectedRows == null || selectedRows.Count == 0)
                {
                    MessageBox.Show("请先选择要删除的行", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 确认删除操作
                DialogResult result = MessageBox.Show(
                    $"确定要删除选中的 {selectedRows.Count} 行数据吗？",
                    "确认删除",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    return;
                }

                // 检查 DataGridView 的数据源类型
                object dataSource = dataGridView.DataSource;
                DataTable dataTable = null;
                System.Collections.IList listDataSource = null;

                if (dataSource is DataTable dt)
                {
                    // 数据源是 DataTable
                    dataTable = dt;
                }
                else if (dataSource is IList list)
                {
                    // 数据源是 List<T>
                    listDataSource = list;
                }

                int deletedCount = 0;

                if (dataTable != null)
                {
                    // 从 DataTable 中删除选中的行
                    // 需要反向遍历，避免索引变化问题
                    for (int i = dataGridView.Rows.Count - 1; i >= 0; i--)
                    {
                        DataGridViewRow row = dataGridView.Rows[i];

                        // 跳过新行
                        if (row.IsNewRow)
                            continue;

                        // 检查是否被选中
                        if (Convert.ToBoolean(row.Cells["Selection"].Value ?? false))
                        {
                            // 获取对应的 DataRowView
                            if (row.DataBoundItem is DataRowView dataRowView)
                            {
                                dataRowView.Row.Delete();
                                deletedCount++;
                            }
                        }
                    }

                    // 提交更改
                    dataTable.AcceptChanges();
                }
                else if (listDataSource != null)
                {
                    // 从 List<T> 中删除选中的行
                    // 收集要删除的项（反向遍历）
                    List<object> itemsToDelete = new List<object>();

                    for (int i = dataGridView.Rows.Count - 1; i >= 0; i--)
                    {
                        DataGridViewRow row = dataGridView.Rows[i];

                        // 跳过新行
                        if (row.IsNewRow)
                            continue;

                        // 检查是否被选中
                        if (Convert.ToBoolean(row.Cells["Selection"].Value ?? false))
                        {
                            if (row.DataBoundItem != null)
                            {
                                itemsToDelete.Add(row.DataBoundItem);
                            }
                        }
                    }

                    // 从列表中删除项
                    foreach (var item in itemsToDelete)
                    {
                        // 使用反射调用 Remove 方法
                        var removeMethod = listDataSource.GetType().GetMethod("Remove");
                        if (removeMethod != null)
                        {
                            removeMethod.Invoke(listDataSource, new object[] { item });
                            deletedCount++;
                        }
                    }

                    // 刷新 DataGridView 显示
                    UV.SetDataSourceForGridView(dataGridView, listDataSource);
                }
                else
                {
                    MessageBox.Show("不支持的数据源类型", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show($"成功删除 {deletedCount} 行数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 加载 OneToMany 关联数据
        /// </summary>
        /// <param name="record">父记录对象</param>
        private void LoadOneToManyData(T record)
        {
            if (record == null) return;

            // 处理 OneToMany 关联数据的加载
            Type recordType = typeof(T);
            var properties = recordType.GetProperties();

            foreach (var property in properties)
            {
                // 检查是否包含 OneToMany 特性
                var oneToManyAttr = Attribute.GetCustomAttribute(property, typeof(OneToManyAttribute)) as OneToManyAttribute;
                
                if (oneToManyAttr != null)
                {
                    try
                    {
                        // 获取子实体类型
                        Type childType = oneToManyAttr.ChildType;
                        
                        // 获取外键字段名 (joinColumn) 和映射字段名 (mappedBy)
                        string joinColumn = oneToManyAttr.JoinColumn;
                        string mappedBy = oneToManyAttr.MappedBy;

                        if (string.IsNullOrEmpty(joinColumn) || string.IsNullOrEmpty(mappedBy))
                        {
                            Console.WriteLine($"属性 {property.Name} 的 OneToMany 特性未正确配置 JoinColumn 或 MappedBy");
                            continue;
                        }

                        // 获取当前父记录的 joinColumn 字段值，用于关联查询
                        var joinColumnProperty = recordType.GetProperty(joinColumn);
                        if (joinColumnProperty == null)
                        {
                            Console.WriteLine($"父记录中未找到字段 {joinColumn}");
                            continue;
                        }

                        object parentKeyValue = joinColumnProperty.GetValue(record);
                        
                        if (parentKeyValue == null)
                        {
                            // 如果父记录的关键字段为空，初始化为空列表
                            var lt = typeof(List<>).MakeGenericType(childType);
                            var emptyList = Activator.CreateInstance(lt);
                            property.SetValue(record, emptyList);
                            continue;
                        }

                        // 构建查询条件：ChildType.mappedBy = ParentRecord.joinColumn
                        // 使用 DaoTemplate.FindAll<T>() 方法加载所有子记录，然后在内存中过滤
                        // 或者构造 SQL 查询
                        
                        // 方法1：使用 SQL 查询（推荐，性能更好）
                        string tableName = UV.GetTableName(childType);
                        if (string.IsNullOrEmpty(tableName))
                        {
                            Console.WriteLine($"无法获取子实体 {childType.Name} 的表名");
                            continue;
                        }

                        // 格式化参数值
                        string formattedValue = DaoTemplate.FormatSqlValue(parentKeyValue);
                        string sql = $"SELECT * FROM [{tableName}] WHERE [{mappedBy}] = {formattedValue}";
                        
                        // 执行查询
                        List<Dictionary<string, object>> rows = DaoTemplate.FindAll(sql);
                        
                        // 将 Dictionary 列表转换为实体列表
                        var listType = typeof(List<>).MakeGenericType(childType);
                        var resultList = Activator.CreateInstance(listType) as System.Collections.IList;
                        
                        foreach (var row in rows)
                        {
                            object entity = UV.ConvertDictionaryToEntity(row, childType);
                            if (entity != null)
                            {
                                resultList.Add(entity);
                            }
                        }
                        
                        // 将结果赋值给属性
                        property.SetValue(record, resultList);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"加载 OneToMany 属性 {property.Name} 时出错: {ex.Message}");
                        // 出错时初始化为空列表，避免空引用异常
                        var childType = oneToManyAttr.ChildType;
                        var listType = typeof(List<>).MakeGenericType(childType);
                        var emptyList = Activator.CreateInstance(listType);
                        property.SetValue(record, emptyList);
                    }
                }
            }
        }

        /// <summary>
        /// 从控件中提取数据并更新Record对象
        /// </summary>
        protected virtual void UpdateRecordFromControls()
        {
            if (this.record == null) return;

            // 遍历scheduleRecord的所有属性
            Type recordType = typeof(T);
            var properties = recordType.GetProperties();

            foreach (var property in properties)
            {
                // 检查是否包含 OneToMany 特性
                var oneToManyAttr = Attribute.GetCustomAttribute(property, typeof(OneToManyAttribute)) as OneToManyAttribute;
                
                if (oneToManyAttr != null)
                {
                    // 处理 OneToMany 特性修饰的属性
                    try
                    {
                        // 获取当前选中的 TabPage 标题
                        string currentTabTitle = oneToManyAttr.Title;
                        
                        if (string.IsNullOrEmpty(currentTabTitle))
                        {
                            Console.WriteLine($"属性 {property.Name} 的 OneToMany 特性未设置 Title");
                            continue;
                        }

                        // 在整个表单中递归查找 TabControl
                        TabControl tabControl = UV.FindControlRecursive<TabControl>(this);
                        if (tabControl == null)
                        {
                            Console.WriteLine($"未找到 TabControl，无法处理属性 {property.Name}");
                            continue;
                        }

                        // 查找与 OneToMany Title 匹配的 TabPage
                        TabPage targetTabPage = null;
                        foreach (TabPage tabPage in tabControl.TabPages)
                        {
                            if (string.Equals(tabPage.Text, currentTabTitle, StringComparison.OrdinalIgnoreCase))
                            {
                                targetTabPage = tabPage;
                                break;
                            }
                        }

                        if (targetTabPage == null)
                        {
                            Console.WriteLine($"未找到标题为 '{currentTabTitle}' 的 TabPage，无法处理属性 {property.Name}");
                            continue;
                        }

                        // 从 TabPage 中查找 DataGridView 控件
                        DataGridView dataGridView = UV.FindDataGridViewInTabPage(targetTabPage);
                        if (dataGridView == null)
                        {
                            Console.WriteLine($"在 TabPage '{currentTabTitle}' 中未找到 DataGridView 控件，无法处理属性 {property.Name}");
                            continue;
                        }

                        // 获取 DataGridView 中的所有行数据并转换为实体列表
                        List<object> entityList = new List<object>();
                        Type childType = oneToManyAttr.ChildType;

                        foreach (DataGridViewRow row in dataGridView.Rows)
                        {
                            // 跳过新行（AllowUserToAddRows 产生的空行）
                            if (row.IsNewRow)
                                continue;

                            // 直接从 DataGridView 单元格构建实体（最可靠的方式）
                            object entity = UV.CreateEntityFromDataGridViewRow(row, childType);
                            if (entity != null)
                            {
                                entityList.Add(entity);
                            }
                        }

                        // 将实体列表转换为正确的类型并赋值给属性
                        if (property.PropertyType.IsGenericType && 
                            property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            // 创建 List<T> 实例
                            Type listType = typeof(List<>).MakeGenericType(childType);
                            object listInstance = Activator.CreateInstance(listType);
                            
                            // 调用 Add 方法添加所有实体
                            MethodInfo addMethod = listType.GetMethod("Add");
                            foreach (var item in entityList)
                            {
                                if (childType.IsAssignableFrom(item.GetType()))
                                {
                                    addMethod.Invoke(listInstance, new object[] { item });
                                }
                            }
                            
                            // 设置属性值
                            property.SetValue(record, listInstance);
                        }
                        else if (property.PropertyType.IsArray)
                        {
                            // 如果是数组类型，转换为数组
                            Array array = Array.CreateInstance(childType, entityList.Count);
                            for (int i = 0; i < entityList.Count; i++)
                            {
                                array.SetValue(entityList[i], i);
                            }
                            property.SetValue(record, array);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"更新 OneToMany 属性 {property.Name} 时出错: {ex.Message}");
                    }
                    
                    // 跳过后续的普通控件处理
                    continue;
                }

                // 获取属性名称，这应该与控件的Name属性匹配
                string propertyName = property.Name;

                // 查找对应的控件
                Control control = FindControlByName(GetFlowLayoutPanel(), propertyName);

                if (control != null)
                {
                    // 根据控件类型更新属性值
                    if (control is TextBox textBox)
                    {
                        // 处理TextBox控件
                        try
                        {
                            if (property.PropertyType == typeof(string))
                            {
                                property.SetValue(record, textBox.Text);
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (int.TryParse(textBox.Text, out int intValue))
                                {
                                    property.SetValue(record, intValue);
                                }
                            }
                            else if (property.PropertyType == typeof(float))
                            {
                                if (float.TryParse(textBox.Text, out float floatValue))
                                {
                                    property.SetValue(record, floatValue);
                                }
                            }
                            else if (property.PropertyType == typeof(double))
                            {
                                if (double.TryParse(textBox.Text, out double doubleValue))
                                {
                                    property.SetValue(record, doubleValue);
                                }
                            }
                            else if (property.PropertyType == typeof(decimal))
                            {
                                if (decimal.TryParse(textBox.Text, out decimal decimalValue))
                                {
                                    property.SetValue(record, decimalValue);
                                }
                            }
                            // 可以根据需要添加更多类型支持
                        }
                        catch (Exception ex)
                        {
                            // 记录日志或处理异常
                            Console.WriteLine($"更新属性 {propertyName} 时出错: {ex.Message}");
                        }
                    }
                    else if (control is CheckBox checkBox)
                    {
                        // 处理CheckBox控件
                        try
                        {
                            if (property.PropertyType == typeof(bool))
                            {
                                property.SetValue(record, checkBox.Checked);
                            }
                        }
                        catch (Exception ex)
                        {
                            // 记录日志或处理异常
                            Console.WriteLine($"更新属性 {propertyName} 时出错: {ex.Message}");
                        }
                    }
                    else if (control is DateTimePicker dateTimePicker)
                    {
                        // 处理DateTimePicker控件
                        try
                        {
                            if (property.PropertyType == typeof(DateTime))
                            {
                                property.SetValue(record, dateTimePicker.Value);
                            }
                        }
                        catch (Exception ex)
                        {
                            // 记录日志或处理异常
                            Console.WriteLine($"更新属性 {propertyName} 时出错: {ex.Message}");
                        }
                    }
                    else if (control is ComboBox comboBox)
                    {
                        // 处理ComboBox控件
                        try
                        {
                            if (property.PropertyType == typeof(string))
                            {
                                property.SetValue(record, comboBox.SelectedItem?.ToString());
                            }
                            else
                            {
                                // 如果属性类型不是字符串，则尝试转换
                                if (comboBox.SelectedItem != null)
                                {
                                    var convertedValue = Convert.ChangeType(comboBox.SelectedItem.ToString(), property.PropertyType);
                                    property.SetValue(record, convertedValue);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // 记录日志或处理异常
                            Console.WriteLine($"更新属性 {propertyName} 时出错: {ex.Message}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 根据ControlAttribute属性的描述创建Label和控件，并添加到FlowLayoutPanel中
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="entity">实体对象</param>
        /// <param name="flowLayoutPanel">容器</param>
        private void CreateControlsFromAttributes(T entity, FlowLayoutPanel flowLayoutPanel)
        {
            bool isNew = 0 == entity.FID;   //是否新创建的

            // 清空容器中现有的控件
            flowLayoutPanel.Controls.Clear();

            Type entityType = typeof(T);
            var properties = entityType.GetProperties();

            // 收集所有带控件属性的属性信息
            var controlInfoList = new List<Tuple<PropertyInfo, ControlAttribute>>();

            foreach (var property in properties)
            {
                // 处理TextBoxAttribute
                var textBoxAttribute = Attribute.GetCustomAttribute(property, typeof(TextBoxAttribute)) as TextBoxAttribute;
                // 处理CheckBoxAttribute
                var checkBoxAttribute = Attribute.GetCustomAttribute(property, typeof(CheckBoxAttribute)) as CheckBoxAttribute;
                // 处理DateTimePickerAttribute
                var dateTimePickerAttribute = Attribute.GetCustomAttribute(property, typeof(DateTimePickerAttribute)) as DateTimePickerAttribute;
                // 处理ComboBoxAttribute
                var comboBoxAttribute = Attribute.GetCustomAttribute(property, typeof(ComboBoxAttribute)) as ComboBoxAttribute;

                // 确定使用的属性
                ControlAttribute controlAttribute = textBoxAttribute ?? (ControlAttribute)checkBoxAttribute ?? dateTimePickerAttribute;
                controlAttribute = controlAttribute ?? comboBoxAttribute;

                if (controlAttribute != null)
                {
                    controlInfoList.Add(new Tuple<PropertyInfo, ControlAttribute>(property, controlAttribute));
                }
            }

            // 根据Index属性排序，Index值越小位置越靠前
            var sortedControlInfoList = controlInfoList.OrderBy(tuple => tuple.Item2.Index).ToList();

            // 根据排序后的信息创建控件
            foreach (var tuple in sortedControlInfoList)
            {
                var property = tuple.Item1;
                var controlAttribute = tuple.Item2;

                // 判断具体是哪种控件属性
                var textBoxAttribute = controlAttribute as TextBoxAttribute;
                var checkBoxAttribute = controlAttribute as CheckBoxAttribute;
                var dateTimePickerAttribute = controlAttribute as DateTimePickerAttribute;
                var comboBoxAttribute = controlAttribute as ComboBoxAttribute;

                if (textBoxAttribute != null)
                {
                    // 创建Label控件
                    Label label = new Label();
                    label.Text = string.IsNullOrEmpty(textBoxAttribute.Title) ? Utility.GetAttributeValueByField<T>("Field", "Name", property.Name) as string : textBoxAttribute.Title;
                    label.AutoSize = true;
                    label.TextAlign = ContentAlignment.MiddleRight;
                    label.Padding = new Padding(0, 5, 5, 0);

                    // 创建TextBox控件
                    TextBox textBox = new TextBox();
                    textBox.Name = property.Name;
                    textBox.Width = textBoxAttribute.Width;
                    textBox.ReadOnly = textBoxAttribute.UI || (!isNew && textBoxAttribute.ReadOnly);

                    // 设置TextBox的值
                    var propertyValue = property.GetValue(entity);
                    textBox.Text = propertyValue?.ToString() ?? "";

                    // 创建面板来容纳Label和TextBox
                    Panel panel = new Panel();
                    panel.Height = 30;
                    panel.Width = textBox.Width + label.Width + 20;
                    panel.Dock = DockStyle.Top;

                    // 设置控件位置
                    label.Location = new Point(0, 5);
                    textBox.Location = new Point(label.Width + 5, 2);

                    // 将控件添加到面板
                    panel.Controls.Add(label);
                    panel.Controls.Add(textBox);

                    // 将面板添加到FlowLayoutPanel
                    flowLayoutPanel.Controls.Add(panel);
                }
                else if (checkBoxAttribute != null)
                {
                    // 创建Label控件
                    Label label = new Label();
                    label.Text = string.IsNullOrEmpty(checkBoxAttribute.Title) ? Utility.GetAttributeValueByField<T>("Field", "Name", property.Name) as string : checkBoxAttribute.Title;
                    label.AutoSize = true;
                    label.TextAlign = ContentAlignment.MiddleRight;
                    label.Padding = new Padding(0, 5, 5, 0);

                    // 创建CheckBox控件
                    CheckBox checkBox = new CheckBox();
                    checkBox.Name = property.Name;
                    checkBox.Enabled = !checkBoxAttribute.UI && (isNew || !checkBoxAttribute.ReadOnly); // CheckBox使用Enabled而不是ReadOnly

                    // 设置CheckBox的选中状态
                    var propertyValue = property.GetValue(entity);
                    if (propertyValue is bool boolValue)
                    {
                        checkBox.Checked = boolValue;
                    }
                    else if (bool.TryParse(propertyValue?.ToString(), out bool parsedValue))
                    {
                        checkBox.Checked = parsedValue;
                    }
                    else
                    {
                        checkBox.Checked = false;
                    }

                    // 创建面板来容纳Label和CheckBox
                    Panel panel = new Panel();
                    panel.Height = 30;
                    panel.Width = 200;
                    panel.Dock = DockStyle.Top;

                    // 设置控件位置
                    label.Location = new Point(0, 5);
                    checkBox.Location = new Point(label.Width + 5, 5);

                    // 将控件添加到面板
                    panel.Controls.Add(label);
                    panel.Controls.Add(checkBox);

                    // 将面板添加到FlowLayoutPanel
                    flowLayoutPanel.Controls.Add(panel);
                }
                else if (dateTimePickerAttribute != null)
                {
                    // 创建Label控件
                    Label label = new Label();
                    label.Text = string.IsNullOrEmpty(dateTimePickerAttribute.Title) ? Utility.GetAttributeValueByField<T>("Field", "Name", property.Name) as string : dateTimePickerAttribute.Title;
                    label.AutoSize = true;
                    label.TextAlign = ContentAlignment.MiddleRight;
                    label.Padding = new Padding(0, 5, 5, 0);

                    // 创建DateTimePicker控件
                    DateTimePicker dateTimePicker = new DateTimePicker();
                    dateTimePicker.Name = property.Name;
                    dateTimePicker.Width = dateTimePickerAttribute.Width;
                    dateTimePicker.Enabled = !dateTimePickerAttribute.UI && (isNew || !dateTimePickerAttribute.ReadOnly);

                    // 设置DateTimePicker的值
                    var propertyValue = property.GetValue(entity);
                    if (propertyValue is DateTime dateTimeValue)
                    {
                        dateTimePicker.Value = dateTimeValue;
                    }
                    else if (DateTime.TryParse(propertyValue?.ToString(), out DateTime parsedValue))
                    {
                        dateTimePicker.Value = parsedValue;
                    }
                    else
                    {
                        dateTimePicker.Value = DateTime.Now;
                    }

                    // 创建面板来容纳Label和DateTimePicker
                    Panel panel = new Panel();
                    panel.Height = 30;
                    panel.Width = dateTimePicker.Width + label.Width + 20;
                    panel.Dock = DockStyle.Top;

                    // 设置控件位置
                    label.Location = new Point(0, 5);
                    dateTimePicker.Location = new Point(label.Width + 5, 2);

                    // 将控件添加到面板
                    panel.Controls.Add(label);
                    panel.Controls.Add(dateTimePicker);

                    // 将面板添加到FlowLayoutPanel
                    flowLayoutPanel.Controls.Add(panel);
                }
                else if (comboBoxAttribute != null)
                {
                    // 创建Label控件
                    Label label = new Label();
                    label.Text = string.IsNullOrEmpty(comboBoxAttribute.Title) ? Utility.GetAttributeValueByField<T>("Field", "Name", property.Name) as string : comboBoxAttribute.Title;
                    label.AutoSize = true;
                    label.TextAlign = ContentAlignment.MiddleRight;
                    label.Padding = new Padding(0, 5, 5, 0);

                    // 创建ComboBox控件
                    ComboBox comboBox = new ComboBox();
                    comboBox.Name = property.Name;
                    comboBox.Width = comboBoxAttribute.Width;
                    comboBox.Enabled = comboBoxAttribute.UI && (isNew || !comboBoxAttribute.ReadOnly);

                    // 解析并设置ComboBox的选项
                    if (!string.IsNullOrEmpty(comboBoxAttribute.Items))
                    {
                        string[] items = comboBoxAttribute.Items.Split(';');
                        foreach (string item in items)
                        {
                            comboBox.Items.Add(item.Trim());
                        }
                    }

                    // 设置ComboBox的值
                    var propertyValue = property.GetValue(entity);
                    if (propertyValue != null)
                    {
                        comboBox.SelectedItem = propertyValue.ToString();
                    }

                    // 创建面板来容纳Label和ComboBox
                    Panel panel = new Panel();
                    panel.Height = 30;
                    panel.Width = comboBox.Width + label.Width + 20;
                    panel.Dock = DockStyle.Top;

                    // 设置控件位置
                    label.Location = new Point(0, 5);
                    comboBox.Location = new Point(label.Width + 5, 2);

                    // 将控件添加到面板
                    panel.Controls.Add(label);
                    panel.Controls.Add(comboBox);

                    // 将面板添加到FlowLayoutPanel
                    flowLayoutPanel.Controls.Add(panel);
                }
            }

            // 查找所有被 OneToMany 特性修饰的属性，并绑定对应的 DataGridView 数据源
            var oneToManyProperties = typeof(T).GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(OneToManyAttribute)))
                .ToList();

            foreach (var property in oneToManyProperties)
            {
                var oneToManyAttr = property.GetCustomAttribute<OneToManyAttribute>();
                if (oneToManyAttr == null || string.IsNullOrEmpty(oneToManyAttr.Title))
                    continue;

                // 在整个表单中递归查找 TabControl
                TabControl tabControl = UV.FindControlRecursive<TabControl>(this);
                if (tabControl == null)
                    continue;

                // 查找与 OneToMany Title 匹配的 TabPage
                TabPage targetTabPage = null;
                foreach (TabPage tabPage in tabControl.TabPages)
                {
                    if (string.Equals(tabPage.Text, oneToManyAttr.Title, StringComparison.OrdinalIgnoreCase))
                    {
                        targetTabPage = tabPage;
                        break;
                    }
                }

                if (targetTabPage == null)
                    continue;

                // 从 TabPage 中查找 DataGridView 控件
                DataGridView dataGridView = UV.FindDataGridViewInTabPage(targetTabPage);
                if (dataGridView == null)
                    continue;

                // 获取属性值作为数据源
                var dataSource = property.GetValue(this.record);
                if (dataSource != null)
                {
                    // 如果数据源是 IList，检查是否为空
                    if (dataSource is IList list && list.Count == 0)
                    {
                        // 列表为空，不设置数据源或设置为空，避免显示空白行
                        dataGridView.DataSource = null;
                    }
                    else
                    {
                        // 设置 DataGridView 的数据源
                        dataGridView.DataSource = dataSource;
                    }
                }
            }
        }

        /// <summary>
        /// 根据控件名称在FlowLayoutPanel中查找控件
        /// </summary>
        /// <param name="flowLayoutPanel">要搜索的FlowLayoutPanel</param>
        /// <param name="controlName">控件名称</param>
        /// <returns>找到的控件，如果未找到则返回null</returns>
        protected Control FindControlByName(FlowLayoutPanel flowLayoutPanel, string controlName)
        {
            foreach (Control panel in flowLayoutPanel.Controls)
            {
                if (panel is Panel containerPanel)
                {
                    foreach (Control control in containerPanel.Controls)
                    {
                        if (control.Name == controlName)
                        {
                            return control;
                        }
                    }
                }
            }
            return null;
        }

        protected C FindControlByName<C>(FlowLayoutPanel flowLayoutPanel, string controlName) where C : Control
        {
            // 查找对应的控件
            Control control = FindControlByName(flowLayoutPanel, controlName);
            if (control is C c)
                return c;
            else
                return null;
        }

        protected bool ValidateInput()
        {
            if (this.record == null) 
                return false;

            Type recordType = typeof(T);
            var properties = recordType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                // 获取属性上的FieldAttribute特性
                var fieldAttribute = Attribute.GetCustomAttribute(property, typeof(FieldAttribute)) as FieldAttribute;

                if (fieldAttribute != null)
                {
                    // 获取属性的当前值
                    var value = property.GetValue(record);
                    string valueStr = value?.ToString() ?? "";

                    // 首先检查AllowNull约束
                    if (!fieldAttribute.AllowNull && (value == null || string.IsNullOrWhiteSpace(valueStr)))
                    {
                        MessageBox.Show(
                            $"字段 \"{fieldAttribute.Name}\" 不能为空！",
                            "输入验证",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );

                        return false;
                    }

                    // 如果值为空且允许为空，则跳过后续验证
                    if (string.IsNullOrWhiteSpace(valueStr))
                    {
                        continue;
                    }

                    // 如果有Validation规则，进行格式验证
                    if (!string.IsNullOrEmpty(fieldAttribute.Validation))
                    {
                        // 使用Utility.ParseFilterCondition解析验证规则
                        var condition = Utility.ParseFilterCondition(fieldAttribute.Validation);

                        if (condition != null)
                        {
                            // 使用Utility.MatchCondition进行验证
                            bool isValid = Utility.MatchCondition(value, condition);

                            if (!isValid)
                            {
                                // 显示错误消息
                                MessageBox.Show(
                                    $"字段 \"{fieldAttribute.Name}({property.Name})\" 验证失败！\n\n{fieldAttribute.ErrorMessage}",
                                    "输入验证",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning
                                );

                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 为带有PopupAttribute特性的属性对应的TextBox控件绑定双击事件
        /// 当用户双击TextBox时，根据PopupAttribute配置创建并显示弹窗
        /// </summary>
        /// <param name="flowLayoutPanel">包含控件的容器</param>
        public void BindPopupDoubleClickEvents(FlowLayoutPanel flowLayoutPanel)
        {
            if (flowLayoutPanel == null || this.record == null)
                return;

            Type entityType = typeof(T);
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                // 获取属性上的PopupAttribute特性
                var popupAttribute = Attribute.GetCustomAttribute(property, typeof(PopupAttribute)) as PopupAttribute;

                if (popupAttribute != null)
                {
                    // 查找对应的TextBox控件
                    TextBox textBox = FindControlByName<TextBox>(flowLayoutPanel, property.Name);

                    if (textBox != null && !textBox.ReadOnly)
                    {
                        // 移除已存在的事件处理器（防止重复绑定）
                        textBox.DoubleClick -= TextBox_OpenPopup;
                        
                        // 将PopupAttribute存储到Tag中供事件处理器使用
                        textBox.Tag = new Tuple<PropertyInfo, PopupAttribute>(property, popupAttribute);
                        
                        // 绑定双击事件
                        textBox.DoubleClick += TextBox_OpenPopup;
                    }
                }
            }
        }

        /// <summary>
        /// 根据popupAttr.Parameter中保存的属性名，从对应的控件中获取属性值的数组
        /// </summary>
        /// <returns></returns>
        private object [] GetParameter(PopupAttribute popupAttr)
        {
            if (popupAttr.Parameter == null || popupAttr.Parameter.Length == 0)
                return null;

            object [] parameterValues = new object[popupAttr.Parameter.Length];
            for (int i = 0; i < popupAttr.Parameter.Length; i++)
            {
                string parameterPropertyName = popupAttr.Parameter[i];
                var propertyInfo = typeof(T).GetProperty(parameterPropertyName);
                if (propertyInfo != null)
                {
                    // 从控件中获取值，而不是直接从record中获取
                    Control control = FindControlByName(GetFlowLayoutPanel(), parameterPropertyName);
                    
                    if (control != null)
                    {
                        // 根据控件类型获取值
                        if (control is TextBox textBox)
                        {
                            string textValue = textBox.Text;
                            // 根据属性类型转换值
                            if (propertyInfo.PropertyType == typeof(string))
                            {
                                parameterValues[i] = textValue;
                            }
                            else if (propertyInfo.PropertyType == typeof(int))
                            {
                                if (int.TryParse(textValue, out int intValue))
                                {
                                    parameterValues[i] = intValue;
                                }
                            }
                            else if (propertyInfo.PropertyType == typeof(float))
                            {
                                if (float.TryParse(textValue, out float floatValue))
                                {
                                    parameterValues[i] = floatValue;
                                }
                            }
                            else if (propertyInfo.PropertyType == typeof(double))
                            {
                                if (double.TryParse(textValue, out double doubleValue))
                                {
                                    parameterValues[i] = doubleValue;
                                }
                            }
                            else if (propertyInfo.PropertyType == typeof(decimal))
                            {
                                if (decimal.TryParse(textValue, out decimal decimalValue))
                                {
                                    parameterValues[i] = decimalValue;
                                }
                            }
                            else if (propertyInfo.PropertyType == typeof(DateTime))
                            {
                                if (DateTime.TryParse(textValue, out DateTime dateTimeValue))
                                {
                                    parameterValues[i] = dateTimeValue;
                                }
                            }
                            else if (propertyInfo.PropertyType == typeof(bool))
                            {
                                if (bool.TryParse(textValue, out bool boolValue))
                                {
                                    parameterValues[i] = boolValue;
                                }
                            }
                            else
                            {
                                parameterValues[i] = textValue;
                            }
                        }
                        else if (control is CheckBox checkBox)
                        {
                            parameterValues[i] = checkBox.Checked;
                        }
                        else if (control is DateTimePicker dateTimePicker)
                        {
                            parameterValues[i] = dateTimePicker.Value;
                        }
                        else if (control is ComboBox comboBox)
                        {
                            parameterValues[i] = comboBox.SelectedItem?.ToString();
                        }
                        else
                        {
                            // 如果找不到合适的控件类型，则从record中获取
                            parameterValues[i] = propertyInfo.GetValue(this.record);
                        }
                    }
                    else
                    {
                        // 如果找不到控件，则从record中获取
                        parameterValues[i] = propertyInfo.GetValue(this.record);
                    }
                }
            }
            return parameterValues;
        }

        /// <summary>
        /// 返回类型弹窗，返回数据
        /// </summary>
        /// <param name="popupAttr"></param>
        /// <param name="action"></param>
        protected void OpenPopup(PopupAttribute popupAttr, Action<Form> action = null)
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
                var parameters = GetParameter(popupAttr);
                
                if (sql.Contains("{0}") && parameters != null && parameters.Length > 0)
                {
                    sql = string.Format(sql, parameters);
                }

                // 这里可以根据需要替换SQL中的占位符，例如：
                // sql = sql.Replace("@FID", this.record.FID.ToString());

                // 执行查询
                List<Dictionary<string, object>> queryResult = DaoTemplate.FindAll(sql);

                if (!popupAttr.AllowDuplicates)
                {
                    // 处理 OneToMany 属性的差集逻辑
                    queryResult = FilterOneToManyExistingData(dataGridView, popupAttr, queryResult);
                }

                // 获取弹窗的SetDataSource方法
                MethodInfo setDataSourceMethod = popupType.GetMethod("SetDataSource");
                if (setDataSourceMethod != null)
                {
                    // 调用SetDataSource方法设置数据
                    setDataSourceMethod.Invoke(popupForm, new object[] { queryResult });
                }
            }
            else if(!string.IsNullOrEmpty(popupAttr.SqlFile))
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
                    var back = Utility.ParseXmlTag(sqlFromFile, "back");

                    if (!string.IsNullOrEmpty(param))
                    {
                        popupAttr.Parameter = param.Split(';');
                    }

                    if (!string.IsNullOrEmpty(back))
                    {
                        popupAttr.Return = back;
                    }

                    var parameters = GetParameter(popupAttr);

                    if (sqlFromFile.Contains("{0}") && parameters != null && parameters.Length > 0)
                    {
                        sqlFromFile = string.Format(sqlFromFile, parameters);
                    }

                    // 执行查询
                    List<Dictionary<string, object>> queryResult = DaoTemplate.FindAll(sqlFromFile);

                    if (!popupAttr.AllowDuplicates)
                    {
                        // 处理 OneToMany 属性的差集逻辑
                        queryResult = FilterOneToManyExistingData(dataGridView, popupAttr, queryResult);
                    }

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
                    var back = Utility.ParseXmlTag(sqlFromFile, "back");

                    if (!string.IsNullOrEmpty(param))
                    {
                        popupAttr.Parameter = param.Split(';');
                    }

                    if (!string.IsNullOrEmpty(back))
                    {
                        popupAttr.Return = back;
                    }

                    var parameters = GetParameter(popupAttr);

                    if (sqlFromFile.Contains("{0}") && parameters != null && parameters.Length > 0)
                    {
                        sqlFromFile = string.Format(sqlFromFile, parameters);
                    }
                    
                    // 执行查询
                    List<Dictionary<string, object>> queryResult = DaoTemplate.FindAll(sqlFromFile);

                    if (!popupAttr.AllowDuplicates)
                    {
                        // 处理 OneToMany 属性的差集逻辑
                        queryResult = FilterOneToManyExistingData(dataGridView, popupAttr, queryResult);
                    }

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

        /// <summary>
        /// TextBox双击事件处理器
        /// 根据PopupAttribute配置创建弹窗并显示数据
        /// </summary>
        /// <param name="sender">触发事件的TextBox控件</param>
        /// <param name="e">事件参数</param>
        private void TextBox_OpenPopup(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null || textBox.Tag == null)
                return;

            // 从Tag中获取PropertyInfo和PopupAttribute
            var tagData = textBox.Tag as Tuple<PropertyInfo, PopupAttribute>;
            if (tagData == null)
                return;

            PropertyInfo property = tagData.Item1;
            PopupAttribute popupAttr = tagData.Item2;

            OpenPopup(popupAttr, (popupForm) => {
                Type popupType = popupAttr.Type;
                List<Dictionary<string, object>> selectedItems = new List<Dictionary<string, object>>();

                // 尝试从弹窗获取选中的数据
                MethodInfo getSelectedItemsMethod = popupType.GetMethod("GetSelectedItems");
                if (getSelectedItemsMethod != null)
                {
                    string jsonString = getSelectedItemsMethod.Invoke(popupForm, null) as string;
                    selectedItems = JsonHelper.toObject<List<Dictionary<string, object>>>(jsonString);
                }

                // 如果有选中的数据，更新TextBox的值
                if (selectedItems != null && selectedItems.Count > 0)
                {
                    if (popupAttr.IsMultipleColumnReturn)
                    {
                        if (popupAttr.AllowMultipleRowSelection)
                        {

                        }
                        else
                        {
                            // 如果返回的是多列，例如"品号=FNumber;品名=FName;规格=FModel"，则根据FNumber、FName、FModel找到对应textBox控件赋值，并同步更新record对象的对应属性
                            // 解析多列返回配置，格式如："品号=FNumber;品名=FName;规格=FModel"
                            string[] columnMappings = popupAttr.Return.Split(';');
                            var firstItem = selectedItems[0];

                            foreach (string mapping in columnMappings)
                            {
                                if (string.IsNullOrWhiteSpace(mapping)) continue;

                                // 分割显示名称和数据列名，例如 "品号=FNumber"
                                string[] parts = mapping.Split('=');
                                if (parts.Length != 2) continue;

                                string displayLabel = parts[0].Trim(); // 例如 "品号"
                                string dataColumn = parts[1].Trim();   // 例如 "FNumber"

                                // 1. 查找对应的 TextBox 控件并赋值
                                // 注意：这里假设控件的 Name 属性与实体属性名一致，或者我们需要根据 displayLabel 找到对应的属性名
                                // 由于 CreateControlsFromAttributes 中控件 Name 设置为 property.Name
                                // 我们需要找到哪个属性的 FieldAttribute.Name 或 TextBoxAttribute.Title 匹配 displayLabel
                                // 或者更简单地，如果 Return 配置中的 key 直接对应实体属性名会更好，但这里配置的是 "显示名=数据列"

                                // 遍历实体属性，找到 Title 或 Name 匹配 displayLabel 的属性
                                Type recordType = typeof(T);
                                PropertyInfo targetProperty = null;

                                foreach (var prop in recordType.GetProperties())
                                {
                                    var fieldAttr = Attribute.GetCustomAttribute(prop, typeof(FieldAttribute)) as FieldAttribute;
                                    var textBoxAttr = Attribute.GetCustomAttribute(prop, typeof(TextBoxAttribute)) as TextBoxAttribute;

                                    string propName = prop.Name;
                                    //string title = fieldAttr?.Name ?? textBoxAttr?.Title;

                                    if (propName == dataColumn)
                                    {
                                        targetProperty = prop;
                                        break;
                                    }
                                }

                                if (targetProperty != null && firstItem.ContainsKey(displayLabel))
                                {
                                    object value = firstItem[displayLabel];
                                    string valueStr = value?.ToString() ?? "";

                                    // 更新 UI 控件
                                    TextBox targetTextBox = FindControlByName<TextBox>(GetFlowLayoutPanel(), targetProperty.Name);
                                    if (targetTextBox != null)
                                    {
                                        targetTextBox.Text = valueStr;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (popupAttr.AllowMultipleRowSelection)
                        {

                        }
                        else
                        {
                            // 根据Return字段指定的列名获取值
                            string returnColumn = popupAttr.Return;
                            var firstItem = selectedItems[0];
                            if (firstItem.ContainsKey(returnColumn) && firstItem[returnColumn] != null)
                            {
                                textBox.Text = firstItem[returnColumn].ToString();
                            }
                        }
                    }
                }

                // 释放弹窗资源
                popupForm.Dispose();
            });
        }

        /// <summary>
        /// 当用户需要导入数据时，打开弹窗，根据PopupAttribute配置创建弹窗并显示数据，类似TextBox_OpenPopup方法
        /// </summary>
        protected void Import_OpenPopup()
        {
            // 在整个表单中递归查找TabControl
            TabControl tabControl = UV.FindControlRecursive<TabControl>(this);
            if (tabControl == null || tabControl.SelectedTab == null)
            {
                MessageBox.Show("未找到TabControl或未选中TabPage", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            TabPage currentPage = tabControl.SelectedTab;
            string currentTabTitle = currentPage.Text;

            if (string.IsNullOrEmpty(currentTabTitle))
            {
                MessageBox.Show("当前TabPage标题为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 从当前TabPage中查找DataGridView控件
            DataGridView dataGridView = UV.FindDataGridViewInTabPage(currentPage);
            if (dataGridView == null)
            {
                MessageBox.Show($"在TabPage '{currentTabTitle}' 中未找到DataGridView控件", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 获取 Bom 类的所有属性
            var type = typeof(T);
            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                // 检查是否包含 OneToMany 特性
                var oneToManyAttr = property.GetCustomAttributes(typeof(OneToManyAttribute), false).FirstOrDefault() as OneToManyAttribute;
                if (oneToManyAttr == null)
                {
                    continue;
                }

                // 判断 OneToMany 的 Title 是否与当前 Tab 标题一致
                if (!string.Equals(oneToManyAttr.Title, currentTabTitle, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // 判断是否有 PopupAttribute 特性
                var popupAttr = property.GetCustomAttributes(typeof(PopupAttribute), false).FirstOrDefault() as PopupAttribute;
                if (popupAttr == null)
                {
                    continue;
                }

                OpenPopup(popupAttr, (popupForm) =>
                {
                    Type popupType = popupAttr.Type;
                    List<Dictionary<string, object>> selectedItems = new List<Dictionary<string, object>>();

                    // 尝试从弹窗获取选中的数据
                    MethodInfo getSelectedItemsMethod = popupType.GetMethod("GetSelectedItems");
                    if (getSelectedItemsMethod != null)
                    {
                        string jsonString = getSelectedItemsMethod.Invoke(popupForm, null) as string;
                        selectedItems = JsonHelper.toObject<List<Dictionary<string, object>>>(jsonString);
                    }

                    // 如果有选中的数据，添加到DataGridView中
                    if (selectedItems != null && selectedItems.Count > 0)
                    {
                        // 检查 DataGridView 的数据源类型
                        object dataSource = dataGridView.DataSource;
                        DataTable dataTable = null;
                        IList listDataSource = null;
                        Type listElementType = null;

                        if (dataSource is DataTable dt)
                        {
                            // 数据源是 DataTable
                            dataTable = dt;
                        }
                        else if (dataSource is IList list && list.Count > 0)
                        {
                            // 数据源是 List<T>
                            listDataSource = list;
                            // 获取列表元素的类型
                            listElementType = list[0].GetType();
                        }

                        // 判断是否返回多列
                        if (popupAttr.IsMultipleColumnReturn)
                        {
                            // 判断是否返回多行
                            if (popupAttr.AllowMultipleRowSelection)
                            {
                                // 多行多列：将所有选中的行添加到DataGridView
                                foreach (var item in selectedItems)
                                {
                                    UV.AddItemToDataGridView(dataGridView, dataTable, listDataSource, listElementType, item, popupAttr.Return);
                                }
                            }
                            else
                            {
                                // 单行多列：只添加第一行
                                var firstItem = selectedItems[0];
                                UV.AddItemToDataGridView(dataGridView, dataTable, listDataSource, listElementType, firstItem, popupAttr.Return);
                            }
                        }
                        else
                        {
                            // 判断是否返回多行
                            if (popupAttr.AllowMultipleRowSelection)
                            {
                                // 多行单列：将所有选中的行的指定列值添加到DataGridView
                                foreach (var item in selectedItems)
                                {
                                    UV.AddItemToDataGridView(dataGridView, dataTable, listDataSource, listElementType, item, popupAttr.Return);
                                }
                            }
                            else
                            {
                                // 单行单列：只添加第一行的指定列值
                                var firstItem = selectedItems[0];
                                UV.AddItemToDataGridView(dataGridView, dataTable, listDataSource, listElementType, firstItem, popupAttr.Return);
                            }
                        }
                    }

                    // 释放弹窗资源
                    popupForm.Dispose();
                });
            }
        }

        /// <summary>
        /// 过滤 dataGridView 中已存在的数据（取差集）
        /// </summary>
        /// <param name="dataGridView"></param>
        /// <param name="queryResult">查询结果列表</param>
        /// <returns>过滤后的查询结果</returns>
        private List<Dictionary<string, object>> FilterOneToManyExistingData(DataGridView dataGridView, PopupAttribute popupAttr, List<Dictionary<string, object>> queryResult)
        {
            if (dataGridView == null || queryResult == null || queryResult.Count == 0)
                return queryResult;

            try
            {
                // 查找触发弹窗的属性
                PropertyInfo targetProperty = null;
                Type entityType = typeof(T);

                foreach (var prop in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var attr = Attribute.GetCustomAttribute(prop, typeof(PopupAttribute)) as PopupAttribute;
                    if (attr != null && attr.Name == popupAttr.Name)
                    {
                        targetProperty = prop;
                        break;
                    }
                }

                if (targetProperty == null)
                    return queryResult;

                // 检查该属性是否被 OneToMany 特性修饰
                var oneToManyAttr = Attribute.GetCustomAttribute(targetProperty, typeof(OneToManyAttribute)) as OneToManyAttribute;

                if (oneToManyAttr == null)
                    return queryResult;

                // 获取子实体类型
                Type childType = oneToManyAttr.ChildType;

                // 获取 T 类型中所有被 Keyword 特性修饰的属性
                List<PropertyInfo> keywordProperties = new List<PropertyInfo>();
                foreach (var prop in childType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var keywordAttr = Attribute.GetCustomAttribute(prop, typeof(KeywordAttribute)) as KeywordAttribute;
                    if (keywordAttr != null)
                    {
                        keywordProperties.Add(prop);
                    }
                }

                // 如果没有找到 Keyword 属性，默认使用 FID
                if (keywordProperties.Count == 0)
                {
                    var fidProp = childType.GetProperty("FID");
                    if (fidProp != null)
                    {
                        keywordProperties.Add(fidProp);
                    }
                    else
                    {
                        Console.WriteLine($"实体 {childType.Name} 中没有找到 Keyword 属性或 FID 属性");
                        return queryResult;
                    }
                }

                // 从 DataGridView 行数据中收集已存在数据的键值集合（使用复合键）
                HashSet<string> existingKeys = new HashSet<string>();
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    // 跳过新行
                    if (row.IsNewRow)
                        continue;

                    StringBuilder keyBuilder = new StringBuilder();
                    bool hasValue = false;

                    foreach (var keyProp in keywordProperties)
                    {
                        string propName = keyProp.Name;

                        // 尝试从 FieldAttribute 获取列名
                        var fieldAttr = Attribute.GetCustomAttribute(keyProp, typeof(FieldAttribute)) as FieldAttribute;
                        string columnName = fieldAttr != null && !string.IsNullOrEmpty(fieldAttr.Name) ? fieldAttr.Name : propName;

                        // 优先使用 Field 名称，其次使用属性名，安全获取单元格值
                        object cellValue = GetDataGridViewCellValue(row, columnName);
                        if (cellValue == null)
                        {
                            cellValue = GetDataGridViewCellValue(row, propName);
                        }

                        if (cellValue != null)
                        {
                            if (keyBuilder.Length > 0)
                                keyBuilder.Append("|");
                            keyBuilder.Append(cellValue.ToString());
                            hasValue = true;
                        }
                    }

                    if (hasValue)
                    {
                        existingKeys.Add(keyBuilder.ToString());
                    }
                }

                // 过滤 queryResult，移除已存在的项（取差集）
                if (existingKeys.Count > 0)
                {
                    queryResult = queryResult.Where(dict =>
                    {
                        StringBuilder dictKeyBuilder = new StringBuilder();
                        bool hasValue = false;

                        foreach (var keyProp in keywordProperties)
                        {
                            string propName = keyProp.Name;

                            var fieldAttr = Attribute.GetCustomAttribute(keyProp, typeof(FieldAttribute)) as FieldAttribute;
                            string columnName = fieldAttr != null && !string.IsNullOrEmpty(fieldAttr.Name) ? fieldAttr.Name : propName;

                            if (dict.ContainsKey(columnName))
                            {
                                object dictKeyValue = dict[columnName];
                                if (dictKeyValue != null)
                                {
                                    if (dictKeyBuilder.Length > 0)
                                        dictKeyBuilder.Append("|");
                                    dictKeyBuilder.Append(dictKeyValue.ToString());
                                    hasValue = true;
                                }
                            }
                            else if (dict.ContainsKey(propName))
                            {
                                object dictKeyValue = dict[propName];
                                if (dictKeyValue != null)
                                {
                                    if (dictKeyBuilder.Length > 0)
                                        dictKeyBuilder.Append("|");
                                    dictKeyBuilder.Append(dictKeyValue.ToString());
                                    hasValue = true;
                                }
                            }
                        }

                        if (!hasValue)
                            return true;

                        // 如果字典中的复合键存在于现有列表中，则排除该项
                        return !existingKeys.Contains(dictKeyBuilder.ToString());
                    }).ToList();
                }

                return queryResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"过滤 DataGridView 中已存在的数据时出错: {ex.Message}");
                return queryResult;
            }
        }

        /// <summary>
        /// 安全获取 DataGridView 单元格的值，列名不存在时返回 null
        /// </summary>
        /// <param name="row">DataGridView 行</param>
        /// <param name="columnName">列名</param>
        /// <returns>单元格值，列不存在或值为 null 时返回 null</returns>
        private object GetDataGridViewCellValue(DataGridViewRow row, string columnName)
        {
            if (row == null || string.IsNullOrEmpty(columnName))
                return null;

            // 检查列是否存在，避免 ArgumentException
            if (row.DataGridView.Columns.Contains(columnName))
            {
                return row.Cells[columnName].Value;
            }

            return null;
        }

        /// <summary>
        /// 过滤 OneToMany 属性中已存在的数据（取差集）
        /// </summary>
        /// <param name="popupAttr">弹窗特性</param>
        /// <param name="queryResult">查询结果列表</param>
        /// <returns>过滤后的查询结果</returns>
        private List<Dictionary<string, object>> FilterOneToManyExistingData(PopupAttribute popupAttr, List<Dictionary<string, object>> queryResult)
        {
            if (popupAttr == null || queryResult == null || queryResult.Count == 0)
                return queryResult;

            try
            {
                // 查找触发弹窗的属性
                PropertyInfo targetProperty = null;
                Type entityType = typeof(T);
                
                foreach (var prop in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var attr = Attribute.GetCustomAttribute(prop, typeof(PopupAttribute)) as PopupAttribute;
                    if (attr != null && attr.Name == popupAttr.Name)
                    {
                        targetProperty = prop;
                        break;
                    }
                }

                if (targetProperty == null)
                    return queryResult;

                // 检查该属性是否被 OneToMany 特性修饰
                var oneToManyAttr = Attribute.GetCustomAttribute(targetProperty, typeof(OneToManyAttribute)) as OneToManyAttribute;
                
                if (oneToManyAttr == null)
                    return queryResult;

                // 获取当前记录中该属性的现有数据列表
                var existingList = targetProperty.GetValue(this.record) as IList;
                
                if (existingList == null || existingList.Count == 0)
                    return queryResult;

                // 获取子实体类型
                Type childType = oneToManyAttr.ChildType;
                
                // 获取 childType 中所有被 Keyword 特性修饰的属性
                List<PropertyInfo> keywordProperties = new List<PropertyInfo>();
                foreach (var prop in childType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var keywordAttr = Attribute.GetCustomAttribute(prop, typeof(KeywordAttribute)) as KeywordAttribute;
                    if (keywordAttr != null)
                    {
                        keywordProperties.Add(prop);
                    }
                }

                // 如果没有找到 Keyword 属性，默认使用 FID
                if (keywordProperties.Count == 0)
                {
                    var fidProp = childType.GetProperty("FID");
                    if (fidProp != null)
                    {
                        keywordProperties.Add(fidProp);
                    }
                    else
                    {
                        Console.WriteLine($"子实体 {childType.Name} 中没有找到 Keyword 属性或 FID 属性");
                        return queryResult;
                    }
                }
                
                // 收集现有数据的键值集合（使用复合键）
                HashSet<string> existingKeys = new HashSet<string>();
                foreach (var item in existingList)
                {
                    // 构建复合键字符串
                    StringBuilder keyBuilder = new StringBuilder();
                    bool hasValue = false;
                    
                    foreach (var keyProp in keywordProperties)
                    {
                        var keyValue = keyProp.GetValue(item);
                        if (keyValue != null)
                        {
                            if (keyBuilder.Length > 0)
                                keyBuilder.Append("|");
                            keyBuilder.Append(keyValue.ToString());
                            hasValue = true;
                        }
                    }
                    
                    if (hasValue)
                    {
                        existingKeys.Add(keyBuilder.ToString());
                    }
                }

                // 过滤 queryResult，移除已存在的项（取差集）
                if (existingKeys.Count > 0)
                {
                    queryResult = queryResult.Where(dict => 
                    {
                        // 构建字典的复合键字符串
                        StringBuilder dictKeyBuilder = new StringBuilder();
                        bool hasValue = false;
                        
                        foreach (var keyProp in keywordProperties)
                        {
                            string propName = keyProp.Name;
                            
                            // 尝试从 FieldAttribute 获取列名
                            var fieldAttr = Attribute.GetCustomAttribute(keyProp, typeof(FieldAttribute)) as FieldAttribute;
                            string columnName = fieldAttr != null && !string.IsNullOrEmpty(fieldAttr.Name) ? fieldAttr.Name : propName;
                            
                            // 优先使用 Field 名称，其次使用属性名
                            if (dict.ContainsKey(columnName))
                            {
                                object dictKeyValue = dict[columnName];
                                if (dictKeyValue != null)
                                {
                                    if (dictKeyBuilder.Length > 0)
                                        dictKeyBuilder.Append("|");
                                    dictKeyBuilder.Append(dictKeyValue.ToString());
                                    hasValue = true;
                                }
                            }
                            else if (dict.ContainsKey(propName))
                            {
                                object dictKeyValue = dict[propName];
                                if (dictKeyValue != null)
                                {
                                    if (dictKeyBuilder.Length > 0)
                                        dictKeyBuilder.Append("|");
                                    dictKeyBuilder.Append(dictKeyValue.ToString());
                                    hasValue = true;
                                }
                            }
                        }
                        
                        // 如果字典中没有键字段值，保留该项
                        if (!hasValue)
                            return true;
                        
                        // 如果字典中的复合键存在于现有列表中，则排除该项
                        return !existingKeys.Contains(dictKeyBuilder.ToString());
                    }).ToList();
                }

                return queryResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理 OneToMany 属性的差集逻辑时出错: {ex.Message}");
                // 出错时返回原始 queryResult，避免阻断流程
                return queryResult;
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // EditPopup
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "EditPopup";
            this.Load += new System.EventHandler(this.EditPopup_Load);
            this.ResumeLayout(false);

        }
    }
}
