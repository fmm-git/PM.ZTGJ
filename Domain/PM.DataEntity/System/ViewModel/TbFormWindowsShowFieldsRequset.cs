using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataEntity
{
    public class TbFormWindowsShowFieldsRequset : PageSearchRequest
    {
        public int id { get; set; } 
        public string PhysicalTableName{ get; set; }
        public string FieldCode{ get; set; }
        public string FieldName{ get; set; }
        public int FieldShowOrder{ get; set; }
        public int FieldIsShow{ get; set; }
    }
}
