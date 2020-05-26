using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity.RawMaterial.ViewModel
{
    public class StatisticsReportFormRequest : PageSearchRequest
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
        /// 规格
        /// </summary>
        public string SpecificationModel { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public string BegTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime { get; set; }
        public DateTime? HistoryMonth { get; set; }

        public string level { get; set; }
    }
    public class YearList
    {
        public string Year { get; set; }
        public string YearName { get; set; }   
    }
}
