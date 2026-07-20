using com.jiuhuan.plan.domain;
using com.jiuhuan.plan.models;
using com.jiuhuan.plan.tools;
using QFramework;
using System;
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
    public partial class ScheduleDetailsResultsForm : GridViewForm<ScheduleDetailsRecord>
    {
        public ScheduleDetailsResultsForm()
        {
            InitializeComponent();
        }

        private void ScheduleDetailsResultsForm_Load(object sender, EventArgs e)
        {
            InitializeData();

            dateTimePicker1.Value = DateTime.Now;
            dateTimePicker2.Value = DateTime.Now.AddMonths(1);

            comboBox1.SelectedIndex = 0;
        }

        protected override string Sql()
        {
            return Config.Default.v_schedule_details_record_findall;
        }

        protected override SplitContainer GetSplitContainer()
        {
            return splitContainer1;
        }

        protected override DataGridView GetDataGridView1()
        {
            return dataGridView1;
        }

        protected override DataGridView GetDataGridView2()
        {
            return dataGridView2;
        }

        protected override void FilterBills1()
        {
            // 获取用户选择的参数
            DateTime startDate = dateTimePicker1.Value.Date;
            DateTime endDate = dateTimePicker2.Value.Date;
            string selectedStatus = comboBox1.SelectedItem?.ToString() ?? "";
            string queryText = textBox1.Text.Trim();

            // 过滤条件
            filteredRecords1 = selectedRecords.Where(bill =>
                bill.FOperationPlanStartDate >= startDate &&
                bill.FOperationPlanStartDate <= endDate &&
                (string.IsNullOrEmpty(selectedStatus) || "全部".Equals(selectedStatus) || bill.FState == selectedStatus) &&
                (string.IsNullOrEmpty(queryText) ||
                bill.FBillNo.Contains(queryText) ||
                bill.FNumber.Contains(queryText) ||
                bill.FName.Contains(queryText) ||
                bill.FModel.Contains(queryText))
            ).ToList();
        }

        /// <summary>
        /// 验证合法性
        /// </summary>
        protected override bool ValidateRecord()
        {
            bool validate = true;

            foreach (var record in selectedRecords)
                record.RowBackColor = Color.LightGreen;

            // 筛选出缺料的记录并标记为红色
            foreach (var record in selectedRecords.Where(r => r.FLack))
            {
                record.RowBackColor = Color.Red;
                validate = false;
            }

            // 有漏排的记录并标记为黄色
            foreach (var record in selectedRecords.Where(r => 
                r.FMoPlanCompleteDate >= DateTime.Today && 
                r.FMoPlanCompleteDate <= DateTime.Today.AddDays(1) &&
                (r.FClasses == null || !r.FClasses.Any())))
            {
                record.RowBackColor = Color.Yellow;
                validate = false;
            }

            UV.RefreshDataGridViewBackColor(dataGridView1);

            return validate;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Query1();
        }

        private void 加载ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void 查询ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Query1();
        }

        private void 恢复底稿ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReloadRecords();
        }

        private void 清空ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clear1();
        }

        private void 删除选中行ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteSelectedRows();
        }

        private void 导出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Export();
        }

        private void 保存到数据库ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void 分屏ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowExtraGridView();
        }

        private void 重置ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private void 设备日历ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ValidateRecord();
        }

        /// <summary>
        /// 删除选中行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            DeleteSelectedRows();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            设备日历ToolStripMenuItem_Click(sender, e);
        }

        /// <summary>
        /// 双击dataGridView1时，创建并显示ScheduleDetailsRecordEditForm，将选中行的ScheduleDetailsRecord数据填充到对应控件
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            CreateEditForm<ScheduleDetailsRecordEditForm2>(sender, e);

            //// 验证点击位置是否有效
            //if (e.RowIndex < 0 || e.ColumnIndex < 0)
            //{
            //    return;
            //}

            //DataGridView dataGridView = sender as DataGridView;
            //if (dataGridView == null)
            //{
            //    return;
            //}

            //// 获取选中的行
            //DataGridViewRow selectedRow = dataGridView.Rows[e.RowIndex];

            //// 获取该行的数据对象
            //var scheduleDetailsRecord = selectedRow.DataBoundItem as ScheduleDetailsRecord;

            //// 验证数据对象是否存在
            //if (scheduleDetailsRecord == null)
            //{
            //    MessageBox.Show("无法获取工序记录数据，请重试。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            //// 创建编辑窗体实例
            //using (var editForm = new ScheduleDetailsRecordEditForm2())
            //{
            //    // 设置编辑窗体的数据源
            //    editForm.SetRecord(scheduleDetailsRecord);

            //    // 显示编辑窗体（模态对话框）
            //    DialogResult result = editForm.ShowDialog();

            //    // 根据用户操作决定是否更新数据源
            //    if (result == DialogResult.OK)
            //    {
            //        // 更新原始数据
            //        UpdateDataSourceWithEditedRecord(scheduleDetailsRecord);

            //        // 刷新DataGridView显示
            //        UpdateUI();
            //    }
            //}
        }

        private void 生成计划ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 创建窗体实例
            using (var editForm = new ToScheduleFinalRecordForm())
            {
                // 显示窗体（模态对话框）
                DialogResult result = editForm.ShowDialog();

                // 根据用户操作决定是否更新数据源
                if (result == DialogResult.OK)
                {
                    var begin = editForm.GetBeginTime();
                    var end = editForm.GetEndTime();
                    var classes = editForm.GetClasses();

                    // 根据时间段筛选记录
                    var recordsInPeriod = selectedRecords.Where(r =>
                    r.FOperationPlanStartDate >= begin &&
                    r.FOperationPlanStartDate <= end &&
                    r.FClasses != null && 
                    r.FClasses.Keys.Any(key => "全部".Equals(classes) || key.Equals(classes))).ToList();

                    // 转换为最终计划记录
                    var finalRecords = ConvertToScheduleFinalRecords(recordsInPeriod, begin, end, classes);

                    var sql = string.Format(Config.Default.sql_schedule_final_record_deleteall, begin.Date.ToString("yyyy-MM-dd"), end.Date.ToString("yyyy-MM-dd"), user.FName, "全部".Equals(classes) ? "" : classes);
                    DaoTemplate.ExecuteNonQuery(sql);

                    UV.SaveAsync(finalRecords, this, () => {
                        var form = OpenForm<ScheduleFinalResultsForm>("ScheduleFinalResultsForm", "生产计划");
                        form.LoadData();
                    });
                }
            }
        }

        /// <summary>
        /// 根据提供的时间段将ScheduleDetailsRecord转换为ScheduleFinalRecord列表
        /// </summary>
        /// <param name="detailRecords">ScheduleDetailsRecord记录列表</param>
        /// <param name="begin">时间段开始时间</param>
        /// <param name="end">时间段结束时间</param>
        /// <returns>ScheduleFinalRecord记录列表</returns>
        private List<ScheduleFinalRecord> ConvertToScheduleFinalRecords(List<ScheduleDetailsRecord> detailRecords, DateTime begin, DateTime end, string classes)
        {
            var finalRecords = new List<ScheduleFinalRecord>();

            foreach (var detailRecord in detailRecords)
            {
                // 检查记录是否在指定时间段内
                if (detailRecord.FOperationPlanStartDate < begin || detailRecord.FOperationPlanStartDate > end)
                    continue;

                // 检查FClasses是否为空或者没有元素
                if (detailRecord.FClasses == null || detailRecord.FClasses.Count == 0)
                {
                    continue;
                }
                else if (detailRecord.FClasses.Count == 1)
                {
                    // 只有一个班次，直接复制
                    var finalRecord = CreateBaseScheduleFinalRecord(detailRecord);
                    var kvp = detailRecord.FClasses.First();
                    finalRecord.FClasses = kvp.Key; // 班次
                    finalRecord.FNeedWorkHours = kvp.Value; // 对应工时
                    finalRecords.Add(finalRecord);
                }
                else
                {
                    // 多个班次，拆分为多个记录
                    foreach (var kvp in detailRecord.FClasses)
                    {
                        var finalRecord = CreateBaseScheduleFinalRecord(detailRecord);
                        finalRecord.FClasses = kvp.Key; // 班次
                        finalRecord.FNeedWorkHours = kvp.Value; // 对应工时
                        finalRecord.FRemainQty = (int)(finalRecord.FNeedWorkHours / detailRecord.FStandardWorkHours);  //待加工数
                        
                        if(kvp.Key.Equals(classes))
                            finalRecords.Add(finalRecord);
                    }
                }
            }

            return finalRecords;
        }

        /// <summary>
        /// 根据ScheduleDetailsRecord创建基础的ScheduleFinalRecord（不包括班次和工时）
        /// </summary>
        /// <param name="detailRecord">源ScheduleDetailsRecord</param>
        /// <returns>基础ScheduleFinalRecord</returns>
        private ScheduleFinalRecord CreateBaseScheduleFinalRecord(ScheduleDetailsRecord detailRecord)
        {
            var singleRecord = new ScheduleFinalRecord();
            CopyProperties(detailRecord, singleRecord);
            return singleRecord;

            //return new ScheduleFinalRecord
            //{
            //    // 从父类OperationRecord继承的属性
            //    FID = detailRecord.FID,
            //    FUser = detailRecord.FUser,
            //    FOperationNumber = detailRecord.FOperationNumber,
            //    FOperationName = detailRecord.FOperationName,
            //    FDispatchedQty = detailRecord.FDispatchedQty,
            //    FCollectQty = detailRecord.FCollectQty,
            //    FNeedWorkHours = detailRecord.FNeedWorkHours, // 默认值，后续会被覆盖
            //    FOperationPlanStartDate = detailRecord.FOperationPlanStartDate,
            //    FOperationPlanCompleteDate = detailRecord.FOperationPlanCompleteDate,
            //    FTimeout = detailRecord.FTimeout,
            //    FMachineTeam = detailRecord.FMachineTeam,
            //    FBillNo = detailRecord.FBillNo,
            //    FNumber = detailRecord.FNumber,
            //    FName = detailRecord.FName,
            //    FModel = detailRecord.FModel,
            //    FState = detailRecord.FState,
            //    FMoPlanCompleteDate = detailRecord.FMoPlanCompleteDate,
            //    FQty = detailRecord.FQty,
            //    FCompletedQty = detailRecord.FCompletedQty,
            //    FCustomerCode = detailRecord.FCustomerCode,
            //    FCustomerContractNo = detailRecord.FCustomerContractNo,
            //    FRemark = detailRecord.FRemark,
            //    FMoCreateDate = detailRecord.FMoCreateDate,
            //    FWorkCenter = detailRecord.FWorkCenter,
            //    FAidedWorkHours = detailRecord.FAidedWorkHours,
            //    FStandardWorkHours = detailRecord.FStandardWorkHours,
            //    FRemainQty = detailRecord.FRemainQty,

            //    // ScheduleFinalRecord 特有属性
            //    FScheduleName = "default" // 可以根据需要设置默认值
            //};
        }

        /// <summary>
        /// 复制ScheduleDetailsRecord的属性到ScheduleFinalRecord
        /// </summary>
        /// <param name="source">源对象</param>
        /// <param name="target">目标对象</param>
        private static void CopyProperties(ScheduleDetailsRecord source, ScheduleFinalRecord target)
        {
            // 使用反射获取所有公共属性并复制
            var properties = typeof(ScheduleDetailsRecord).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                // 跳过只读属性和特殊标记的属性
                if (property.Name == "FID" || property.Name == "FClasses" ||
                    property.Name == "FClasses2")
                {
                    continue;
                }

                // 获取源对象的属性值
                var value = property.GetValue(source);

                // 在目标对象中查找同名属性
                var targetProperty = typeof(ScheduleFinalRecord).GetProperty(property.Name);
                if (targetProperty != null && targetProperty.CanWrite)
                {
                    targetProperty.SetValue(target, value);
                }
            }
        }

        private void 生成班次信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GenerateClassesInformation();
        }

        /// <summary>
        /// 生成班次信息
        /// 根据FNeedWorkHours生成FClasses，班次按早、中、夜顺序分配
        /// </summary>
        private void GenerateClassesInformation()
        {
            // 获取需要生成班次信息的记录
            var recordsToProcess = filteredRecords1.Any() ? filteredRecords1 : selectedRecords;

            if (!recordsToProcess.Any())
            {
                MessageBox.Show("没有需要生成班次信息的记录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //清除已有班次信息
            foreach (var record in recordsToProcess)
                record.FClasses.Clear();

            // 获取排产工具类
            var scheduleUtility = this.GetUtility<IScheduleUtility>();

            // 定义班次顺序：早、中、夜
            var classesOrder = new List<string> { "早", "中", "夜" };

            foreach (var record in recordsToProcess)
            {
                // 初始化班次字典
                record.FClasses = new Dictionary<string, int>();

                // 剩余需要分配的工时
                int remainingHours = (int)record.FNeedWorkHours;

                // 按照班次顺序分配工时
                foreach (var className in classesOrder)
                {
                    // 如果已经分配完所有工时，则跳出循环
                    if (remainingHours <= 0)
                        break;

                    // 计算指定日期和班次的剩余产能
                    int remainingCapacity = 0;
                    try
                    {
                        remainingCapacity = scheduleUtility.CalculateRemainingCapacityForClasses(
                            workdays,
                            selectedRecords.OfType<ScheduleDetailsRecord>().ToList(),
                            record.FOperationPlanStartDate,
                            className,
                            record.FMachineTeam);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"计算班次 {className} 的剩余产能时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 如果该班次有剩余产能，则分配
                    if (remainingCapacity > 0)
                    {
                        // 分配工时：取剩余工时和剩余产能的最小值
                        int allocatedHours = Math.Min(remainingHours, remainingCapacity);

                        // 将分配的工时添加到班次字典中
                        record.FClasses[className] = allocatedHours;

                        // 更新剩余需要分配的工时
                        remainingHours -= allocatedHours;
                    }
                }

                // 如果还有未分配的工时，说明产能不足
                if (remainingHours > 0)
                {
                    MessageBox.Show($"工单 {record.FBillNo} 的工序 {record.FOperationNumber} 产能不足，仍有 {remainingHours} 工时无法分配！",
                        "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            // 保存
            //Save();

            // 刷新显示
            //Query1();
            GetDataGridView1().Refresh();

            //MessageBox.Show("班次信息生成完成！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 清除班次信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            var selectedRows = UV.GetSelectedRows<ScheduleDetailsRecord>(dataGridView1);

            if (selectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要删除的行！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return ;
            }

            var result = MessageBox.Show($"确定要删除选中的{selectedRows.Count}条数据吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                foreach (var item in selectedRows)
                {
                    item.FClasses.Clear();
                }
            }

            // 保存
            UV.SaveAsync(selectedRows, this);

            // 刷新显示
            //Query1();
            GetDataGridView1().Refresh();
        }

        private void 全选ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectAll();
        }

        /// <summary>
        /// 查看缺料明细
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            // 获取选中的行
            var selectedRows = UV.GetSelectedRows2<ScheduleDetailsRecord>(dataGridView1);

            // 检查是否有选中行
            if (selectedRows == null || selectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要查看缺料明细的行！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 获取第一个选中行的FBillNo
            var selectedRecord = selectedRows.FirstOrDefault();
            if (selectedRecord == null || string.IsNullOrEmpty(selectedRecord.FBillNo) || !selectedRecord.FLack)
            {
                MessageBox.Show("选中的记录是否缺料，请检查！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 打开MoInventoryStatusDetailsDataGridViewForm窗口并传入FBillNo参数
            using (var inventoryForm = new MoInventoryStatusDetailsDataGridViewForm())
            {
                inventoryForm.SetBillNo(selectedRecord.FBillNo);

                // 显示编辑窗体（模态对话框）
                DialogResult result = inventoryForm.ShowDialog();
            }
        }

        private void 显示隐藏列ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColumnSettings();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            GoToFirstPage();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            GoToPreviousPage();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GoToNextPage();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            GoToLastPage();
        }

        public override void UpdatePageInfo()
        {
            this.textBox2.Text = _currentPage.ToString();
            label3.Text = "/" + _totalPages;
            label6.Text = "总数:" + _totalCount;
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && this.textBox2.Text != "")
            {
                if (int.TryParse(textBox2.Text, out int pageNumber))
                {
                    GoToPage(pageNumber);
                }
            }
        }
    }
}