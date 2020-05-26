using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataEntity
{
    public class Department
    {
        /// <summary>
        /// 标识ID
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 所属公司Code
        /// </summary>
        public string BelongCompanyCode { get; set; }
        /// <summary>
        /// 公司Code
        /// </summary>
        public string CompanyCode { get; set; }
        public string LFCode { get; set; }
        /// <summary>
        /// 部门编码Code
        /// </summary>
        public string DepartmentCode { get; set; }
        /// <summary>
        /// 父级部门Code
        /// </summary>
        public string ParentDepartmentCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ParentDepartmentCode_F { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FullCode { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepartmentName { get; set; }
        /// <summary>
        /// 部门主管
        /// </summary>
        public string DepartmentLeader { get;set; }
        /// <summary>
        /// 部门分管领导
        /// </summary>
        public string DepartmentSecLeader { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        public string Telephone { get; set; }
        /// <summary>
        /// 办公地址
        /// </summary>
        public string WorkSpace { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 排序号
        /// </summary>
        public int SortNumber { get; set; }
    }

    public class TbDepartmentRequest : PageSearchRequest 
    {
        public string DepartmentName { get; set; }
        public string DepartmentType { get; set; }
        public string DepartmentProjectId { get; set; }
    }
}
