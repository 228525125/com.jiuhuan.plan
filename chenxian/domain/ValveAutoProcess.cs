using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{
    [Entity(entirety: true, databaseName: "coc", title: "自动化参数")]
    public class ValveAutoProcess : Entity
    {
        [Column(width: 100)]
        [TextBox(width: 100, readOnly: false)]                                                                                          //"品名" = 返回调用弹窗的控件，"品号=FNumber"多个用;隔开，等于返回属性名为FNumber的属性对应的控件
        [Popup("请选择品号", sql: "SELECT ITEM_CODE AS 品号, ITEM_NAME AS 品名, ITEM_SPECIFICATION AS 规格  FROM CQJH.dbo.ITEM", Return = "品号=FNumber", IsMultipleColumnReturn = true)]
        [Field("品号", allowNull:false)]
        public string FNumber { get; set; }

        [Column(width: 100)]
        [ComboBox("五阀体;三阀体;左高压二阀体;差压左高压二阀体", width:100, readOnly: false)]
        [Field("阀体类型")]
        public string FVavleType { get; set; }

        [Column(width: 100)]
        [ComboBox("32", width: 100, readOnly: false)]
        [Field("阀体宽度")]
        public int ValveWidth { get; set; }

        [Column(width: 100)]
        [ComboBox("60;64", width: 100, readOnly: false)]
        [Field("阀体厚度")]
        public int ValveHeight { get; set; }

        [Column(width: 90)]
        [CheckBox(readOnly: false, index: 90)]
        [Field("正面打标")]
        public bool MarkFront { get; set; }

        [Column(width: 90)]
        [CheckBox(readOnly: false, index: 90)]
        [Field("上面打标")]
        public bool MarkUp { get; set; }

        [Column(width: 90)]
        [CheckBox(readOnly:false, index: 90)]
        [Field("底面打标")]
        public bool MarkDown { get; set; }

        public bool MarkReserve2 { get; set; }

        [Column(width: 100)]
        [TextBox(width: 100, readOnly: false)]
        [Field("试压次数", allowNull: false)]
        public int PressureTestTime { get; set; }

        [Column(width: 90)]
        [CheckBox(readOnly:false, index: 90)]
        [Field("自动化")]
        public bool IsAutomation { get; set; }
    }
}
