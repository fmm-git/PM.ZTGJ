using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataEntity
{
    public class TbSysMenuRequset : PageSearchRequest
    {
        public int ID { get; set; }
        public string MenuCode { get; set; }
        public string MenuName { get; set; }
        public string MenuUrl { get; set; }
        public string MenuPCode { get; set; }
        public string MenuIconCls { get; set; }
        public string OperationExamination { get; set; }
        public string OperationView { get; set; }
        public string OperationAdd { get; set; }
        public string OperationEdit { get; set; }
        public string OperationDel { get; set; }
        public string OperationOutput { get; set; }
        public string OperationOther1{ get; set; }
        public string OperationOther1IconCls { get; set; }
        public string OperationOther1Fun { get; set; }
        public string OperationOther2 { get; set; }
        public string OperationOther2IconCls { get; set; }
        public string OperationOther2Fun { get; set; }
        public string OperationOther3 { get; set; }
        public string OperationOther3IconCls { get; set; }
        public string OperationOther3Fun { get; set; }
        public string OperationOther4 { get; set; }
        public string OperationOther4IconCls { get; set; }
        public string OperationOther4Fun { get; set; }
        public string OperationOther5 { get; set; }
        public string OperationOther5IconCls { get; set; }
        public string OperationOther5Fun { get; set; }
        public int Sort { get; set; }
        public string IsShow { get; set; }
        public string DataAuthority { get; set; }
        public string DuanXinTemplate { get; set; }
        public string MenuType { get; set; }
        public string IsNoticeOrEarly { get; set; }
    }

    public class TbSysMenuApp  
    {
        public string MenuCode { get; set; }
        public string MenuName { get; set; }
        public string MenuUrl { get; set; }
        public string MenuPCode { get; set; }
        public string MenuIconCls { get; set; }
        public int Sort { get; set; }
        public string IsShow { get; set; }
        public string MenuType { get; set; }
    }
}
