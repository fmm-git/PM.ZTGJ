using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PM.Common.Helper;

namespace PM.Common.Extension
{
   public static class StringEx
    {
        private static readonly DateTime DefaultTime = DateTime.Parse("1900-01-01");

        #region ToFloat
        /// <summary>
        /// string转换为float
        /// </summary>
        /// <param name="strValue"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static float ToFloat(this string strValue, float defaultValue)
        {
            if ((strValue == null) || (strValue.Length > 10))
                return defaultValue;

            var intValue = defaultValue;
            var isFloat = strValue.IsFloat();
            if (isFloat)
                float.TryParse(strValue, out intValue);
            return intValue;
        }

        /// <summary>
        /// object转化为float
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static float ToFloat(this object obj, float defaultValue)
        {
            if (obj == null)
                return defaultValue;
            return ToFloat(obj.ToString(), defaultValue);
        }

        #endregion

        #region ToInt
        /// <summary>
        /// string转化为int
        /// </summary>
        /// <param name="str">字符</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static int ToInt(this string str, int defaultValue)
        {
            if (string.IsNullOrEmpty(str) || str.Trim().Length >= 11 ||
                !str.IsFloat())
                return defaultValue;

            int rv;
            if (int.TryParse(str, out rv))
                return rv;

            return Convert.ToInt32(ToFloat(str, defaultValue));
        }

        /// <summary>
        /// object转化为int
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int ToInt(this object obj, int defaultValue)
        {
            if (obj == null)
                return defaultValue;
            return ToInt(obj.ToString(), defaultValue);
        }
        #endregion

        #region ToLong

        /// <summary>
        /// string型转换为long型
        /// </summary>
        /// <param name="strValue"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static long ToLong(this string strValue, long defaultValue)
        {
            if (strValue == null) return defaultValue;

            long num;
            if (long.TryParse(strValue, out num))
            {
                return num;
            }
            else
            {
                decimal dem = 0;
                if (decimal.TryParse(strValue, out dem))
                {
                    return Convert.ToInt64(dem);
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// object转化为int
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static long ToLong(this object obj, long defaultValue)
        {
            if (obj == null)
                return defaultValue;
            return ToLong(obj.ToString(), defaultValue);
        }

        #endregion

        #region ToDateTime
        /// <summary>
        /// 将对象转换为日期时间类型
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string str, DateTime defaultValue)
        {
            if (string.IsNullOrEmpty(str)) return defaultValue;

            DateTime dateTime;
            if (DateTime.TryParse(str, out dateTime))
                return dateTime;

            return defaultValue;
        }

        /// <summary>
        /// 将对象转换为日期时间类型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this object obj, DateTime defaultValue)
        {
            if (obj == null) return defaultValue;
            return ToDateTime(obj.ToString(), defaultValue);
        }

        /// <summary>
        /// 将对象转换为日期时间类型
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <returns>转换后的int类型结果</returns>
        public static DateTime ToDateTime(this string str)
        {
            return ToDateTime(str, DefaultTime);
        }
        #endregion

        #region ToDecimal
        public static decimal ToDecimal(this string c, decimal defaultValue)
        {
            return (decimal)c.ToFloat((float)defaultValue);
        }

        public static decimal ToDecimal(this object c, decimal defaultValue)
        {
            return (decimal)c.ToFloat((float)defaultValue);
        }
        #endregion

        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="value">值</param>
        public static bool IsEmpty(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        #region 获取参数扩展

        #region 获取GET方式传入的值

        /// <summary>
        /// 获得指定Url参数的值
        /// </summary>
        /// <param name="queryItem">页面请求的参数名称</param>
        /// <returns></returns>
        public static string GetQueryString(this string queryItem)
        {
            return RequestHelper.GetQueryString(queryItem, "");
        }

        /// <summary>
        /// 获取WEB页面传入的值(返回字符串类型)
        /// </summary>
        /// <param name="queryItem">页面请求的参数名称</param>
        /// <param name="defaultValue">如果没有获取到 返回该默认值</param>
        /// <returns></returns>
        public static string GetQueryString(this string queryItem, string defaultValue)
        {
            return RequestHelper.GetQueryString(queryItem, defaultValue);
        }

        /// <summary>
        /// 获取WEB页面传入的值(返回整型)
        /// </summary>
        /// <param name="queryItem">页面请求的参数名称</param>
        /// <param name="defaultValue">如果没有获取到 返回该默认值</param>
        /// <returns>返回整型</returns>
        public static int GetQueryInt32(this string queryItem, int defaultValue)
        {
            return RequestHelper.GetQueryInt32(queryItem, defaultValue);
        }

        /// <summary>
        /// 获取WEB页面传入的值(返回双浮点型)
        /// </summary>
        /// <param name="queryItem"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static double GetQuerySingle(this string queryItem, double defaultValue)
        {
            return RequestHelper.GetQuerySingle(queryItem, defaultValue);
        }

        /// <summary>
        /// 获取WEB页面传入的值(返回双浮点型)
        /// </summary>
        /// <param name="queryItem"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static decimal GetQueryDecimal(this string queryItem, decimal defaultValue)
        {
            return RequestHelper.GetQueryDecimal(queryItem, defaultValue);
        }

        /// <summary>
        /// 获取WEB页面传入的值(返回双浮点型)
        /// </summary>
        /// <param name="queryItem"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static long GetQueryLong(this string queryItem, long defaultValue)
        {
            return RequestHelper.GetQueryLong(queryItem, defaultValue);
        }

        #endregion

        #region 获取POST方式传入的值

        /// <summary>
        /// 获得指定Url参数的值
        /// </summary>
        /// <param name="queryItem">页面请求的参数名称</param>
        /// <returns></returns>
        public static string GetFormString(this string queryItem)
        {
            return RequestHelper.GetFormString(queryItem, "");
        }

        /// <summary>
        /// 获取WEB页面传入的值(返回字符串类型)
        /// </summary>
        /// <param name="queryItem">页面请求的参数名称</param>
        /// <param name="defaultValue">如果没有获取到 返回该默认值</param>
        /// <returns></returns>
        public static string GetFormString(this string queryItem, string defaultValue)
        {
            return RequestHelper.GetFormString(queryItem, defaultValue);
        }

        /// <summary>
        /// 获取WEB页面传入的值(返回整型)
        /// </summary>
        /// <param name="queryItem">页面请求的参数名称</param>
        /// <param name="defaultValue">如果没有获取到 返回该默认值</param>
        /// <returns>返回整型</returns>
        public static int GetFormInt32(this string queryItem, int defaultValue)
        {
            return RequestHelper.GetFormInt32(queryItem, defaultValue);
        }

        /// <summary>
        /// 获取WEB页面传入的值(返回双浮点型)
        /// </summary>
        /// <param name="queryItem"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static double GetFormSingle(this string queryItem, double defaultValue)
        {
            return RequestHelper.GetFormSingle(queryItem, defaultValue);
        }

        /// <summary>
        /// 获取WEB页面传入的值(返回双浮点型)
        /// </summary>
        /// <param name="queryItem"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static decimal GetFormDecimal(this string queryItem, decimal defaultValue)
        {
            return RequestHelper.GetFormDecimal(queryItem, defaultValue);
        }


        /// <summary>
        /// 获取WEB页面传入的值(返回双浮点型)
        /// </summary>
        /// <param name="queryItem"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static long GetFormLong(this string queryItem, long defaultValue)
        {
            return RequestHelper.GetFormLong(queryItem, defaultValue);
        }

        #endregion

        #endregion
    }
}
