using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity.Production.ViewModel
{
    public class RawMonthDemPlanRequest : PageSearchRequest
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
        /// <summary>
        /// 需求计划编号
        /// </summary>
        public string DemandPlanCode { get; set; }
        /// <summary>
        /// 钢筋类型
        /// </summary>
        public string RebarType { get; set; }
        /// <summary>
        /// 弹框收索条件
        /// </summary>
        public string keyword { get; set; }
        /// <summary>
        /// 需求月份
        /// </summary>
        public DateTime? HistoryMonth { get; set; }
        /// <summary>
        /// 材料计划类型
        /// </summary>
        public string PlanType { get; set; }
        /// <summary>
        /// 计划是否充足
        /// </summary>
        public string PlanIsAmple { get; set; }
        /// <summary>
        /// 供货是否满足
        /// </summary>
        public string SupplyAmple { get; set; }

        /// <summary>
        /// 是否按时提交
        /// </summary>
        public string IsAsTj { get; set; }
        /// <summary>
        /// 导出
        /// </summary>
        public bool IsOutput { get; set; }
    }

    public class RawMaterialMonthDemandPlanDetail
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
        /// 需求数量
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

    public class SumRowData
    {
        public decimal BuildingSteel { get; set; }
        public decimal SectionSteel { get; set; }
    }
}
