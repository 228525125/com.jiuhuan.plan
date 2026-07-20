using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace com.jiuhuan.plan
{
    /// <summary>
    /// SQL参数定义模型，对应JSON格式：{"name":"startdate","title":"起始日期","type":"datetime","format":"yyyy-MM-dd"}
    /// </summary>
    public class SqlParamDefinition
    {
        /// <summary>
        /// 参数名称，对应SQL中的占位符 {name}
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 参数显示标题，用于控件前面的标签文本
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// 参数类型：datetime、string、int、decimal等
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// 日期或数字格式，type为datetime时使用日期格式，type为decimal/int时可使用数字格式
        /// </summary>
        public string format { get; set; }
    }

    /// <summary>
    /// SQL参数输入弹窗，根据参数定义动态生成控件，获取用户输入
    /// </summary>
    public class SqlParameterDialog : Form
    {
        private List<SqlParamDefinition> _paramDefinitions;
        private Dictionary<string, Control> _paramControls = new Dictionary<string, Control>();
        private Dictionary<string, string> _paramValues = new Dictionary<string, string>();

        /// <summary>
        /// 获取用户输入的参数值字典，key为参数名，value为用户输入的格式化字符串
        /// </summary>
        public Dictionary<string, string> ParamValues
        {
            get { return _paramValues; }
        }

        /// <summary>
        /// 创建SQL参数输入弹窗
        /// </summary>
        /// <param name="paramDefinitions">参数定义列表</param>
        public SqlParameterDialog(List<SqlParamDefinition> paramDefinitions)
        {
            _paramDefinitions = paramDefinitions;
            InitializeComponent();
            CreateParamControls();
        }

        /// <summary>
        /// 初始化窗体基本属性
        /// </summary>
        private void InitializeComponent()
        {
            this.Text = "请输入查询参数";
            this.Size = new Size(400, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        /// <summary>
        /// 根据参数定义动态创建输入控件
        /// </summary>
        private void CreateParamControls()
        {
            // 使用TableLayoutPanel布局，两列：标签列 + 控件列
            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.RowCount = _paramDefinitions.Count + 1; // +1 为按钮行
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65f));
            tableLayoutPanel.Padding = new Padding(10);

            int row = 0;
            foreach (var paramDef in _paramDefinitions)
            {
                // 创建标签，优先使用title，无title时使用name
                Label label = new Label();
                label.Text = string.IsNullOrEmpty(paramDef.title) ? paramDef.name : paramDef.title;
                label.Dock = DockStyle.Fill;
                label.TextAlign = ContentAlignment.MiddleRight;
                label.Padding = new Padding(0, 5, 5, 5);
                tableLayoutPanel.Controls.Add(label, 0, row);

                // 根据参数类型创建对应控件
                Control inputControl = CreateInputControl(paramDef);
                inputControl.Dock = DockStyle.Fill;
                inputControl.Margin = new Padding(5);
                tableLayoutPanel.Controls.Add(inputControl, 1, row);

                _paramControls[paramDef.name] = inputControl;
                row++;
            }

            // 创建按钮面板
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel();
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Padding = new Padding(5);

            Button btnOk = new Button();
            btnOk.Text = "确定";
            btnOk.Size = new Size(80, 30);
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Click += BtnOk_Click;

            Button btnCancel = new Button();
            btnCancel.Text = "取消";
            btnCancel.Size = new Size(80, 30);
            btnCancel.DialogResult = DialogResult.Cancel;

            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.Controls.Add(btnOk);

            tableLayoutPanel.Controls.Add(buttonPanel, 0, row);
            tableLayoutPanel.SetColumnSpan(buttonPanel, 2);

            this.Controls.Add(tableLayoutPanel);

            // 根据参数数量调整窗体高度
            this.Height = 80 + _paramDefinitions.Count * 40;
        }

        /// <summary>
        /// 根据参数类型创建对应的输入控件
        /// </summary>
        /// <param name="paramDef">参数定义</param>
        /// <returns>对应的输入控件</returns>
        private Control CreateInputControl(SqlParamDefinition paramDef)
        {
            string paramType = paramDef.type?.ToLower() ?? "string";

            if (paramType == "datetime" || paramType == "date")
            {
                // 日期类型使用 DateTimePicker
                DateTimePicker dateTimePicker = new DateTimePicker();
                dateTimePicker.Format = DateTimePickerFormat.Custom;
                dateTimePicker.CustomFormat = string.IsNullOrEmpty(paramDef.format) ? "yyyy-MM-dd" : paramDef.format;
                dateTimePicker.Tag = paramDef; // 存储参数定义以便后续格式化
                return dateTimePicker;
            }
            else if (paramType == "decimal")
            {
                // 小数类型使用 TextBox，支持自定义格式
                TextBox textBox = new TextBox();
                return textBox;
            }
            else if (paramType == "int" || paramType == "integer")
            {
                // 整数类型使用 TextBox
                TextBox textBox = new TextBox();
                return textBox;
            }
            else if (paramType == "bool" || paramType == "boolean")
            {
                // 布尔类型使用 CheckBox
                CheckBox checkBox = new CheckBox();
                checkBox.Text = "是";
                checkBox.Dock = DockStyle.Fill;
                return checkBox;
            }
            else
            {
                // 默认使用 TextBox（字符串类型）
                TextBox textBox = new TextBox();
                return textBox;
            }
        }

        /// <summary>
        /// 确定按钮点击事件，收集用户输入的参数值
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void BtnOk_Click(object sender, EventArgs e)
        {
            _paramValues.Clear();

            foreach (var paramDef in _paramDefinitions)
            {
                Control control = _paramControls[paramDef.name];
                string value = GetControlValue(control, paramDef);
                _paramValues[paramDef.name] = value;
            }
        }

        /// <summary>
        /// 从控件中获取格式化后的参数值
        /// </summary>
        /// <param name="control">输入控件</param>
        /// <param name="paramDef">参数定义</param>
        /// <returns>格式化后的参数值字符串</returns>
        private string GetControlValue(Control control, SqlParamDefinition paramDef)
        {
            if (control is DateTimePicker dateTimePicker)
            {
                string format = string.IsNullOrEmpty(paramDef.format) ? "yyyy-MM-dd" : paramDef.format;
                return dateTimePicker.Value.ToString(format);
            }
            else if (control is NumericUpDown numericUpDown)
            {
                return numericUpDown.Value.ToString();
            }
            else if (control is CheckBox checkBox)
            {
                return checkBox.Checked ? "1" : "0";
            }
            else if (control is TextBox textBox)
            {
                string value = textBox.Text;
                // 对decimal类型按format格式化
                if (paramDef.type?.ToLower() == "decimal" && !string.IsNullOrEmpty(paramDef.format))
                {
                    if (decimal.TryParse(value, out decimal decValue))
                    {
                        return decValue.ToString(paramDef.format);
                    }
                }
                return value;
            }

            return string.Empty;
        }
    }
}
