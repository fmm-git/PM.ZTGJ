using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace PM.Web.Models.ExcelModel
{
    public class FactoryBatchNeedPlanExcel
    {
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
        /// 批次计划编号
        /// </summary>
        [DataMember(Name = "批次计划编号")]
        public string BatchPlanNum { get; set; }
        /// <summary>
        /// 钢筋类型
        /// </summary>
        [DataMember(Name = "钢筋类型")]
        public string RebarTypeNew { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        [DataMember(Name = "供货状态")]
        public string StateCodeShow_StateCodeShow2 { get; set; }
        public string StateCodeShow
        {
            get
            {
                if (StateCode == "未供货")
                {
                    if (this.SupplyDate.HasValue)
                    {
                        if (this.SupplyDate.Value >= DateTime.Now)
                            return StateCode;
                        else
                            return "超期未供货";
                    }
                    else
                    {
                        return StateCode;
                    }
                }
                else if (StateCode == "部分供货")
                {
                    if (this.SupplyDate.Value >= DateTime.Now)
                        this.StateCodeShow2 = "按时供货";
                    else
                        this.StateCodeShow2 = "超时供货";
                    return "供货不足";
                }
                else
                {
                    if (this.SupplyDate.Value >= this.SupplyCompleteTime)
                        this.StateCodeShow2 = "按时供货";
                    else
                        this.StateCodeShow2 = "超时供货";
                    var pclsx = this.BatchPlanTotal + (this.BatchPlanTotal * 0.05m);
                    if (this.HasSupplierTotal >= pclsx)
                        return "供货过多";
                    else
                        return "供货完成";
                }
            }
        }
        public string StateCodeShow2 { get; set; }
        public string StateCode { get; set; }
        /// <summary>
        /// 计划总量(kg)
        /// </summary>
        [DataMember(Name = "计划总量(kg)")]
        public decimal BatchPlanTotal { get; set; }
        /// <summary>
        /// 供应时间
        /// </summary>
        [DataMember(Name = "供应时间")]
        public DateTime? SupplyDate { get; set; }
        /// <summary>
        /// 已供应总量(kg)
        /// </summary>
        [DataMember(Name = "已供应总量(kg)")]
        public decimal HasSupplierTotal { get; set; }
        /// <summary>
        /// 供货完成时间
        /// </summary>
        [DataMember(Name = "供货完成时间")]
        public DateTime? SupplyCompleteTime { get; set; }
        /// <summary>
        /// 验收/检验不合格量(kg)
        /// </summary>
        [DataMember(Name = "验收/检验不合格量(kg)",Order=30)]
        public string NoPassTotalShow_QyBhgCountShow { get; set; }
        public string NoPassTotalShow { get { return "验收:" + NoPassTotal; } }
        public decimal NoPassTotal { get; set; }
        public string QyBhgCountShow { get { return "检测:" + QyBhgCount; } }
        public decimal QyBhgCount { get; set; }
        /// <summary>
        /// 合格供货量(kg)
        /// </summary>
        [DataMember(Name = "合格供货量(kg)")]
        public decimal PassCount { get; set; }
        /// <summary>
        /// 验收人及联系方式 
        /// </summary>
        [DataMember(Name = "验收人及联系方式")]
        public string AcceptorNameShow { get { return this.AcceptorName + "/" + this.ContactWay; } }
        /// <summary>
        /// 验收人
        /// </summary>
        public string AcceptorName { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        public string ContactWay { get; set; }
    }
}