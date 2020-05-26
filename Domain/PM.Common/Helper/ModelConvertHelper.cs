using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dos.ORM;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace PM.Common.Helper
{
    /// <summary>    
    /// 实体转换辅助类
    /// </summary>    
    public class ModelConvertHelper<T> where T : new()
    {
        /// <summary>
        /// DataTable To List
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<T> ToList(DataTable dt)
        {
            // 定义集合    
            List<T> ts = new List<T>();

            // 获得此模型的类型   
            Type type = typeof(T);
            string tempName = "";

            foreach (DataRow dr in dt.Rows)
            {
                T t = new T();
                // 获得此模型的公共属性      
                PropertyInfo[] propertys = t.GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    tempName = pi.Name;  // 检查DataTable是否包含此列    

                    if (dt.Columns.Contains(tempName))
                    {
                        // 判断此属性是否有Setter      
                        if (!pi.CanWrite) continue;

                        object value = dr[tempName];
                        if (value != DBNull.Value)
                            pi.SetValue(t, value, null);
                    }
                }
                ts.Add(t);
            }
            return ts;
        }

        /// <summary>
        /// class to Dictionary
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ToDictionary()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            PropertyInfo[] peroperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in peroperties)
            {
                object[] objs = property.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (objs.Length > 0)
                {
                    dictionary.Add(property.Name, ((DescriptionAttribute)objs[0]).Description);
                }
            }
            return dictionary;
        }
    }

    public class GetAttribute<T>
    {
        /// <summary>
        /// 获取属性标记名称
        /// </summary>
        /// <returns></returns>
        public static List<ExcelHead> GetAttributeSignName()
        {
            var SignNames = new List<ExcelHead>();
            var MyProperties = typeof(T).GetProperties();
            foreach (var property in MyProperties)
            {
                var attributes = property.GetCustomAttributes(typeof(DataMemberAttribute), true);
                foreach (DataMemberAttribute dma in attributes)
                {
                    var item = new ExcelHead()
                    {
                        Key = property.Name,
                        Value = dma.Name
                    };
                    if (dma.Order > 0)
                        item.ColumnWidth = dma.Order;
                    SignNames.Add(item);
                }
            }
            return SignNames;
        }
    }

    public class ExcelHead
    {
        public ExcelHead()
        {
            this.ColumnWidth = 20;
        }
        public ExcelHead(string key, string value, int columnWidth = 20, bool islangt=false)
        {
            this.Key = key;
            this.Value = value;
            this.ColumnWidth = columnWidth;
            this.Islangt = islangt;
        }
        public string Key { get; set; }
        public string Value { get; set; }
        public int ColumnWidth { get; set; }
        public int ColumnWidthExcel { get { return this.ColumnWidth * 256; } }
        public bool Islangt { get; set; }
    }
}
