using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity.Production.ViewModel
{
    public class RawMarchivesRequest : PageSearchRequest
    {
        /// <summary>
        /// 原材料编号
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        /// 原材料名称
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// 规格型号
        /// </summary>
        public string SpecificationModel { get; set; }

        /// <summary>
        /// 钢筋类型
        /// </summary>
        public string RebarType { get; set; }
    }
}
