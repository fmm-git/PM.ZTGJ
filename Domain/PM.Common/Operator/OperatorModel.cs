using System;

namespace PM.Common
{
    public class CurrentUserInfo
    {
        public string UserId { get; set; }
        public string UserCode { get; set; }
        public string UserName { get; set; }
        public string UserPwd { get; set; }
        public string ProjectId { get; set; }
        public string CompanyId { get; set; }
        public string ComPanyName { get; set; }
        public string OrgType { get; set; }
        public string ProjectOrgAllId { get; set; }
        public string ProjectOrgAllName { get; set; }
        //public string DepartmentCode { get; set; }
        //public string PositionCode { get; set; }
        public string RoleCode { get; set; }
        public string LoginToken { get; set; }
        public DateTime LoginTime { get; set; }
        public bool IsSystem { get; set; }
        public bool IsDriver { get; set; }
        /// <summary>
        /// 加工厂
        /// </summary>
        public string ProcessFactoryCode { get; set; }
        /// <summary>
        /// 加工厂
        /// </summary>
        public string ProcessFactoryName { get; set; }
    }
}
