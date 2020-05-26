using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataEntity
{
    public class TbSysMenuTableRequset : PageSearchRequest
    {
        public int ID {get;set;}
        public string MenuCode { get; set; }
        public string TableName { get; set; }
        public string IsMainTabel { get; set; }
    }
}
