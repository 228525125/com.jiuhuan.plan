using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{
    [Entity(Title = "生产计划", FormName = "ScheduleFinalResultsForm", EditFormName = "ScheduleFinalResultsEditForm2", Pagination = false)]
    public class ScheduleFinalRecord : OperationRecord
    {
        //private int _FOrder = 0;

        //[Ignore]
        //[Column("顺序", 70, index: 10, readOnly: false)]
        //public int FOrder { get { return _FOrder == 0 ? FID : _FOrder; } set { _FOrder = value; } }

        [Column("班次", 40, index: 90)]
        [ComboBox("早;中;夜", title: "班次", index: 90, readOnly: false)]
        public string FClasses { get; set; }

        public string FScheduleName { get; set; } = "default";

    }
}
