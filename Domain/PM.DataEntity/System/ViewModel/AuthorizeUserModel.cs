using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataEntity
{
    public class AuthorizeUserRequest
    {
        /// <summary>
        /// 
        /// </summary>
        public string MenuCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int DataID { get; set; }

        public List<AuthorizeUser> AuthorizeUser { get; set; }
    }
    public class AuthorizeUserResponse : AuthorizeUserRequest
    {
    }

    public class AuthorizeUser
    {
        public int ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MenuCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int DataID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SQR { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BSQR { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime SDT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime EDT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Reamrk { get; set; }

        public string UserName { get; set; }
    }
}
