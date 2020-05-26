using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity.DataManage.ViewModel
{
    public class TbDataManageRequest : PageSearchRequest
    {
        //资料类别
        public string TypeName { get; set; }
    }
}
