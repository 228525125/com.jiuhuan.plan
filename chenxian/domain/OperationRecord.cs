using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{
    [Entity(Title = "工序维护", FormName = "ProcessResultsForm", EditFormName = "ProcessResultsForm2", Pagination = false)]
    public class OperationRecord : Entity
    {

        //[Column(name:"行号", 60, description:"E10单身行号")]
        public int FSequenceNumber { get; set; }

        [Ignore]
        [Column("最小包装", 80, index: 100)]
        public int FPackingSize { get; set; }

        [Ignore]
        [Column("自动化", 80, index: 105)]
        public bool FExistsValveAutoProcess { get; set; }

        [Ignore]
        [Column("试压次数", 80, index: 110)]
        public int FPressureTestTime { get; set; }

        [Ignore]
        [Column("压力等级", 80, index: 115)]
        public string FPressureLevel { get; set; }

        [Ignore]
        [Column("刻印模板", 80, index: 120)]
        public bool FExistsStampTemplate { get; set; }

        [Ignore]
        [Column("附件BOM", 80, index: 125)]
        public bool FExistsAccessoryBom { get; set; }

        [Ignore]
        [Column("二维码", 80, index: 130)]
        public int CYSQRCodeCount { get; set; }

        [Ignore]
        [Column("阀号起", 80, index: 135)]
        public string FValveSerialNumberBegin { get; set; }

        [Ignore]
        [Column("阀号止", 80, index: 140)]
        public string FValveSerialNumberEnd { get; set; }

        [Ignore]
        [Column("客户刻", 80, index: 145)]
        public string FCustomerStampContent { get; set; }

        [Ignore]
        [Column("已刻印", 80, index: 150)]
        public int FMarkedCount { get; set; }

        [Ignore]
        [Column("炉号", 80, index: 155)]
        public string FMaterialBatchNo { get; set; }

        [Column("状态", 80, index: 160, description: "工单状态")]
        [TextBox("状态")]
        public string FState { get; set; }

        [Column("缺料", 80, index: 165, description: "根据工单单身查询库存余额")]
        public bool FLack { get; set; }

        [Column("齐套", 80, index: 170, description: "根据工单单身检查是否齐套")]
        public bool FIntegrity { get; set; }

        [Column("工单号", index: 175)]
        [TextBox("工单号")]
        public string FBillNo { get; set; }

        [Column("工单计划完工", index: 180)]
        [DateTimePicker("工单计划完工")]
        public DateTime FMoPlanCompleteDate { get; set; }

        [Column("品号", index: 185)]
        [TextBox("品号")]
        public string FNumber { get; set; }

        [Column("品名", index: 190)]
        [TextBox("品名")]
        public string FName { get; set; }

        [Column("规格", index: 195)]
        [TextBox("规格")]
        public string FModel { get; set; }

        [Column("待加工数", 80, index: 200, description: "待加工数=工单数量+工序投入调整量-工序报废数量-工序完成数量-工序破坏数量")]
        [TextBox("待加工数", readOnly: false)]
        public float FRemainQty { get; set; }

        [Column("工序号", 70, index: 205)]
        [TextBox("工序号")]
        public string FOperationNumber { get; set; }

        [Column("工序名称", index: 210)]
        [TextBox("工序名称")]
        public string FOperationName { get; set; }

        [Column("需要工时", 80, index: 215, description: "需要工时数/秒 = 待加工数 * 标准工时 + 辅助工时")]
        [TextBox("需要工时")]
        public int FNeedWorkHours { get; set; }

        [Column("辅助工时", 80, index: 220, description: "工艺路线固定人时")]
        [TextBox("辅助工时")]
        public int FAidedWorkHours { get; set; }

        [Column("标准工时", 80, index: 225, description: "工艺路线变动人时")]
        [TextBox("标准工时")]
        public int FStandardWorkHours { get; set; }

        [Column("预计开工", 100, index: 230)]
        [DateTimePicker("预计开工", readOnly: false)]
        public DateTime FOperationPlanStartDate { get; set; }

        [Column("预计完工", 100, index: 235)]
        [DateTimePicker("预计完工", readOnly: false)]
        public DateTime FOperationPlanCompleteDate { get; set; }

        [Column("投入数", 80, index: 240)]
        [TextBox("投入数")]
        public float FDispatchedQty { get; set; }

        [Column("报工数", 80, index: 245)]
        [TextBox("报工数")]
        public float FCollectQty { get; set; }

        //[Column("超期", 40, index: 250)]
        //[TextBox("超期")]
        public bool FTimeout { get; set; }

        [Column("工单数量", index: 255)]
        [TextBox("工单数量")]
        public float FQty { get; set; }

        [Column("完工数量", index: 260)]
        [TextBox("完工数量")]
        public float FCompletedQty { get; set; }

        [Column("客户代码", index: 265)]
        [TextBox("客户代码")]
        public string FCustomerCode { get; set; }

        [Column("客户合同号", index: 270)]
        [TextBox("客户合同号")]
        public string FCustomerContractNo { get; set; }

        [Column("备注", index: 275)]
        [TextBox("备注")]
        public string FRemark { get; set; } = "";

        [Column("工单制单", index: 280)]
        [DateTimePicker("工单制单")]
        public DateTime FMoCreateDate { get; set; }

        [Column("设备组", index: 285)]
        [ComboBox("505-装配;501-自动化线", title: "设备组", readOnly: false)]
        public string FMachineTeam { get; set; }

        [Column("工作中心", index: 290)]
        [TextBox("工作中心")]
        public string FWorkCenter { get; set; }
    }
}
