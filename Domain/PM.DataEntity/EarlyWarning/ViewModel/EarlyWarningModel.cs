using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataEntity.EarlyWarning.ViewModel
{
    public class EarlyWarningModel : PageSearchRequest
    {

    }
    public class EarlyWarningRequest : PageSearchRequest
    {

        /// <summary>
        /// 预警状态
        /// </summary>
        public int EWStart { get; set; }

        /// <summary>
        /// 工区编号
        /// </summary>
        public string WorkArea { get; set; }
        /// <summary>
        /// 站点编号
        /// </summary>
        public string CompanyCode { get; set; }
    }

    public class SupplyEarningModel
    {
        public string MenuCode { get; set; }
        public int EWNodeCode { get; set; }
        public string PersonnelCode { get; set; }
        public string ProjectId { get; set; }
        public string EarlyWarningCode { get; set; }
        public int EWFormDataCode { get; set; }
        public string BatchPlanNum { get; set; }
        public string CompanyCode { get; set; }
        public string BranchName { get; set; }
        public string WorkArea { get; set; }
        public string WorkAreaName { get; set; }
        public string SiteCode { get; set; }
        public string SiteName { get; set; }
        public string ManagerDepartment { get; set; }

        public decimal CsDay { get; set; }

        public string MsgType { get; set; }
    }
}
