using com.jiuhuan.plan.domain;
using com.jiuhuan.plan.model;
using com.jiuhuan.plan.models;
using com.jiuhuan.plan.tools;
using QFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.jiuhuan.plan
{
    public partial class ScheduleResultsForm : GridViewForm<ScheduleRecord>
    {
        public ScheduleResultsForm()
        {
            InitializeComponent();
        }

        private void ScheduleResultsForm_Load(object sender, EventArgs e)
        {
            InitializeData();

            dateTimePicker1.Value = DateTime.Now;
            dateTimePicker2.Value = DateTime.Now.AddMonths(1);

            comboBox1.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
        }

        protected override string Sql()
        {
            return Config.Default.v_schedule_record_findall;
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
            string selectedStatus2 = comboBox4.SelectedItem?.ToString() ?? "";
            string queryText = textBox1.Text.Trim();

            // 过滤条件
            filteredRecords1 = selectedRecords.Where(bill =>
                bill.FOperationPlanStartDate >= startDate &&
                bill.FOperationPlanStartDate <= endDate &&
                (string.IsNullOrEmpty(selectedStatus) || "全部".Equals(selectedStatus) || bill.FFixed == selectedStatus.Equals("固定")) &&
                (string.IsNullOrEmpty(selectedStatus2) || "全部".Equals(selectedStatus2) || bill.FState == selectedStatus2) &&
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

            foreach (var record in selectedRecords.Where(r => r.FPlanCapacityOccupation == null || !r.FPlanCapacityOccupation.Any() || r.FStandardWorkHours == 0))
            {
                record.RowBackColor = Color.Red;
                validate = false;
            }

            UV.RefreshDataGridViewBackColor(dataGridView1);

            return validate;
        }

        private void 加载ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Query1();
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

        private void 保存到数据库ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void 重置ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private void 开始排产ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool allValid = ValidateRecord();

            // 如果有不是LightGreen的行，提示用户
            if (!allValid)
            {
                MessageBox.Show("有部分行异常，请检查！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // 提前结束方法执行
            }

            DaoTemplate.DeleteAll<ScheduleDetailsRecord>(user.FName);
            List<ScheduleDetailsRecord> list = new List<ScheduleDetailsRecord>();

            foreach (var record in selectedRecords)
            {
                var scheduleDetailsReocrds = ConvertToScheduleDetailsRecords(record);

                list.AddRange(scheduleDetailsReocrds);
            }

            UV.SaveAsync(list, this, () => {
                var form = OpenForm<ScheduleDetailsResultsForm>("ScheduleDetailsResultsForm", "排产结果明细");
                form.LoadData();
            });
        }

        /// <summary>
        /// 双击dataGridView1时，创建并显示ScheduleRecordEditForm，将选中行的ScheduleRecord数据填充到对应控件
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            CreateEditForm<ScheduleRecordEditForm2>(sender, e);

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
            //var scheduleRecord = selectedRow.DataBoundItem as ScheduleRecord;

            //// 验证数据对象是否存在
            //if (scheduleRecord == null)
            //{
            //    MessageBox.Show("无法获取工序记录数据，请重试。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            //// 创建编辑窗体实例
            //using (var editForm = new ScheduleRecordEditForm2())
            //{
            //    // 设置编辑窗体的数据源
            //    editForm.SetRecord(scheduleRecord);

            //    // 显示编辑窗体（模态对话框）
            //    DialogResult result = editForm.ShowDialog();

            //    // 根据用户操作决定是否更新数据源
            //    if (result == DialogResult.OK)
            //    {
            //        // 更新原始数据
            //        UpdateDataSourceWithEditedRecord(scheduleRecord);

            //        // 刷新DataGridView显示
            //        UpdateUI();
            //    }
            //}
        }

        /// <summary>
        /// 将ScheduleRecord转换为多个ScheduleDetailsRecord，实现1对多的关系
        /// </summary>
        /// <param name="scheduleRecord">源ScheduleRecord对象</param>
        /// <returns>转换后的ScheduleDetailsRecord列表</returns>
        private static List<ScheduleDetailsRecord> ConvertToScheduleDetailsRecords(ScheduleRecord scheduleRecord)
        {
            if (scheduleRecord == null)
            {
                throw new ArgumentNullException(nameof(scheduleRecord), "ScheduleRecord不能为null");
            }

            var result = new List<ScheduleDetailsRecord>();

            // 检查FPlanCapacityOccupation是否为空或null
            if (scheduleRecord.FPlanCapacityOccupation == null ||
                !scheduleRecord.FPlanCapacityOccupation.Any())
            {
                // 如果没有产能占用数据
                MessageBox.Show($"{scheduleRecord.FBillNo}没有分配到产能，请检查！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return result;
            }

            // 根据FPlanCapacityOccupation的数量决定处理方式
            if (scheduleRecord.FPlanCapacityOccupation.Count == 1)
            {
                // 如果只有一个日期的产能占用，直接复制所有属性
                var singleRecord = new ScheduleDetailsRecord();
                CopyProperties(scheduleRecord, singleRecord);

                result.Add(singleRecord);
            }
            else
            {
                // 如果有多个日期的产能占用，按每个日期拆分成多个记录
                foreach (var kvp in scheduleRecord.FPlanCapacityOccupation)
                {
                    var detailsRecord = new ScheduleDetailsRecord();
                    CopyProperties(scheduleRecord, detailsRecord);

                    // 设置开工和完工时间
                    detailsRecord.FOperationPlanStartDate = kvp.Key;
                    detailsRecord.FOperationPlanCompleteDate = kvp.Key;
                    detailsRecord.FNeedWorkHours = kvp.Value;
                    detailsRecord.FRemainQty = (int)(detailsRecord.FNeedWorkHours / scheduleRecord.FStandardWorkHours);  //待加工数

                    result.Add(detailsRecord);
                }
            }

            return result;
        }

        /// <summary>
        /// 复制ScheduleRecord的属性到ScheduleDetailsRecord
        /// </summary>
        /// <param name="source">源对象</param>
        /// <param name="target">目标对象</param>
        private static void CopyProperties(ScheduleRecord source, ScheduleDetailsRecord target)
        {
            // 使用反射获取所有公共属性并复制
            var properties = typeof(ScheduleRecord).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                // 跳过只读属性和特殊标记的属性
                if (property.Name == "FID" || property.Name == "FPlanCapacityOccupation" ||
                    property.Name == "FPlanCapacityOccupation2")
                {
                    continue;
                }

                // 获取源对象的属性值
                var value = property.GetValue(source);

                // 在目标对象中查找同名属性
                var targetProperty = typeof(ScheduleDetailsRecord).GetProperty(property.Name);
                if (targetProperty != null && targetProperty.CanWrite)
                {
                    targetProperty.SetValue(target, value);
                }
            }
        }

        /// <summary>
        /// 将编辑后的ScheduleRecord更新到数据源中
        /// </summary>
        /// <param name="editedRecord">编辑后的工单记录</param>
        /*private void UpdateDataSourceWithEditedRecord(ScheduleRecord editedRecord)
        {
            // 在实际应用中，这里应该根据具体的业务逻辑来更新数据源
            // 例如：在selectedRecords列表中找到对应的记录并更新
            var recordToUpdate = selectedRecords.FirstOrDefault(r => r.FID == editedRecord.FID);

            if (recordToUpdate != null)
            {
                // 使用属性复制或手动赋值方式更新记录
                //recordToUpdate.FSequenceNumber = editedRecord.FSequenceNumber;
                //recordToUpdate.FOperationNumber = editedRecord.FOperationNumber;
                //recordToUpdate.FOperationName = editedRecord.FOperationName;
                //recordToUpdate.FDispatchedQty = editedRecord.FDispatchedQty;
                //recordToUpdate.FCollectQty = editedRecord.FCollectQty;
                //recordToUpdate.FNeedWorkHours = editedRecord.FNeedWorkHours;
                //recordToUpdate.FOperationPlanStartDate = editedRecord.FOperationPlanStartDate;
                //recordToUpdate.FOperationPlanCompleteDate = editedRecord.FOperationPlanCompleteDate;
                recordToUpdate.FPlanCapacityOccupation = editedRecord.FPlanCapacityOccupation;
                //recordToUpdate.FTimeout = editedRecord.FTimeout;
                //recordToUpdate.FMachineTeam = editedRecord.FMachineTeam;
                //recordToUpdate.FBillNo = editedRecord.FBillNo;
                //recordToUpdate.FNumber = editedRecord.FNumber;
                //recordToUpdate.FName = editedRecord.FName;
                //recordToUpdate.FModel = editedRecord.FModel;
                //recordToUpdate.FState = editedRecord.FState;
                //recordToUpdate.FMoPlanCompleteDate = editedRecord.FMoPlanCompleteDate;
                //recordToUpdate.FQty = editedRecord.FQty;
                //recordToUpdate.FCompletedQty = editedRecord.FCompletedQty;
                //recordToUpdate.FCustomerCode = editedRecord.FCustomerCode;
                //recordToUpdate.FCustomerContractNo = editedRecord.FCustomerContractNo;
                //recordToUpdate.FRemark = editedRecord.FRemark;
                //recordToUpdate.FMoCreateDate = editedRecord.FMoCreateDate;
                //recordToUpdate.FWorkCenter = editedRecord.FWorkCenter;
                //recordToUpdate.FFixed = editedRecord.FFixed;
            }
        }*/

        
        private void 设备日历ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var wds = workdays.Where(w => w.FWorkDate >= DateTime.Today).ToList();
            
            // 将workdays按照机器和日期分组，并计算每天的总工作时长
            var groupedWorkdays = wds
                .GroupBy(w => new {w.FMachineTeam, w.FWorkDate })
                .Select(g => new Workday
                {
                    FMachineTeam = g.Key.FMachineTeam,
                    FWorkDate = g.Key.FWorkDate,
                    FDuration = g.Sum(w => w.FDuration)
                })
                .ToList();

            // 创建并初始化窗口
            var calendarForm = new MachineCapacityCalendarForm();
            calendarForm.Initialize(groupedWorkdays, selectedRecords);
            calendarForm.Show();
        }


        private void 重排ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var scheduleUtility = this.GetUtility<IScheduleUtility>();
            List<ScheduleRecord> fixedRecords = selectedRecords.Where(r => r.FFixed == true).ToList();
            var records = selectedRecords.Except(fixedRecords).ToList();
            var wds = SubtractFixedCapacity(fixedRecords, workdays);
            List<ScheduleRecord> list = scheduleUtility.Scheduling(wds, records, true);
            list.AddRange(fixedRecords);

            DaoTemplate.DeleteAll<ScheduleRecord>(user.FName);

            UV.SaveAsync(list, this, () => {
                var form = OpenForm<ScheduleResultsForm>("ScheduleResultsForm", "排产结果");
                form.LoadData();
            });
        }

        private void 正排ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var scheduleUtility = this.GetUtility<IScheduleUtility>();
            List<ScheduleRecord> fixedRecords = selectedRecords.Where(r => r.FFixed == true).ToList();
            var records = selectedRecords.Except(fixedRecords).ToList();
            var wds = SubtractFixedCapacity(fixedRecords, workdays);
            List<ScheduleRecord> list = scheduleUtility.Scheduling(wds, records, false);
            list.AddRange(fixedRecords);

            DaoTemplate.DeleteAll<ScheduleRecord>(user.FName);

            UV.SaveAsync(list, this, () => {
                var form = OpenForm<ScheduleResultsForm>("ScheduleResultsForm", "排产结果");
                form.LoadData();
            });
        }

        /// <summary>
        /// 删除选中行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            DeleteSelectedRows();
        }

        private void 分屏ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowExtraGridView();
        }

        /// <summary>
        /// 从工厂日历中减去固定工序的产能占用，不考虑班次，并确保机器组匹配
        /// </summary>
        /// <param name="fixedRecords">固定工序记录列表</param>
        /// <param name="workdays">工厂日历数据</param>
        /// <returns>更新后的工厂日历数据</returns>
        public static List<Workday> SubtractFixedCapacity(List<ScheduleRecord> fixedRecords, List<Workday> workdays)
        {
            if (fixedRecords == null)
            {
                throw new ArgumentNullException(nameof(fixedRecords), "固定工序记录不能为null");
            }

            if (workdays == null)
            {
                throw new ArgumentNullException(nameof(workdays), "工厂日历数据不能为null");
            }

            // 将workdays按照工作日期和机器组进行分组，将同一天同一机器组的多个班次工作时长合计起来
            var updatedWorkdays = workdays
                .GroupBy(w => new { w.FWorkDate, w.FMachineTeam })
                .Select(g => new Workday
                {
                    FWorkDate = g.Key.FWorkDate,
                    FMachineTeam = g.Key.FMachineTeam,
                    FDuration = g.Sum(w => w.FDuration)
                })
                .ToList();

            // 遍历所有固定工序记录
            foreach (var record in fixedRecords)
            {
                // 检查FPlanCapacityOccupation是否为空或null
                if (record.FPlanCapacityOccupation == null || !record.FPlanCapacityOccupation.Any())
                {
                    continue;
                }

                // 获取当前工序的机器组
                string machineTeam = record.FMachineTeam;

                // 遍历每个日期的产能占用
                foreach (var kvp in record.FPlanCapacityOccupation)
                {
                    var date = kvp.Key;
                    var capacity = kvp.Value;

                    // 查找对应日期和机器组的一条工作日历记录
                    var dayWorkday = updatedWorkdays.FirstOrDefault(w => w.FWorkDate == date && w.FMachineTeam == machineTeam);

                    // 计算该日期和机器组的总工作时长
                    int totalDuration = dayWorkday.FDuration;

                    // 如果总工作时长大于0，则减去产能占用
                    if (totalDuration > 0)
                    {
                        // 计算可减去的产能（不超过总工作时长）
                        int subtractedCapacity = Math.Min(capacity, totalDuration);

                        // 从该日期和机器组减去产能
                        dayWorkday.FDuration -= subtractedCapacity;
                    }
                }
            }

            return updatedWorkdays;
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            ValidateRecord();
        }

        /// <summary>
        /// 合法性检查
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 设备日历ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ValidateRecord();
        }

        private void 导出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Export();
        }

        private void 全选ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectAll();
        }

        private void 齐套性检查ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<MaterialRequirement> materialRequirements = new List<MaterialRequirement>();
            List<WorkOrder> workOrders = new List<WorkOrder>();
            string sql;

            //弹出进度条
            //UV.ShowAutoProgressWindowAsync(this, 3); 

            //清空齐套字段
            foreach (var record in selectedRecords)
                record.FIntegrity = false;

            //选出第一道序,且未领料
            var queryRecords = filteredRecords1.Any() ? filteredRecords1 : selectedRecords;
            var list = queryRecords.Where(item => item.FSequenceNumber == 1 && item.FState == "未生产").ToList();

            foreach (var record in list)
            {
                sql = string.Format(Config.Default.sql_mo_d_find_by_billno, record.FBillNo);
                var records = DaoTemplate.FindAll<MaterialRequirement>(sql);
                foreach (var r in records)
                    r.RequiredDate = record.FOperationPlanStartDate;
                materialRequirements.AddRange(records);

                workOrders.Add(new WorkOrder() { PlannedStartDate = record.FOperationPlanStartDate, WorkOrderNumber = record.FBillNo });
            }

            sql = string.Format(Config.Default.v_StockAvailableQuantity_findall);
            List<Inventory> inventories = DaoTemplate.FindAll<Inventory>(sql);

            var scheduleUtility = this.GetUtility<IScheduleUtility>();
            scheduleUtility.CalculateKittingStatus(workOrders, materialRequirements, inventories);

            // 将齐套性检查结果回填到排产记录中
            foreach (var record in list)
            {
                var workOrder = workOrders.FirstOrDefault(wo => wo.WorkOrderNumber == record.FBillNo);
                if (workOrder != null)
                {
                    record.FIntegrity = workOrder.IsKitComplete;
                }
            }

            //不保存，因为库存数据随时都在变化
            //UV.SaveAsync(list, this);

            // 刷新界面显示
            UpdateUI();

            MessageBox.Show("齐套计算完毕！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void 显示隐藏列ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColumnSettings();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            GoToFirstPage();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            GoToPreviousPage();
        }

        private void button7_Click(object sender, EventArgs e)
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
            label7.Text = "/" + _totalPages;
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
