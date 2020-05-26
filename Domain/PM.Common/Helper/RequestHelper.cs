using System;
using System.Text.RegularExpressions;
using System.Web;

namespace PM.Common.Helper
{
    public class RequestHelper
    {
        #region 判断当前页面是否接收到了Post请求
        /// <summary>
        /// 判断当前页面是否接收到了Post请求
        /// </summary>
        /// <returns>是否接收到了Post请求</returns>
        public static bool IsPost()
        {
            return HttpContext.Current.Request.HttpMethod.Equals("POST");
        }
        #endregion

        #region 判断当前页面是否接收到了Get请求
        /// <summary>
        /// 判断当前页面是否接收到了Get请求
        /// </summary>
        /// <returns>是否接收到了Get请求</returns>
        public static bool IsGet()
        {
            return HttpContext.Current.Request.HttpMethod.Equals("GET");
        }
        #endregion

        #region 获得当前页面客户端的IP
        /// <summary>
        /// 获得当前页面客户端的IP
        /// </summary>
        /// <returns>当前页面客户端的IP</returns>
        public static string GetIP()
        {
            string result = String.Empty;

            result = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(result))
            {
                result = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            if (string.IsNullOrEmpty(result))
            {
                result = HttpContext.Current.Request.UserHostAddress;
            }

            if (string.IsNullOrEmpty(result) || !IsIP(result))
            {
                return "127.0.0.1";
            }

            return result;
        }
        #endregion

        #region 检测是否是正确的Url
        /// <summary>
        /// 检测是否是正确的Url
        /// </summary>
        /// <param name="strUrl">要验证的Url</param>
        /// <returns>判断结果</returns>
        public static bool IsURL(string strUrl)
        {
            return Regex.IsMatch(strUrl, @"^(http|https)\://([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&%\$\-]+)*@)*((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|localhost|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.(com|edu|gov|int|mil|net|org|biz|arpa|info|name|pro|aero|coop|museum|[a-zA-Z]{1,10}))(\:[0-9]+)*(/($|[a-zA-Z0-9\.\,\?\'\\\+&%\$#\=~_\-]+))*$");
        }
        #endregion

        #region 是否为ip
        /// <summary>
        /// 是否为ip
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIP(string ip)
        {
            return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }
        #endregion

        #region 获取GET方式传入的值

        /// <summary>
        /// 获得指定Url参数的值
        /// </summary>
        /// <param name="queryItem">页面请求的参数名称</param>
        /// <returns></returns>
        public static string GetQueryString(string queryItem)
        {
            return GetQueryString(queryItem, "");
        }

        /// <summary>
        /// 获取WEB页面传入的值(返回字符串类型)
        /// </summary>
        /// <param name="queryItem">页面请求的参数名称</param>
        /// <param name="defaultValue">如果没有获取到 返回该默认值</param>
        /// <returns></returns>
        public static string GetQueryString(string queryItem, string defaultValue)
        {
            string _str = HttpContext.Current.Request.QueryString[queryItem];
            if (string.IsNullOrEmpty(_str))
            {
                return defaultValue;
            }

            return _str;
        }

        /// <summary>
        /// 获取WEB页面传入的值(返回整型)
        /// </summary>
        /// <param name="queryItem">页面请求的参数名称</param>
        /// <param name="defaultValue">如果没有获取到 返回该默认值</param>
        /// <returns>返回整型</returns>
        public static int GetQueryInt32(string queryItem, int defaultValue)
        {
            queryItem = HttpContext.Current.Request.QueryString[queryItem];
            int num;
            if (!int.TryParse(queryItem, out num))
            {
                num = defaultValue;
            }
            return num;
        }

        /// <summary>
        /// 获取WEB页面传入的值(返回双浮点型)
        /// </summary>
        /// <param name="queryItem"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static double GetQuerySingle(string queryItem, double defaultValue)
        {
            queryItem = HttpContext.Current.Request.QueryString[queryItem];
            double num;
            if (!double.TryParse(queryItem, out num))
            {
                num = defaultValue;
            }
            return num;
        }

        /// <summary>
        /// 获取WEB页面传入的值(返回双浮点型)
        /// </summary>
        /// <param name="queryItem"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static decimal GetQueryDecimal(string queryItem, decimal defaultValue)
        {
            queryItem = HttpContext.Current.Request.QueryString[queryItem];
            decimal num;
            if (!decimal.TryParse(queryItem, out num))
            {
                num = defaultValue;
            }
            return num;
        }


        /// <summary>
        /// 获取WEB页面传入的值
        /// </summary>
        /// <param name="queryItem"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static long GetQueryLong(string queryItem, long defaultValue)
        {
            queryItem = HttpContext.Current.Request.QueryString[queryItem];
            long num;
            if (!long.TryParse(queryItem, out num))
            {
                num = defaultValue;
            }
            return num;
        }

        #endregion

        #region 获取POST方式传入的值

        /// <summary>
        /// 获得指定Url参数的值
        /// </summary>
        /// <param name="queryItem">页面请求的参数名称</param>
        /// <returns></returns>
        public static string GetFormString(string queryItem)
        {
            return GetFormString(queryItem, "");
        }

        /// <summary>
        /// 获取WEB页面传入的值(返回字符串类型)
        /// </summary>
        /// <param name="queryItem">页面请求的参数名称</param>
        /// <param name="defaultValue">如果没有获取到 返回该默认值</param>
        /// <returns></returns>
        public static string GetFormString(string queryItem, string defaultValue)
        {
            string _str = HttpContext.Current.Request.Form[queryItem];
            if (string.IsNullOrEmpty(_str))
            {
                return defaultValue;
            }

            return _str;
        }

        /// <summary>
        /// 获取WEB页面传入的值(返回整型)
        /// </summary>
        /// <param name="queryItem">页面请求的参数名称</param>
        /// <param name="defaultValue">如果没有获取到 返回该默认值</param>
        /// <returns>返回整型</returns>
        public static int GetFormInt32(string queryItem, int defaultValue)
        {
            queryItem = HttpContext.Current.Request.Form[queryItem];
            int num;
            if (!int.TryParse(queryItem, out num))
            {
                num = defaultValue;
            }
            return num;
        }

        /// <summary>
        /// 获取WEB页面传入的值(返回双浮点型)
        /// </summary>
        /// <param name="queryItem"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static double GetFormSingle(string queryItem, double defaultValue)
        {
            queryItem = HttpContext.Current.Request.Form[queryItem];
            double num;
            if (!double.TryParse(queryItem, out num))
            {
                num = defaultValue;
            }
            return num;
        }

        /// <summary>
        /// 获取WEB页面传入的值(返回双浮点型)
        /// </summary>
        /// <param name="queryItem"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static decimal GetFormDecimal(string queryItem, decimal defaultValue)
        {
            queryItem = HttpContext.Current.Request.Form[queryItem];
            decimal num;
            if (!decimal.TryParse(queryItem, out num))
            {
                num = defaultValue;
            }
            return num;
        }


        /// <summary>
        /// 获取WEB页面传入的值
        /// </summary>
        /// <param name="queryItem"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static long GetFormLong(string queryItem, long defaultValue)
        {
            queryItem = HttpContext.Current.Request.Form[queryItem];
            long num;
            if (!long.TryParse(queryItem, out num))
            {
                num = defaultValue;
            }
            return num;
        }

        #endregion

        #region 获取用户代理信息
        /// <summary>
        /// 获取用户代理信息
        /// </summary>
        /// <returns></returns>
        public static string GetUserAgent()
        {
            if (HttpContext.Current == null) return string.Empty;
            HttpBrowserCapabilities bc = HttpContext.Current.Request.Browser;
            return bc.Browser + "/" + bc.MajorVersion + "." + bc.MinorVersion;
        }
        #endregion
    }
}
