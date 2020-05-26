using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataEntity
{
    public class AuthorizeActionModel
    {
        public string F_Id { set; get; }
        public string F_UrlAddress { set; get; }
    }

    public class AuthorizationCodeIdModel
    {
        /// <summary>
        /// 是否菜单授权
        /// </summary>
        public bool IsAuthorize { get; set; }

        /// <summary>
        /// 用户编号
        /// </summary>
        public List<string> UserCodes { get; set; }

        /// <summary>
        /// 数据ID
        /// </summary>
        public List<int> Ids { get; set; }
        public string UserCodesStr
        {
            get
            {
                return UserCodes != null ? "'" + string.Join("','", UserCodes) + "'" : "";
            }
        }
        public string IdsStr
        {
            get
            {
                return Ids != null ? string.Join(",", Ids) : "";
            }
        }
    }

    public class AuthorizationParameterModel
    {
        public AuthorizationParameterModel()
        {
        }
        public AuthorizationParameterModel(string formCode)
        {
            this.FormCode = formCode;
        }
        /// <summary>
        /// 菜单编号
        /// </summary>
        public string FormCode { get; set; }

        /// <summary>
        /// 用户编号
        /// </summary>
        public string UserCode { get; set; }
    }
}
