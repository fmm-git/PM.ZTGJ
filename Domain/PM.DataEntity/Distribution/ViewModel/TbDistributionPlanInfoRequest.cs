using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity.Distribution.ViewModel
{
    public class TbDistributionPlanInfoRequest : PageSearchRequest
    {
        ////分部编号
        //public string BranchCode { get; set; }
        ////工区编号
        //public string WorkAreaCode { get; set; }

        //订单编号
        public string OrderCode { get; set; }
        //类型编号
        public string TypeCode { get; set; }
        //类型名称
        public string TypeName { get; set; }
        //配送状态
        public string DistributionStart { get; set; }
        //配送时间状态
        public string DistributionTimeStart { get; set; }
        //配送数据来源
        public string IsOffline { get; set; }
        //签收状态
        public string SignState { get; set; }
        /// <summary>
        /// 历史月份
        /// </summary>
        public DateTime? HistoryMonth { get; set; }
        //计划配送开始时间
        public DateTime? DistributionBegTime { get; set; }
        //计划配送结束时间
        public DateTime? DistributionEndTime { get; set; }
        public bool IsOutPut { get; set; }
        /// <summary>
        /// 问题类型
        /// </summary>
        public string ProblemType { get; set; }
    }
}
