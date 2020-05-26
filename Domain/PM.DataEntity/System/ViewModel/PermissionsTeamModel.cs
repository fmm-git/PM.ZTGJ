using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataEntity
{
    /// <summary>
    /// 团队信息
    /// </summary>
    public class PermissionsTeamRequest
    {
        /// <summary>
        /// 
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TeamNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TeamName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MenuCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CreateUser { get; set; }

        public List<PermissionsTeamMemberRequest> PermissionsTeamMember { get; set; }
    }

    /// <summary>
    /// 团队人员
    /// </summary>
    public class PermissionsTeamMemberRequest
    {
        /// <summary>
        /// 
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TeamNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UserCode { get; set; }

        public string UserName { get; set; }
    }

    public class PermissionsTeamResponse : PermissionsTeamRequest
    {
        public string MenuName { get; set; }
    }
}
