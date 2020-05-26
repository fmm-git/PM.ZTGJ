using Dos.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PM.DataEntity.Flow
{
    public partial class OrgEntity : Entity
    {
        public string pid{get;set;}
        public string id{get;set;}
        public string Name{get;set;}
        public string TypeName { get; set; }
        public string  OrgType { get; set; }
        public string ProjectId { get; set; }
        public string DeptId { get; set; }
        public string DeptName { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }

    }
}
