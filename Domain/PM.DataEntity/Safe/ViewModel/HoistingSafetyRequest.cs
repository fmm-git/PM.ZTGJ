using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity.Safe.ViewModel
{
    public class HoistingSafetyRequest : PageSearchRequest
    {
        /// <summary>
        /// 监管人
        /// </summary>
        public string SuperviseUser { get; set; }

        /// <summary>
        /// 弹框收索条件
        /// </summary>
        public string keyword { get; set; }

    }
}
