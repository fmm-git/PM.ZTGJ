using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity.Production.ViewModel
{
    public class OrderProgressRequest : PageSearchRequest
    {
        ///// <summary>
        ///// 加工厂编号
        ///// </summary>
        //public string ProcessFactoryCode { get; set; }
        /// <summary>
        /// 类型名称
        /// </summary>
        public string TypeName { get; set; }
        ///// <summary>
        ///// 站点名称
        ///// </summary>
        //public string SiteCode { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderCode { get; set; }
        /// <summary>
        /// 组织机构类型
        /// </summary>
        public string OrgType { get; set; }
        /// <summary>
        /// 组织机构编号
        /// </summary>
        public string CompanyCode { get; set; }
        /// <summary>
        /// 填报状态
        /// </summary>
        public string ReportedStatus { get; set; }
        /// <summary>
        /// 加工状态
        /// </summary>
        public string ProcessingState { get; set; }
        /// <summary>
        /// 历史月份
        /// </summary>
        public DateTime? HistoryMonth { get; set; }
        /// <summary>
        /// 进度类型
        /// </summary>
        public string ProgressType { get; set; }
    }
    public class OrderProgressResponse
    {
        public int ID { get; set; }
        public string OrderCode { get; set; }
        public string TypeCode { get; set; }
        public string TypeName { get; set; }
        public string SiteCode { get; set; }
        public string SiteName { get; set; }
        public DateTime? DistributionTime { get; set; }
        public string ComponentName { get; set; }
        public string Number { get; set; }
        public string PackNumber { get; set; }
        public string lldyNum { get; set; }
        public string syNum { get; set; }
    }
}
