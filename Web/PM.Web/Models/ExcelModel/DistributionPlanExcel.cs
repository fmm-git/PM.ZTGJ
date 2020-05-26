using System;
using System.Runtime.Serialization;

namespace PM.Web.Models.ExcelModel
{
    public class DistributionPlanExcel
    {
        /// <summary>
        /// 站点名称
        /// </summary>
        [DataMember(Name = "站点名称")]
        public string SiteName { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        [DataMember(Name = "订单编号及类型", Order = 25)]
        public string OrderCode_OrderStart { get; set; }
        public string OrderCode { get; set; }
        public string OrderStart { get; set; }
        /// <summary>
        /// 类型编号及名称
        /// </summary>
        [DataMember(Name = "类型编号及名称")]
        public string TypeCode_TypeName { get; set; }
        /// <summary>
        /// 类型编码
        /// </summary>
        public string TypeCode { get; set; }
        /// <summary>
        /// 类型名称
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// 合计重量（kg）
        /// </summary>
        [DataMember(Name = "合计重量（kg）")]
        public string WeightTotal { get; set; }
        /// <summary>
        /// 配送状态
        /// </summary>
        [DataMember(Name = "配送状态")]
        public string DistributionStart_DistributionTiemStartShow { get; set; }
        /// <summary>
        /// 配送状态
        /// </summary>
        public string DistributionStart { get; set; }
        /// <summary>
        /// 时间状态
        /// </summary>
        public string DistributionTiemStart { get; set; }
        public string DistributionTiemStartShow { get { return DistributionTiemStart.Equals("暂无") ? "" : DistributionTiemStart; } }
        /// <summary>
        /// 站点名称
        /// </summary>
        [DataMember(Name = "配送时间")]
        public string DistributionTimeShow_LoadCompleteTimeShow { get; set; }
        /// <summary>
        /// 计划配送时间
        /// </summary>
        public DateTime DistributionTime { get; set; }
        /// <summary>
        /// 实际配送时间
        /// </summary>
        public DateTime? LoadCompleteTime { get; set; }
        /// <summary>
        /// 计划配送时间
        /// </summary>
        public string DistributionTimeShow { get { return "计划:" + DistributionTime.ToString("yyyy-MM-dd"); } }
        /// <summary>
        /// 计划配送时间
        /// </summary>
        public string LoadCompleteTimeShow
        {
            get
            {
                string str = "实际:";
                if (!LoadCompleteTime.HasValue) return str;
                return str + LoadCompleteTime.Value.ToString("yyyy-MM-dd");
            }
        }
        /// <summary>
        /// 工区签收
        /// </summary>
        [DataMember(Name = "工区签收")]
        public string SignState { get; set; }
    }
}