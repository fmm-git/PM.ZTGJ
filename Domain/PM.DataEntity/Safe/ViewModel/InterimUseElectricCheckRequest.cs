using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity.Safe.ViewModel
{
    public class InterimUseElectricCheckRequest:PageSearchRequest
    {
        /// <summary>
        /// 检查编号
        /// </summary>
        public string CheckCode { get; set; }

        /// <summary>
        /// 检查时间(开始)
        /// </summary>
        public DateTime? CheckTimeS { get; set; }

        /// <summary>
        /// 检查时间(结束)
        /// </summary>
        public DateTime? CheckTimeE { get; set; }
    }
}
