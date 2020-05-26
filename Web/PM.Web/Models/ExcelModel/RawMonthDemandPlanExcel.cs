using System;
using System.Runtime.Serialization;

namespace PM.Web.Models.ExcelModel
{
    public class RawMonthDemandPlanExcel
    {
        /// <summary>
        /// 需求月份
        /// </summary>
        [DataMember(Name = "需求月份")]
        public string DemandMonth { get; set; }
        [DataMember(Name = "工区")]
        public string WorkAreaNameShow { get { return this.BranchName + "/" + this.WorkAreaName; } }
        /// <summary>
        /// 分部名称
        /// </summary>
        public string BranchName { get; set; }
        /// <summary>
        /// 工区名称
        /// </summary>
        public string WorkAreaName { get; set; }
        /// <summary>
        /// 计划编号及类型
        /// </summary>
        [DataMember(Name = "计划编号及类型")]
        public string DemandPlanCode_RebarTypeNew { get; set; }
        /// <summary>
        /// 计划编号
        /// </summary>
        public string DemandPlanCode { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public string RebarTypeNew { get; set; }
        /// <summary>
        /// 加工厂名称
        /// </summary>
        [DataMember(Name = "加工厂名称")]
        public string ProcessFactoryName { get; set; }
        /// <summary>
        /// 计划总量(kg)
        /// </summary>
        [DataMember(Name = "计划总量(kg)")]
        public decimal PlanTotal { get; set; }
        /// <summary>
        /// 补充计划量(kg)
        /// </summary>
        [DataMember(Name = "补充计划量(kg)")]
        public decimal SupplyPlanNum { get; set; }
        /// <summary>
        /// 提交时间
        /// </summary>
        [DataMember(Name = "提交时间")]
        public DateTime InsertTime { get; set; }
        /// <summary>
        /// 批次需求计划
        /// </summary>
        [DataMember(Name = "批次需求计划")]
        public string PcJhCountShow_BatchPlanTotalShow { get; set; }
        /// <summary>
        /// 提交次数
        /// </summary>
        public string PcJhCountShow { get { return "提交次数: " + PcJhCount; } }
        /// <summary>
        /// 提交次数
        /// </summary>
        public int PcJhCount { get; set; }
        /// <summary>
        /// 总量
        /// </summary>
        public string BatchPlanTotalShow { get { return "总量(kg): " + BatchPlanTotal; } }
        /// <summary>
        /// 总量
        /// </summary>
        public decimal BatchPlanTotal { get; set; }
        /// <summary>
        /// 供应量
        /// </summary>
        [DataMember(Name = "供应量")]
        public string GhCountShow_HasSupplierTotalShow { get; set; }
        /// <summary>
        /// 提交次数
        /// </summary>
        public string GhCountShow { get { return "供货次数: " + GhCount; } }
        /// <summary>
        /// 供货次数
        /// </summary>
        public int GhCount { get; set; }
        /// <summary>
        /// 供货总量
        /// </summary>
        public string HasSupplierTotalShow { get { return "总量(kg): " + HasSupplierTotal; } }
        /// <summary>
        /// 供货总量
        /// </summary>
        public decimal HasSupplierTotal { get; set; }
        /// <summary>
        /// 入库情况
        /// </summary>
        [DataMember(Name = "入库情况")]
        public string RkCountShow_InCountShow { get; set; }
        /// <summary>
        /// 入库次数
        /// </summary>
        public string RkCountShow { get { return "入库次数: " + RkCount; } }
        /// <summary>
        /// 入库次数
        /// </summary>
        public int RkCount { get; set; }
        /// <summary>
        /// 入库总量
        /// </summary>
        public string InCountShow { get { return "总量(kg): " + InCount; } }
        /// <summary>
        /// 入库总量
        /// </summary>
        public decimal InCount { get; set; }
        /// <summary>
        /// 取样情况
        /// </summary>
        [DataMember(Name = "取样情况")]
        public string QyCountShow_BhgZlShow { get; set; }
        /// <summary>
        /// 取样次数
        /// </summary>
        public string QyCountShow { get { return "取样次数: " + QyCount; } }
        /// <summary>
        /// 取样次数
        /// </summary>
        public int QyCount { get; set; }
        /// <summary>
        /// 取样总量
        /// </summary>
        public string BhgZlShow { get { return "不合格量(kg): " + BhgZl; } }
        /// <summary>
        /// 取样总量
        /// </summary>
        public decimal BhgZl { get; set; }

    }
}