namespace PM.DataEntity
{
    /// <summary>
    /// 
    /// </summary>
    public class GetRoleResponse
    {
        /// <summary>
        /// 角色主键
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// 公司Id
        /// </summary>
        public long CompanyId { get; set; }

        /// <summary>
        /// 角色说明
        /// </summary>
        public string Memo { get; set; }

        /// <summary>
        /// 角色主键
        /// </summary>
        //public string RoleIdView => RoleId.ToString();
    }
}
