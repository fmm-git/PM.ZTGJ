using PM.DataAccess.DbContext;
using PM.DataEntity;
using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataAccess
{
    public class SysLog
    {
        public static void inputLog(string code, string type)
        {
            if (!string.IsNullOrEmpty(Convert.ToString(System.Web.HttpContext.Current.Session["usercode"])))
            {
                TbSysLog tblog = new TbSysLog();
                tblog.ActionMenu = code;
                tblog.ActionType = type;
                tblog.HostName = GetHostAddress().HostName;//Request.ServerVariables.Get("Remote_Addr").ToString();//Dns.GetHostName();
                tblog.LogDate = DateTime.Now;
                tblog.UserCode = Convert.ToString(System.Web.HttpContext.Current.Session["usercode"]);
                //IPHostEntry ipEntity = Dns.GetHostEntry(tblog.HostName);
                //for (var i = 0; i < ipEntity.AddressList.Length; i++)
                //{
                //    if (ipEntity.AddressList[i].AddressFamily.ToString().Equals("InterNetwork"))
                //    {
                //        tblog.UserIP = ipEntity.AddressList[i].ToString();
                //        break;
                //    }
                //}
                tblog.UserIP = GetHostAddress().UserIP;
                tblog.UserName = Convert.ToString(System.Web.HttpContext.Current.Session["username"]);
                Db.Context.Insert<TbSysLog>(tblog);
            }
        }

        /// <summary>
        /// 获取客户端IP地址（无视代理）
        /// </summary>
        /// <returns>若失败则返回回送地址</returns>
        public static TbSysLog GetHostAddress()
        {
            TbSysLog sl = new TbSysLog();
            string userHostAddress = HttpContext.Current.Request.UserHostAddress;
            string userHostName = "";
            if (string.IsNullOrEmpty(userHostAddress))
            {
                userHostAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                userHostName = HttpContext.Current.Request.ServerVariables["REMOTE_HOST"];
            }

            //最后判断获取是否成功，并检查IP地址的格式（检查其格式非常重要）
            if (!string.IsNullOrEmpty(userHostAddress) && IsIP(userHostAddress))
            {
                sl.UserIP = userHostAddress;
                sl.HostName = userHostName;
                return sl;
            }
            else 
            {
                sl.HostName = Dns.GetHostName();
                IPHostEntry ipEntity = Dns.GetHostEntry(sl.HostName);
                for (var i = 0; i < ipEntity.AddressList.Length; i++)
                {
                    if (ipEntity.AddressList[i].AddressFamily.ToString().Equals("InterNetwork"))
                    {
                        sl.UserIP = ipEntity.AddressList[i].ToString();
                        break;
                    }
                }
                return sl;
            }
        }
        /// <summary>
        /// 检查IP地址格式
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIP(string ip)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }
    }
}
