using Newtonsoft.Json;
using System.Collections.Generic;

namespace PM.Common.Extension
{
    public static class JsonEx
    {
        /// <summary> 序列化为json格式 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonObj"></param>
        /// <returns></returns>
        public static string ToJson(this object jsonObj)
        {
            return JsonConvert.SerializeObject(jsonObj);
        }

        /// <summary> 反序列化到匿名对象 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T JsonToObj<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static List<T> JonsToList<T>(this string Json)
        {
            return JsonConvert.DeserializeObject<List<T>>(Json);
        }

    }
}
