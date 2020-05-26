using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataEntity
{
    public class PositionSearchRequest
    {
        /// <summary>
        /// 岗位编号
        /// </summary>
        public string PositionCode { get; set; }

        /// <summary>
        /// 岗位名称
        /// </summary>
        public string PositionName { get; set; }

        /// <summary>
        /// 部门编号
        /// </summary>
        public string DepartmentCode { get; set; }

        /// <summary>
        /// 部门编号(上级部门)
        /// </summary>
        public string PDepartmentCode { get; set; }

        /// <summary>
        /// 上级岗位
        /// </summary>
        public string FullCode { get; set; }
    }

    public class PositionRequest
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// 上级岗位编号
        /// </summary>
        public string ParentPositionCode { get; set; }

        /// <summary>
        /// 岗位编号
        /// </summary>
        public string PositionCode { get; set; }

        /// <summary>
        /// 岗位名称
        /// </summary>
        public string PositionName { get; set; }

        /// <summary>
        /// 部门编号
        /// </summary>
        public string DepartmentCode { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 录入人
        /// </summary>
        public string CreateUser { get; set; }
    }

    public class PositionUserRequset : PageSearchRequest
    {
        public int Id { get; set; }
        public string PositionCode { get; set; }
        public string UserCode { get; set; }

        public string keyword { get; set; }
    }
}
