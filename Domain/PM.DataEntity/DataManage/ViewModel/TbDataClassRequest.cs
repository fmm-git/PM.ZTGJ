using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity.DataManage.ViewModel
{
    public class TbDataClassRequest:PageSearchRequest
    {
        public string MenuName { get; set; }

        public string code { get; set; }
    }
}
