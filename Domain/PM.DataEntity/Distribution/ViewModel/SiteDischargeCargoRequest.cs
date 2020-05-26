using Dos.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity.Distribution.ViewModel
{
    public class SiteDischargeCargoRequest : PageSearchRequest
    {

        /// <summary>
        /// 卸货时间
        /// </summary>
        public string DistributionTime { get; set; }

        /// <summary>
        /// 车牌号
        /// </summary>
        public string VehicleCode { get; set; }

        /// <summary>
        /// 车牌号
        /// </summary>
        public string CarCph { get; set; }

        /// <summary>
        /// 类型编号
        /// </summary>
        public string TypeCode { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 卸货状态
        /// </summary>
        public string DischargeType { get; set; }
        /// <summary>
        /// 弹框收索条件
        /// </summary>
        public string keyword { get; set; }
        /// <summary>
        /// 当前登录人Code
        /// </summary>
        public string UserCode { get; set; }
    }

    public class TbDistributionEntRequest : PageSearchRequest 
    {
        public string DistributionCode { get; set; }
        public string TypeCode { get; set; }
        public string UsePart { get; set; }
        public string SiteName { get; set; }
        /// <summary>
        /// 弹框收索条件
        /// </summary>
        public string keyword { get; set; }
    }

    public class SiteDischargeCargoDetailRequest : Entity
    {
    
		public string OrderCode {get;set;}
		public int WorkorderdetailId {get;set;}
        public int XhNumber { get; set; }
    }
}
