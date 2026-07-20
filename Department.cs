using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{
    public class Department : Entity
    {
        [Column("编号", 70, index: 100)]
        [TextBox("工序号")]
        public string FNumber { get; set; }

        [Column("名称", index: 105)]
        [TextBox("名称")]
        public string FName { get; set; }
    }
}
