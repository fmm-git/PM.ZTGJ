using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity.RawMaterial.ViewModel
{
    public class TSupplyListRequest : PageSearchRequest
    {
        /// <summary>
        /// 需求计划编号
        /// </summary>
        public string BatchPlanNum { get; set; }
        /// <summary>
        /// 历史月份
        /// </summary>
        public DateTime? HistoryMonth { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string StateCode { get; set; }
        /// <summary>
        /// 月度需求计划编号
        /// </summary>
        public string DemandPlanCode { get; set; }
    }
}
