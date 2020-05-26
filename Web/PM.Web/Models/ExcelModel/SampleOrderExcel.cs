using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace PM.Web.Models.ExcelModel
{
    public class SampleOrderExcel
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
        /// 取样单号
        /// </summary>
        [DataMember(Name = "取样单号")]
        public string SampleOrderCode { get; set; }
        /// <summary>
        /// 入库单号
        /// </summary>
        [DataMember(Name = "入库单号")]
        public string InOrderCode { get; set; }
        /// <summary>
        /// 检测状态
        /// </summary>
        [DataMember(Name = "检测状态")]
        public string CheckStatus_IsUpLoadShow { get; set; }
        public string CheckStatus { get; set; }
        public string IsUpLoadShow { get { return IsUpLoad == 0 ? "待传附件" : ""; } }
        public int IsUpLoad { get; set; }
        /// <summary>
        /// 重量合计
        /// </summary>
        [DataMember(Name = "重量合计")]
        public decimal WeightSum { get; set; }
        /// <summary>
        /// 取样日期
        /// </summary>
        [DataMember(Name = "取样日期")]
        public DateTime SampleTime { get; set; }
        /// <summary>
        /// 录入人
        /// </summary>
        [DataMember(Name = "录入人")]
        public string UserName_InsertTime { get; set; }
        public string UserName { get; set; }
        public DateTime InsertTime { get; set; }
    }
}