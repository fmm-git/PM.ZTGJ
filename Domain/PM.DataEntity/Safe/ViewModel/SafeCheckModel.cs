using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity.Safe.ViewModel
{
    public class SafeCheckRequest:PageSearchRequest
    {

        /// <summary>
        /// 检查类型
        /// </summary>
        public string CheckType { get; set; }

        /// <summary>
        /// 弹框收索条件
        /// </summary>
        public string keyword { get; set; }

        /// <summary>
        /// 检查编号
        /// </summary>
        public string SafeCheckCode { get; set; }

        /// <summary>
        /// 检查时间(开始)
        /// </summary>
        public DateTime? CheckTimeS { get; set; }

        /// <summary>
        /// 检查时间(结束)
        /// </summary>
        public DateTime? CheckTimeE { get; set; }
        /// <summary>
        /// 登录人编号
        /// </summary>
        public string UserCode { get; set; }
    }
}
