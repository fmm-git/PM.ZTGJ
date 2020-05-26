using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity
{
    public class UserRequest
    {
        public int id { get; set; }
        public string depar_Code { get; set; }
        public string employee_Code { get; set; }
        public string UserCode { get; set; }
        public string UserName { get; set; }
        public string UserPwd { get; set; }
        public int UserClosed { get; set; }
        public DateTime? UserCreateTime { get; set; }
        public string PositionCode { get; set; }
        public DateTime? LastAccessMsgTime { get; set; }
    }

    public class TbUserRequest : PageSearchRequest 
    {
        public string UserName { get; set; }
        public string keyword { get; set; }
    }
}
