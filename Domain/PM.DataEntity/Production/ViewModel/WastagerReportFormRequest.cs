using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity.Production.ViewModel
{
    public class WastagerReportFormRequest : PageSearchRequest
    {
        /// <summary>
        ///规格编号
        /// </summary>
        public string SpecificationType { get; set; }
        /// <summary>
        /// 物资名称
        /// </summary>
        public string MaterialName { get; set; }

    }
}
