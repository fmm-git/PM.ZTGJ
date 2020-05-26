using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataEntity
{
    public class TbUserRoleRequset : PageSearchRequest
    {
       public int ID { get; set; }
       //public string ProjectId { get; set; }
       public string RoleCode { get; set; }
       public string DeptId { get; set; }
       public string CompanyId { get; set; }
       public string UserCode { get; set; }
       public string UserName { get; set; }
       public string DepartmentName { get; set; }
       public string CompanyFullName { get; set; }
    }
    public class TbUserRoleUserRequset
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
        public string RoleCode { get; set; }
        public string State { get; set; }
        public string UserCode { get; set; }
        public string UserName { get; set; }
    }
    public class UserListRequset : PageSearchRequest
    {
        public int type { get; set; }
        public string code { get; set; }
        public string keyword { get; set; }
    }

}
