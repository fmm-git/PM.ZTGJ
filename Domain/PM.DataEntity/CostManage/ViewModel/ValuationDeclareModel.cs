using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity.CostManage.ViewModel
{
    /// <summary>
    /// 辅料计划申报列表查询
    /// </summary>
    public class ValuationDeclareRequest : PageSearchRequest
    {
        /// <summary>
        /// 申报编号
        /// </summary>
        public string ValuationDeclareCode { get; set; }
    }
    /// <summary>
    /// 配送费用申报列表查询
    /// </summary>
    public class DistributionDeclareRequest : PageSearchRequest
    {
        /// <summary>
        /// 申报编号
        /// </summary>
        public string DistributionDeclareCode { get; set; }
    }
    /// <summary>
    /// 人员费用核算列表查询
    /// </summary>
    public class UserCostRequest : PageSearchRequest
    {
        /// <summary>
        /// 申报编号
        /// </summary>
        public string CheckCode { get; set; }

        public string keyword { get; set; }
    }
    /// <summary>
    /// 机械费用核算列表查询
    /// </summary>
    public class MachineCostRequest : UserCostRequest
    {
        public string keyword { get; set; }
    }
    /// <summary>
    /// 成本总报表查询
    /// </summary>
    public class CostReportRequest : PageSearchRequest
    {
        /// <summary>
        /// 加工订单编号
        /// </summary>
        public string OrderCode { get; set; }
    }
}
