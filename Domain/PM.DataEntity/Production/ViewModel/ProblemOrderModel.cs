using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity.Production.ViewModel
{
    public class ProblemOrderRequest: PageSearchRequest
    {

        public string CompanyCode { get; set; }

        /// <summary>
        /// 撤销状态
        /// </summary>
        public string RevokeStatus { get; set; }

        /// <summary>
        /// 弹框收索条件
        /// </summary>
        public string keyword { get; set; }

        /// <summary>
        /// 原订单编号
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 类型编码
        /// </summary>
        public string TypeCode { get; set; }
        /// <summary>
        /// 计划配送开始时间
        /// </summary>
        public DateTime? BegTime { get; set; }
        /// <summary>
        /// 计划配送结束时间
        /// </summary>
        public DateTime? EndTiem { get; set; }
        public bool IsCost { get; set; }
        public bool IsOutPut { get; set; }
    }
}
