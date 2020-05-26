using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PM.Web
{
    public static class TreeGrid
    {
        public static string TreeGridJson(this List<TreeGridModel> data)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{ \"rows\": [");
            sb.Append(TreeGridJson(data, -1, "0"));
            sb.Append("]}");
            return sb.ToString();
        }
        private static string TreeGridJson(List<TreeGridModel> data, int index, string parentId)
        {
            StringBuilder sb = new StringBuilder();
            var ChildNodeList = data.FindAll(t => t.parentId == parentId);
            if (ChildNodeList.Count > 0) { index++; }
            if (ChildNodeList.Count < 1 && parentId == "0")
                ChildNodeList = data;
            foreach (TreeGridModel entity in ChildNodeList)
            {
                string strJson = entity.entityJson;
                strJson = strJson.Insert(1, "\"loaded\":" + (entity.loaded == true ? false : true).ToString().ToLower() + ",");
                strJson = strJson.Insert(1, "\"expanded\":" + (entity.expanded).ToString().ToLower() + ",");
                strJson = strJson.Insert(1, "\"isLeaf\":" + (entity.isLeaf == true ? false : true).ToString().ToLower() + ",");
                strJson = strJson.Insert(1, "\"parent\":\"" + parentId + "\",");
                strJson = strJson.Insert(1, "\"level\":" + index + ",");
                sb.Append(strJson);
                sb.Append(TreeGridJson(data, index, entity.id));
            }
            return sb.ToString().Replace("}{", "},{");
        }


        public static string TreeGridAsynJson<T>(this List<TreeGridAsynModel<T>> data)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{ \"rows\": [");
            var index = 0;
            foreach (TreeGridAsynModel<T> entity in data)
            {
                index++;
                sb.Append("{");
                sb.Append("\"id\":\"" + entity.nodeId + "\",");
                sb.Append("\"cell\":[");
                string strJson = ValueToString(entity.entityJson);
                sb.Append(strJson);
                sb.Append("\"" + entity.n_level + "\",");
                sb.Append("\"" + entity.parentId + "\",");
                sb.Append("\"" + (entity.isLeaf == true).ToString().ToLower() + "\",");
                sb.Append("false");
                sb.Append("]}");
            }
            sb.Append("]}");
            return sb.ToString().Replace("}{", "},{");
        }


        private static  string ValueToString<T>(T obj )
        {
            StringBuilder sb = new StringBuilder();
            PropertyInfo[] peroperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in peroperties)
            {
                object[] objs = property.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (objs.Length > 0)
                {
                    var value = property.GetValue(obj, null);
                    sb.Append("\"" + value + "\",");
                }
            }
            return sb.ToString();
        }

    }
}
