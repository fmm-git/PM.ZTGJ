using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace PM.Web.Models.ExcelModel
{
    public class TransportProcessExcel
    {
        /// <summary>
        /// 卸货站点
        /// </summary>
        [DataMember(Name = "卸货站点")]
        public string SiteName { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        [DataMember(Name = "订单编号")]
        public string OrderCode { get; set; }

        /// <summary>
        /// 装车配送编号
        /// </summary>
        [DataMember(Name = "装车配送编号")]
        public string DistributionCode { get; set; }

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
        /// 卸货过程状态
        /// </summary>
        public int FlowState { get; set; }
        [DataMember(Name = "卸货过程状态")]
        public string FlowStateShow { get { return FlowState == 1 ? "正常" : "等待超过30分钟"; } }

        /// <summary>
        /// 计划配送时间
        /// </summary>
        [DataMember(Name = "计划配送时间")]
        public string PlanDistributionTime { get; set; }

        /// <summary>
        /// 实际配送时间
        /// </summary>
        [DataMember(Name = "实际配送时间")]
        public DateTime LoadCompleteTime { get; set; }

        /// <summary>
        /// 车牌号及司机
        /// </summary>
        [DataMember(Name = "车牌号及司机")]
        public string CarCph { get; set; }

        /// <summary>
        /// 出场时间
        /// </summary>
        [DataMember(Name = "出场时间")]
        public DateTime OutSpaceTime { get; set; }

        /// <summary>
        /// 运输到场时间
        /// </summary>
        [DataMember(Name = "运输到场时间")]
        public DateTime EnterSpaceTime { get; set; }

        /// <summary>
        /// 开始卸货时间
        /// </summary>
        [DataMember(Name = "开始卸货时间")]
        public DateTime StartDischargeTime { get; set; }

        /// <summary>
        /// 车辆出厂时间
        /// </summary>
        [DataMember(Name = "车辆出厂时间")]
        public DateTime LeaveFactoryTime { get; set; }
    }
}