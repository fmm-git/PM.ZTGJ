using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace PM.Web.Models.ExcelModel
{
    public class RMProductionMaterialExcel
    {
        /// <summary>
        /// 领用单号
        /// </summary>
        [DataMember(Name = "领用单号")]
        public string CollarCode { get; set; }
        /// <summary>
        /// 加工订单编号
        /// </summary>
        [DataMember(Name = "加工订单编号")]
        public string OrderCode { get; set; }
        /// <summary>
        /// 类型编号    
        /// </summary>
        [DataMember(Name = "类型编号")]
        public string TypeCode { get; set; }
        /// <summary>
        /// 类型名称
        /// </summary>
        [DataMember(Name = "类型名称")]
        public string TypeName { get; set; }
        /// <summary>
        /// 使用部位
        /// </summary>
        [DataMember(Name = "使用部位")]
        public string CollarPosition { get; set; }
        /// <summary>
        /// 重量合计（kg）
        /// </summary>
        [DataMember(Name = "重量合计（kg）")]
        public decimal WeightSum { get; set; }
        /// <summary>
        /// 领用重量合计（kg）
        /// </summary>
        [DataMember(Name = "领用重量合计（kg）", Order = 30)]
        public decimal Total { get; set; }
        [DataMember(Name = "工区")]
        public string BranchNameShow { get { return BranchName + "/" + WorkAreaName; } }
        /// <summary>
        /// 分部
        /// </summary>
        public string BranchName { get; set; }
        /// <summary>
        /// 工区
        /// </summary>
        public string WorkAreaName { get; set; }
        /// <summary>
        /// 站点
        /// </summary>
        [DataMember(Name = "站点")]
        public string SiteName { get; set; }
    }
}