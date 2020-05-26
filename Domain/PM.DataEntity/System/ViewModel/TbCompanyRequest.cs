using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.DataEntity.System.ViewModel
{
    public class TbCompanyRequest : PageSearchRequest
    {
        //cp.CompanyCode,cp.CompanyFullName,cp.ParentCompanyCode,cp.Address,cp.OrgType
       public string CompanyCode { get; set; }
       public string CompanyFullName { get; set; }
       public string ParentCompanyCode { get; set; }
       public string Address { get; set; }
       public int OrgType { get; set; }
       //public string ProjectId { get; set; }   
       /// <summary>
       /// 弹框收索条件
       /// </summary>
       public string keyword { get; set; }
    }

    public class TbCompanyApp
    {
        public string CompanyCode { get; set; }
        public string CompanyFullName { get; set; }
        public string ParentCompanyCode { get; set; }
        public string Address { get; set; }
        public int OrgType { get; set; }
        public string ProjectId { get; set; }   
    }

    public class TbCompanyNew
    {
        public int ID { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyFullName { get; set; }
        public string ParentCompanyCode { get; set; }
        public string Address { get; set; }
        public int OrgType { get; set; }
        public string ProjectId { get; set; }
        public int Wtj { get; set; }//未提交
        public int Cswtj { get; set; }//超时未提交
        public int Cstj { get; set; }//超时提交
        public int Astj { get; set; }//按时提交
        public int JhHl { get; set; }//计划合理
        public int JhBz { get; set; }//计划不足
        public int JhGd { get; set; }//计划过多

    }
    public class TbCompanyNew1
    {
        public int ID { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyFullName { get; set; }
        public string ParentCompanyCode { get; set; }
        public string Address { get; set; }
        public int OrgType { get; set; }
        public string ProjectId { get; set; }
        public int SumOrderCount { get; set; }//总订单数
        public int SumJjCount { get; set; }//加急订单数
        public int SumZhCount { get; set; }//滞后订单数

    }
}
