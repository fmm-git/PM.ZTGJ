using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataEntity
{
    public class InitFlowPerformModel
    {
        public string FlowCode { get; set; }
        public string FormCode { get; set; }
        public string FormDataCode { get; set; }
        public string UserCode { get; set; }
        public string FlowTitle { get; set; }
        public string FlowLevel { get; set; }
        public DateTime? FinalCutoffTime { get; set; }
    }
}
