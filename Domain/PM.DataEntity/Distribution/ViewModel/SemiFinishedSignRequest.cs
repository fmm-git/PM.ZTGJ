using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity.Distribution.ViewModel
{
    public class SemiFinishedSignRequest : PageSearchRequest
    {

        /// <summary>
        /// 卸货时间
        /// </summary>
       public string SigninTime { get; set; }

       /// <summary>
       /// 车牌号
       /// </summary>
       public string VehicleCode { get; set; }


       /// <summary>
       /// 车牌号
       /// </summary>
       public string CarCph { get; set; }
       /// <summary>
       /// 订单编号
       /// </summary>
       public string OrderCode { get; set; }
       /// <summary>
       /// 类型编号
       /// </summary>
       public string TypeCode { get; set; }

      
        /// <summary>
        /// 弹框收索条件
        /// </summary>
        public string keyword { get; set; }

        /// <summary>
        /// 当前登录人编号
        /// </summary>
        public string  UserCode { get; set; }
        /// <summary>
        /// 签收编号
        /// </summary>
        public string SigninNuber { get; set; }
        /// <summary>
        /// 卸货编号
        /// </summary>
        public string DischargeCargoCode { get; set; }
        /// <summary>
        /// 签收状态
        /// </summary>
        public string OperateState { get; set; }
    }
}
