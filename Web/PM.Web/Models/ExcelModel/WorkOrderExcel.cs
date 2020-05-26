using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace PM.Web.Models.ExcelModel
{
    public class WorkOrderExcel
    {
        /// <summary>
        /// 站点名称
        /// </summary>
        [DataMember(Name = "站点名称")]
        public string SiteName { get; set; }
        /// <summary>
        /// 订单编号及类型
        /// </summary>
        [DataMember(Name = "订单编号及类型", Order = 30)]
        public string OrderCode_UrgentDegreeNewShow { get; set; }
        public string OrderCode { get; set; }
        public string UrgentDegreeNewShow
        {
            get
            {
                if (!OrderStart.Equals("部分变更") && !OrderStart.Equals("全部变更")) return UrgentDegreeNew;
                return UrgentDegreeNew + " / " + OrderStart;
            }
        }
        public string UrgentDegreeNew { get; set; }
        public string OrderStart { get; set; }
        /// <summary>
        /// 类型编号
        /// </summary>
        [DataMember(Name = "类型编号")]
        public string TypeCode { get; set; }
        /// <summary>
        /// 类型名称及使用部位
        /// </summary>
        [DataMember(Name = "类型名称及使用部位", Order = 30)]
        public string TypeName_UsePart { get; set; }
        public string TypeName { get; set; }
        public string UsePart { get; set; }
        /// <summary>
        /// 总量合计(kg)
        /// </summary>
        [DataMember(Name = "总量合计(kg)")]
        public decimal WeightTotal { get; set; }
        /// <summary>
        /// 配送时间
        /// </summary>
        [DataMember(Name = "配送时间")]
        public string DistributionTimeShow_LoadCompleteTimeShow { get; set; }
        public string DistributionTimeShow
        {
            get
            {
                return "计划:" + DistributionTime.ToString("yyyy-MM-dd");
            }
        }
        public DateTime DistributionTime { get; set; }
        public string LoadCompleteTimeShow
        {
            get
            {
                string str = "实际:";
                if (string.IsNullOrEmpty(LoadCompleteTime)) return str;
                return str + DateTime.Parse(LoadCompleteTime).ToString("yyyy-MM-dd");
            }
        }
        public string LoadCompleteTime { get; set; }
        /// <summary>
        /// 配送状态
        /// </summary>
        [DataMember(Name = "配送状态")]
        public string DistributionStartShow_DistributionStart2 { get; set; }
        public string DistributionStartShow
        {
            get
            {
                if (DistributionStart.Equals("配送完成") || DistributionStart.Equals("部分配送"))
                {
                    return DistributionStart;
                }
                else
                {
                    if (DateTime.Now <= DistributionTime)
                        return "未配送";
                    else
                        return "超期未配送";
                }
            }
        }
        public string DistributionStart { get; set; }
        public string DistributionStart2
        {
            get
            {
                if (!string.IsNullOrEmpty(LoadCompleteTime))
                {
                    if (DateTime.Parse(LoadCompleteTime) > DistributionTime)
                        return "延迟配送";
                    else if (DateTime.Parse(LoadCompleteTime) < DistributionTime)
                        return "提前配送";
                    else
                        return "正常配送";
                }
                else
                {
                    return "";
                }
            }

        }
        /// <summary>
        /// 配送数据来源
        /// </summary>
        [DataMember(Name = "配送数据来源")]
        public string IsOfflineShow
        {
            get
            {
                if (IsOffline == 0)
                {
                    if (DistributionStart != "" && DistributionStart != "未配送")
                    {
                        return "系统记录";
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "线下补录";
                }
            }
        }
        public int IsOffline { get; set; }
        /// <summary>
        /// 工区签收
        /// </summary>
        [DataMember(Name = "工区签收")]
        public string SignState { get; set; }
        /// <summary>
        /// 录入人及时间
        /// </summary>
        [DataMember(Name = "录入人及时间")]
        public string UserName_InsertTime { get; set; }
        /// <summary>
        /// 录入人
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 录入时间
        /// </summary>
        public DateTime InsertTime { get; set; }

    }
}