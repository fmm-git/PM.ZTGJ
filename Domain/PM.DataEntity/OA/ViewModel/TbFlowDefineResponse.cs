using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataEntity
{
    public class TbFlowDefineResponse
    {
        public int ID { get; set; }
        public string FlowCode { get; set; }
        public string FlowName { get; set; }
        public int FlowIsPublic { get; set; }
        public int FlowIsFree { get; set; }
        public string FlowDetail { get; set; }
        public string FormCode { get; set; }
        public int RollbackAttribute { get; set; }
        public string FlowSpOrCsType { get; set; }
        public string MenuName { get; set; }
    }
    public class OARequest : PageSearchRequest 
    {
        public string UserId { get; set; }
        public int state { get; set; }
        public DateTime? SDT { get; set; }
        public DateTime? EDT { get; set; }
    }
}
