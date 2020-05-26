using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Data;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using System.Net.Http;
using System.Web.Script.Serialization;

namespace PM.Web
{
    public static class Json
    {
        public static object ToJson(this string Json)
        {
            return Json == null ? null : JsonConvert.DeserializeObject(Json);
        }
        public static string ToJson(this object obj)
        {
            var timeConverter = new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" };
            return JsonConvert.SerializeObject(obj, timeConverter);
        }
        public static string ToJson(this object obj, string datetimeformats)
        {
            var timeConverter = new IsoDateTimeConverter { DateTimeFormat = datetimeformats };
            return JsonConvert.SerializeObject(obj, timeConverter);
        }
        public static T ToObject<T>(this string Json)
        {
            return Json == null ? default(T) : JsonConvert.DeserializeObject<T>(Json);
        }
        public static List<T> ToList<T>(this string Json)
        {
            return Json == null ? null : JsonConvert.DeserializeObject<List<T>>(Json);
        }
        public static DataTable ToTable(this string Json)
        {
            return Json == null ? null : JsonConvert.DeserializeObject<DataTable>(Json);
        }
        public static JObject ToJObject(this string Json)
        {
            return Json == null ? JObject.Parse("{}") : JObject.Parse(Json.Replace("&nbsp;", ""));
        }
        public static HttpResponseMessage ToJsonApi(this Object obj)
        {
            var timeConverter = new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" };
            String str = JsonConvert.SerializeObject(obj, timeConverter);
            HttpResponseMessage result = new HttpResponseMessage { Content = new StringContent(str, Encoding.GetEncoding("UTF-8"), "application/json") };
            return result;
        }
    }

    public class JsonData : ISerializable
    {
        #region 属性

        /// <summary>
        /// 表示业务是否正常
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 返回消息,成功的消息和错误消息都在这里
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 用于返回复杂结果
        /// </summary>
        public object Content { get; set; }
        #endregion

        #region 方法
        /// <summary>
        /// 自定义序列化方法
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("IsSuccess", IsSuccess);
            info.AddValue("Message", Message);
            if (Content != null)
            {
                info.AddValue("Content", Convert.ChangeType(Content, Content.GetType()));
            }
            else
            {
                info.AddValue("Content", null);
            }
        }

        public JsonData() { }
        #endregion
    }
}
