using System;
using System.Runtime.Serialization;

namespace PM.Web.Models.ExcelModel
{
    public class InOrderExcel
    {
        [DataMember(Name = "工区")]
        public string WorkAreaNameShow { get { return this.BranchName + "/" + this.WorkAreaName; } }
        /// <summary>
        /// 入库单号
        /// </summary>
        [DataMember(Name = "入库单号")]
        public string InOrderCode { get; set; }
        /// <summary>
        /// 批次计划编号
        /// </summary>
        [DataMember(Name = "批次计划编号")]
        public string BatchPlanCode { get; set; }
        [DataMember(Name = "钢筋类型")]
        public string RebarTypeShow { get { return RebarType == "SectionSteel" ? "型钢" : "建筑钢筋"; } }
        public string RebarType { get; set; }
        /// <summary>
        /// 分部名称
        /// </summary>
        public string BranchName { get; set; }
        /// <summary>
        /// 工区名称
        /// </summary>
        public string WorkAreaName { get; set; }
        /// <summary>
        /// 入库总量(kg)
        /// </summary>
        [DataMember(Name = "入库总量(kg)")]
        public string InCount_InsertTime { get; set; }
        public decimal InCount { get; set; }
        public DateTime? InsertTime { get; set; }
        /// <summary>
        /// 取样状态
        /// </summary>
        [DataMember(Name = "取样状态")]
        public string SampleOrderState_SampleOrderTime { get; set; }
        public string SampleOrderState { get; set; }
        public DateTime? SampleOrderTime { get; set; }
        /// <summary>
        /// 检测结果
        /// </summary>
        [DataMember(Name = "检测结果")]
        public string ChackResultShow_IsThShow { get; set; }
        public string ChackResultShow
        {
            get
            {
                if (this.ChackResult == 1)
                    return "合格";
                else if (this.ChackResult == 2)
                    return "不合格";
                else
                    return "部分合格";
            }
        }
        public int ChackResult { get; set; }
        public string IsThShow
        {
            get
            {
                if (ChackResult == 1) return "";
                if (IsTh) return "已退回";
                else
                    return "";
            }
        }
        public bool IsTh { get; set; }
        /// <summary>
        /// 入库仓库
        /// </summary>
        [DataMember(Name = "入库仓库")]
        public string StorageName { get; set; }
        /// <summary>
        /// 收货人
        /// </summary>
        [DataMember(Name = "收货人")]
        public string DeliverUserShow { get { return DeliverUser + "/" + Tel; } }
        public string DeliverUser { get; set; }
        public string Tel { get; set; }
    }
}