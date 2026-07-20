using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{
    public class MachineTeam : Entity
    {
        public string FCode { get; set; }
        public string FName { get; set; }
        public float FCapacity { get; set; }
        public float FRatio { get; set; }
        public string FRemark { get; set; } = "";
    }
}
