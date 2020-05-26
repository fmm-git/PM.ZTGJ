using PM.Common;
namespace PM.DataEntity
{
    /// <summary>
    /// 分页信息
    /// </summary>
    public class PageSearchRequest
    {
        public PageSearchRequest()
        {
            this.sord = "desc";
            this.rows = 50;
            this.page = 1;
            if (OperatorProvider.Provider.CurrentUser != null)
            {
                this.ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
                if (OperatorProvider.Provider.CurrentUser.OrgType != "1")
                    this.SiteCode = OperatorProvider.Provider.CurrentUser.CompanyId;
                this.ProcessFactoryCode = OperatorProvider.Provider.CurrentUser.ProcessFactoryCode;
            }
        }
        /// <summary>
        /// 项目id
        /// </summary>
        public string ProjectId { get; set; }
        /// <summary>
        /// 组织机构id
        /// </summary>
        public string CompanyId { get; set; }

        /// <summary>
        /// 组织机构编码
        /// </summary>
        public string SiteCode { get; set; }

        /// <summary>
        /// 加工厂编码
        /// </summary>
        public string ProcessFactoryCode { get; set; }

        /// <summary>
        /// 每页行数
        /// </summary>
        public int rows { get; set; }
        /// <summary>
        /// 当前页
        /// </summary>
        public int page { get; set; }
        /// <summary>
        /// 排序列
        /// </summary>
        public string sidx { get; set; }
        /// <summary>
        /// 排序类型
        /// </summary>
        public string sord { get; set; }
        /// <summary>
        /// 总记录数
        /// </summary>
        public int records { get; set; }
        /// <summary>
        /// 总页数
        /// </summary>
        public int total
        {
            get
            {
                if (records > 0)
                {
                    return records % this.rows == 0 ? records / this.rows : records / this.rows + 1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
