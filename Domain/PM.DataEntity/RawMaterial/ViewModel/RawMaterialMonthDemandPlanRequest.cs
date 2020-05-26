using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity
{
    public class RawMaterialMonthDemandPlanRequest : PageSearchRequest
    {
        /// <summary>
        /// 需求计划编号
        /// </summary>
        public string DemandPlanCode { get; set; }
        /// <summary>
		/// 钢筋类型
		/// </summary>
        public string RebarType { get; set; }
        /// <summary>
        /// 钢筋类型名称
        /// </summary>
        public string RebarTypeName { get; set; }
        /// <summary>
		/// 分部编号
		/// </summary>
        public string BranchCode { get; set; }
        /// <summary>
        /// 分部名称
        /// </summary>
        public string BranchName { get; set; }
        /// <summary>
		/// 站点编号
		/// </summary>
        public string SiteCode { get; set; }
        /// <summary>
        /// 站点名称
        /// </summary>
        public string SiteName { get; set; }
        /// <summary>
		/// 加工厂编号
		/// </summary>
        public string ProcessFactoryCode { get; set; }
        /// <summary>
        /// 加工厂Name
        /// </summary>
        public string ProcessFactoryName { get; set; }
        /// <summary>
		/// 计划总量
		/// </summary>
        public decimal PlanTotal { get; set; }
        /// <summary>
		/// 交货时间
		/// </summary>
        public DateTime DeliveryDate { get; set; }
    }
}
