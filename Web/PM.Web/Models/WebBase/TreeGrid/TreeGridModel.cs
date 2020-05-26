using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Web
{
    public class TreeGridModel
    {
        public string id { get; set; }
        //父级ID
        public string parentId { get; set; }
        //分级展示内容
        public string text { get; set; }
        //是否是分支
        public bool isLeaf { get; set; }
        
        public bool expanded { get; set; }
        //
        public bool loaded { get; set; }
        //实体数据
        public string entityJson { get; set; }
    }

    public class TreeGridAsynModel<T>
    {
        public string nodeId { get; set; }
        //父级ID
        public string parentId { get; set; }

        //是否是分支
        public bool isLeaf { get; set; }

        public int n_level { get; set; }

        //实体数据
        public T entityJson { get; set; }

    }
}
