using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.jiuhuan.plan.domain
{
    [Entity(entirety: false, Database = "plan", Pagination = true)]
    public class Entity
    {
        [Primary("由数据库产生")]
        public int FID { get; set; }

        public DateTime FDate { get; set; } = DateTime.Now;

        public string FUser { get; set; } = "admin";

        public string FKeyNo { get; set; } = Guid.NewGuid().ToString();

        public string FNote { get; set; } = "";

        [Column("顺序", 40, index: 10, readOnly: false, visible: false)]
        public int FStatus { get; set; } = 0;

        public bool FChecked { get; set; } = false;

        [Ignore("用于保存DataGridView中行背景色")]
        public Color RowBackColor { get; set; } = Color.Empty;

        public override int GetHashCode()
        {
            return 0 == FID ? base.GetHashCode() : FID;
        }

        public override bool Equals(object obj)
        {
            if(obj is Entity entity)
            {
                if (0 == FID || 0 == entity.FID)
                {
                    return FKeyNo.Equals(entity.FKeyNo);
                }
                else
                {
                    return FID == entity.FID;
                }
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return $"{FID}/{FDate}/{FUser}/{FKeyNo}/{FNote}/{FStatus}/{FChecked}";
        }
    }
}
