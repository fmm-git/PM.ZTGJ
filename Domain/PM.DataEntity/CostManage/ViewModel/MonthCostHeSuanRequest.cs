using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataEntity.CostManage.ViewModel
{
   public class MonthCostHeSuanRequest : PageSearchRequest
    {
        /// <summary>
        /// 核算编号
        /// </summary>
        public string HeSuanCode { get; set; }
    }
}
