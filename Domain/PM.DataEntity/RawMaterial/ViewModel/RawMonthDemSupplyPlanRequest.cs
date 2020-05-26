using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity.Production.ViewModel
{
    public class RawMonthDemSupplyPlanRequest : PageSearchRequest
    {
        
        /// <summary>
        /// 供应商编号
        /// </summary>
        public string SupplierCode { get; set; }
        /// <summary>
        /// 组织机构类型
        /// </summary>
        public string OrgType { get; set; }
        /// <summary>
        /// 组织机构编号
        /// </summary>
        public string CompanyCode { get; set; }
        public DateTime? HistoryMonth { get; set; }
        /// <summary>
        /// 补充计划编号
        /// </summary>
        public string SupplyPlanCode { get; set; }
        /// <summary>
        /// 需求计划编号
        /// </summary>
        public string DemandPlanCode { get; set; }
        /// <summary>
        /// 钢筋类型
        /// </summary>
        public string RebarType { get; set; }

    }

    public class RawMaterialMonthDemandSupplyPlanDetail
    {
        public int ID { get; set; }
        /// <summary>
        /// 原材料编号
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        /// 原材料名称
        /// </summary>
        public string MaterialName { get; set; }
        /// <summary>
        /// 规格
        /// </summary>
        public string SpecificationModel { get; set; }
        /// <summary>
        /// 计量单位
        /// </summary>
        public string MeasurementUnitText { get; set; }
        /// <summary>
        /// 计量单位编号
        /// </summary>
        public string MeasurementUnit { get; set; }
        /// <summary>
        /// 补充计划
        /// </summary>
        public decimal SupplyNum { get; set; }
        /// <summary>
        /// 原计划数量
        /// </summary>
        public decimal DemandNum { get; set; }
        /// <summary>
        /// 技术要求
        /// </summary>
        public string SkillRequire { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}
