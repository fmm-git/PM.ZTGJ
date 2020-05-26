using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataEntity
{
    public class TbRoleRequset : PageSearchRequest
    {
        public int id { get; set; }
        public string RoleCode { get; set; }
        public string DepartmentId { get; set; }
        public string RoleName { get; set; }
        public string RoleDetail { get; set; }
        public string State { get; set; }
    }
}
