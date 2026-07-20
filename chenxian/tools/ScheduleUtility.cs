using com.jiuhuan.plan.domain;
using com.jiuhuan.plan.model;
using com.jiuhuan.plan.system;
using com.jiuhuan.plan.tools;
using QFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan
{

    public interface IScheduleUtility : IUtility
    {
        /// <summary>
        /// 计算排产
        /// </summary>
        /// <param name="workdays">工厂日历</param>
        /// <param name="records">工序信息</param>
        /// <param name="reverse">排产模式：正排、倒排</param>
        List<ScheduleRecord> Scheduling<T>(List<Workday> workdays, List<T> records, bool reverse = false) where T : OperationRecord ;

        /// <summary>
        /// 计算指定日期的剩余产能
        /// </summary>
        /// <param name="workdays">工厂日历数据</param>
        /// <param name="records">工序记录</param>
        /// <param name="date">查询日期</param>
        /// <returns>指定日期的剩余产能(秒)</returns>
        int CalculateRemainingCapacity<T>(List<Workday> workdays, List<T> records, DateTime date, string machineTeam) where T : ScheduleRecord;

        /// <summary>
        /// 计算指定日期和班次的剩余产能
        /// </summary>
        /// <param name="workdays">工厂日历</param>
        /// <param name="records">工序记录</param>
        /// <param name="date">查询日期</param>
        /// <param name="classes">班次</param>
        /// <returns>指定日期和班次的剩余产能(秒)</returns>
        int CalculateRemainingCapacityForClasses<T>(List<Workday> workdays, List<T> records, DateTime date, string classes, string machineTeam) where T : ScheduleDetailsRecord;

        /// <summary>
        /// 计算工单齐套状态
        /// </summary>
        /// <param name="workOrders">工单列表</param>
        /// <param name="requirements">投料清单列表</param>
        /// <param name="inventories">库存清单列表</param>
        void CalculateKittingStatus(List<WorkOrder> workOrders, List<MaterialRequirement> requirements, List<Inventory> inventories);
    }

    public class ScheduleUtility : IScheduleUtility
    {
        public List<ScheduleRecord> Scheduling<T>(List<Workday> workdays, List<T> records, bool reverse) where T : OperationRecord
        {
            var schedulingSystem = new SchedulingSystem();

            // 获取今天的Workday数据
            var todayWorkdays = workdays.Where(w => w.FWorkDate.Date == DateTime.Today).ToList();
            foreach(var todayWorkday in todayWorkdays)
            {
                var remainingWorkTimePercentage = todayWorkday.GetRemainingWorkTimePercentage();
                todayWorkday.FDuration = Convert.ToInt32(todayWorkday.FDuration * remainingWorkTimePercentage / 100);
            }

            // 将Workday列表转换为FactoryCalendar列表
            // 按照工作日期和机器组进行分组，将同一天同一机器组的多个班次工作时长合计起来
            // 删除掉今天之前的工厂日历
            List<FactoryCalendar> factoryCalendars = workdays
                .Where(w => w.FWorkDate >= DateTime.Today) 
                .GroupBy(w => new { w.FWorkDate, w.FMachineTeam })
                .Select(g => new FactoryCalendar
                {
                    WorkDate = g.Key.FWorkDate,
                    MachineName = g.Key.FMachineTeam,
                    WorkDuration = g.Sum(w => w.FDuration),
                }).ToList();

            // 将ScheduleRecord列表转换为Process列表
            List<Process> processes = records.Select(r => new Process
            {
                ProcessId = r.FID,
                SequenceNumber = r.FSequenceNumber,
                MachineName = r.FMachineTeam,
                DurationSeconds = r.FNeedWorkHours,
                WorkOrderNo = r.FBillNo,
                WorkOrderCreateTime = r.FMoCreateDate,
                WorkOrderPlanEndTime = r.FMoPlanCompleteDate,
            }).ToList();


            var result = reverse ? schedulingSystem.ScheduleProcessesReverse(processes, factoryCalendars) : schedulingSystem.ScheduleProcessesForward(processes, factoryCalendars);

            List<ScheduleRecord> list = new List<ScheduleRecord>();

            foreach (var record in records)
            {
                var scheduleReocrd = Utility.CopyProperties(record, new ScheduleRecord());
                list.Add(scheduleReocrd);
            }

            foreach (var item in result)
            {
                var record = list.FirstOrDefault(r => r.FID == item.ProcessId);
                if (record != null)
                {
                    if (item.PlanCapacityOccupation.Any()) {
                        record.FPlanCapacityOccupation = item.PlanCapacityOccupation.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                        record.FOperationPlanStartDate = record.FPlanCapacityOccupation.Keys.First();
                        record.FOperationPlanCompleteDate = record.FPlanCapacityOccupation.Keys.Last();
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 计算指定日期的剩余产能
        /// </summary>
        /// <param name="workdays">工厂日历数据</param>
        /// <param name="records">工序记录</param>
        /// <param name="date">查询日期</param>
        /// <returns>指定日期的剩余产能(秒)</returns>
        public int CalculateRemainingCapacity<T>(List<Workday> workdays, List<T> records, DateTime date, string machineTeam) where T : ScheduleRecord
        {
            try
            {
                // 获取指定日期的总产能
                var totalCapacity = CalculateTotalCapacity(workdays, date, machineTeam);

                // 获取指定日期已占用的产能
                var usedCapacity = CalculateUsedCapacity(records, date, machineTeam);

                // 返回剩余产能
                return totalCapacity - usedCapacity;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"计算日期 {date:yyyy-MM-dd} 的剩余产能时发生错误: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 计算指定日期的总产能
        /// </summary>
        /// <param name="workdays">工厂日历数据</param>
        /// <param name="date">查询日期</param>
        /// <returns>指定日期的总产能(秒)</returns>
        private int CalculateTotalCapacity(List<Workday> workdays, DateTime date, string machineTeam)
        {
            var targetDate = date.Date;

            // 过滤出指定日期的工作日数据并汇总产能
            return workdays
                .Where(w => w.FWorkDate.Date == targetDate && w.FMachineTeam == machineTeam)
                .Sum(w => w.FDuration);
        }

        /// <summary>
        /// 计算指定日期已占用的产能
        /// </summary>
        /// <param name="records">工序记录</param>
        /// <param name="date">查询日期</param>
        /// <returns>指定日期已占用的产能(秒)</returns>
        private int CalculateUsedCapacity<T>(List<T> records, DateTime date, string machineTeam) where T : ScheduleRecord
        {
            var targetDate = date.Date;

            // 过滤出在指定日期有产能占用的工序记录并汇总已占用产能
            return records
                .Where(r => r.FPlanCapacityOccupation != null &&
                            r.FPlanCapacityOccupation.ContainsKey(targetDate) && r.FMachineTeam == machineTeam)
                .Sum(r => r.FPlanCapacityOccupation[targetDate]);
        }

        /// <summary>
        /// 计算指定日期和班次的剩余产能
        /// </summary>
        /// <param name="workdays">工厂日历</param>
        /// <param name="records">工序记录</param>
        /// <param name="date">查询日期</param>
        /// <param name="classes">班次</param>
        /// <returns>指定日期和班次的剩余产能(秒)</returns>
        public int CalculateRemainingCapacityForClasses<T>(List<Workday> workdays, List<T> records, DateTime date, string classes, string machineTeam) where T : ScheduleDetailsRecord
        {
            try
            {
                // 获取指定日期和班次的总产能
                var totalCapacity = CalculateTotalCapacityForClasses(workdays, date, classes, machineTeam);

                // 获取指定日期和班次已占用的产能
                var usedCapacity = CalculateUsedCapacityForClasses(records, date, classes, machineTeam);

                // 返回剩余产能
                return totalCapacity - usedCapacity;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"计算日期 {date:yyyy-MM-dd} 班次 {classes} 的剩余产能时发生错误: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 计算工单齐套状态
        /// </summary>
        /// <param name="workOrders">工单列表</param>
        /// <param name="requirements">投料清单列表</param>
        /// <param name="inventories">库存清单列表</param>
        public void CalculateKittingStatus(List<WorkOrder> workOrders, List<MaterialRequirement> requirements, List<Inventory> inventories)
        {
            IKittingSystem kittingSystem = new KittingSystem();

            kittingSystem.CalculateKittingStatus(workOrders, requirements, inventories);
        }

        /// <summary>
        /// 计算指定日期和班次的总产能
        /// </summary>
        /// <param name="workdays">工厂日历数据</param>
        /// <param name="date">查询日期</param>
        /// <param name="classes">班次</param>
        /// <returns>指定日期和班次的总产能(秒)</returns>
        private int CalculateTotalCapacityForClasses(List<Workday> workdays, DateTime date, string classes, string machineTeam)
        {
            var targetDate = date.Date;

            // 过滤出指定日期和班次的工作日数据并汇总产能
            return workdays
                .Where(w => w.FWorkDate.Date == targetDate && w.FClasses == classes && w.FMachineTeam == machineTeam)
                .Sum(w => w.FDuration);
        }

        /// <summary>
        /// 计算指定日期和班次已占用的产能
        /// </summary>
        /// <param name="records">工序记录</param>
        /// <param name="date">查询日期</param>
        /// <param name="classes">班次</param>
        /// <returns>指定日期和班次已占用的产能(秒)</returns>
        private int CalculateUsedCapacityForClasses<T>(List<T> records, DateTime date, string classes, string machineTeam) where T : ScheduleDetailsRecord
        {
            var targetDate = date.Date;

            // 过滤出在指定日期有产能占用的工序记录并汇总已占用产能
            return records
                .Where(r => r.FClasses != null && r.FOperationPlanStartDate.Date == targetDate &&
                            r.FClasses.ContainsKey(classes) && r.FMachineTeam == machineTeam)
                .Sum(r => r.FClasses[classes]);
        }
    }
}
