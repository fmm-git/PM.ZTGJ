using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataEntity.Production
{
    public class ProcessingTechnologyRequest : PageSearchRequest
    {
        /// <summary>
        /// 加工工艺名称
        /// </summary>
        public string ProcessingTechnologyName { get; set; }
        public string PProcessingTechnologyName { get; set; }
    }
}
