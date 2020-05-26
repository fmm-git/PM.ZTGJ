using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataEntity
{
    public class PositionResponse
    {
        public string PositionName { get; set; }
        public string PositionCode { get; set; }
        public string ParentPositionCode { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyFullName { get; set; }
        public string CreateUser { get; set; }
        public string UserName { get; set; }

    }

    public class PositionTreeResponse
    {
        public string ParentCode { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 是否公司 1：是 2：否
        /// </summary>
        public int isCompany { get; set; }
    }

    public class PositionUserResponse
    {
        public int Id { get; set; }
        public int ID { get; set; }
        public string CompanyFullName { get; set; }
        public string DepartmentName { get; set; }
        public string PositionName { get; set; }
        public string UserCode { get; set; }
        public string UserName { get; set; }
        public string UserSex { get; set; }
    }
}
