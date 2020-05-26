using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace PM.Web.Models.ExcelModel
{
    public class SemiFinishedSignExcel
    {
        /// <summary>
        /// 站点名称
        /// </summary>
        [DataMember(Name = "站点名称")]
        public string SiteName { get; set; }
        /// <summary>
        /// 配送装车编号
        /// </summary>
        [DataMember(Name = "配送装车编号")]
        public string DistributionCode { get; set; }
        /// <summary>
        /// 签收编号
        /// </summary>
        [DataMember(Name = "签收编号")]
        public string SigninNuber { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        [DataMember(Name = "订单编号")]
        public string OrderCode { get; set; }
        /// <summary>
        /// 签收状态
        /// </summary>
        [DataMember(Name = "签收状态")]
        public string OperateState { get; set; }
        /// <summary>
        /// 签收时间
        /// </summary>
        [DataMember(Name = "签收时间")]
        public DateTime? SigninTime { get; set; }
        /// <summary>
        /// 类型编码
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
        public string UsePart { get; set; }
        /// <summary>
        /// 计划配送时间
        /// </summary>
        [DataMember(Name = "计划配送时间")]
        public string PlanDistributionTime { get; set; }
        /// <summary>
        /// 配送完成时间
        /// </summary>
        [DataMember(Name = "配送完成时间")]
        public string DeliveryCompleteTime { get; set; }
        /// <summary>
        /// 配送地址
        /// </summary>
        [DataMember(Name = "配送地址")]
        public string DistributionAddress { get; set; }
        /// <summary>
        /// 合计重量（kg）
        /// </summary>
        [DataMember(Name = "合计重量（kg）")]
        public decimal SumTotal { get; set; }
        /// <summary>
        /// 车牌号
        /// </summary>
        [DataMember(Name = "车牌号")]
        public string CarCph { get; set; }
        /// <summary>
        /// 驾驶员
        /// </summary>
        [DataMember(Name = "驾驶员")]
        public string CarUser { get; set; }
        /// <summary>
        /// 站点联系人
        /// </summary>
        [DataMember(Name = "站点联系人")]
        public string ContactsName { get; set; }
        /// <summary>
        /// 联系方式
        /// </summary>
        [DataMember(Name = "联系方式")]
        public string ContactWay { get; set; }
    }
}