using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity.Production.ViewModel
{
    public class FactoryBatchNeedPlanRequest : PageSearchRequest
    {
        #region Model

        /// <summary>
        /// 加工厂批次需求计划   标识ID
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// 批次需求计划编号
        /// </summary>
        public string BatchPlanNum { get; set; }
        /// <summary>
        /// 原材需求编号
        /// </summary>
        public string RawMaterialDemandNum { get; set; }
        /// <summary>
        /// 钢材类型Code
        /// </summary>
        public string SteelsTypeCode { get; set; }
        /// <summary>
        /// 钢材类型名称
        /// </summary>
        public string DictionaryText { get; set; }
        /// <summary>
        /// 分部名称Code
        /// </summary>
        public string BranchCode { get; set; }
        /// <summary>
        /// 分部名称
        /// </summary>
        public string CompanyFullName { get; set; }
        /// <summary>
        /// 站点名称Code
        /// </summary>
        public string SiteCode { get; set; }
        /// <summary>
        /// 站点名称Code
        /// </summary>
        public string SiteName { get; set; }
        /// <summary>
        /// 供应商名称Code
        /// </summary>
        public string SupplierCode { get; set; }
        /// <summary>
        /// 供应商名称Code
        /// </summary>
        public string SupplierName { get; set; }
        /// <summary>
        /// 交货地点
        /// </summary>
        public string DeliveryPlace { get; set; }
        /// <summary>
        /// 验收人
        /// </summary>
        public string Acceptor { get; set; }
        /// <summary>
        /// 验收人
        /// </summary>
        public string AcceptorName { get; set; }
        /// <summary>
        /// 联系方式
        /// </summary>
        public string ContactWay { get; set; }
        /// <summary>
        /// 加工厂Code
        /// </summary>
        public string ProcessFactoryCode { get; set; }
        /// <summary>
        /// 加工厂Code
        /// </summary>
        public string ProcessFactoryName { get; set; }
        /// <summary>
        /// 批次计划总量
        /// </summary>
        public decimal BatchPlanTotal { get; set; }
        /// <summary>
        /// 录入人Code
        /// </summary>
        public string InsertUserCode { get; set; }
        /// <summary>
        /// 录入人Code
        /// </summary>
        public string InsertUserName { get; set; }
        /// <summary>
        /// 录入时间
        /// </summary>
        public DateTime InsertTime { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }
        /// <summary>
        /// 审批状态
        /// </summary>
        public string Examinestatus { get; set; }
        /// <summary>
        /// 附件
        /// </summary>
        public string Enclosure { get; set; }
        /// <summary>
        /// 状态Code
        /// </summary>
        public string StateCode { get; set; }

        #endregion
    }
}
