using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataEntity
{
    public class TbAreaAdministrationReauset
    {
        public int id { get; set; }
        public string AreaCode { get; set; }
        public string AreaName { get; set; }
        public string FK_AreaCode_F { get; set; }
        public string FK_AreaCode { get; set; }
        public int? Sort { get; set; }
        public string code { get; set; }
        public string parentid { get; set; }

        public int n_level { get; set; }
    }
    public class TbAreaAdministrationTreeReauset
    {

        [Description("区域名称")]
        public string AreaName { get; set; }

        [Description("区域编号")]
        public string AreaCode { get; set; }

        [Description("上级区域编号")]
        public string FK_AreaCode { get; set; }

        [Description("区域编号")]
        public string code { get; set; }

        [Description("区域名称")]
        public string name { get; set; }

        [Description("所有父级编码")]
        public string FK_AreaCode_F { get; set; }

        [Description("排序")]
        public int Sort { get; set; }
        public int ChildCount { get; set; }
        public int id { get; set; }
    }


}
