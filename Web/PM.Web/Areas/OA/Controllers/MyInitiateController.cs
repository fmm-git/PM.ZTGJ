using Newtonsoft.Json;
using PM.Business;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.OA.Controllers
{
    public class MyInitiateController : BaseController
    {
        FlowPerformLogic _flowperform = new FlowPerformLogic();
        public ActionResult Index()
        {
            return View();
        }
       
        public string GetMyInitiate(PageSearchRequest pt,string param)
        {
            MyInitiate request = new MyInitiate();
            if (string.IsNullOrEmpty(param))
            {
                request.status = -1;
                request.sdt = DateTime.Now.AddDays(-30);
                request.edt = DateTime.Now.AddDays(1);
            }
            else
            {
                request = PM.Common.Extension.JsonEx.JsonToObj<MyInitiate>(param);
            }
            return _flowperform.GetMyInitiate(pt,request.status, Convert.ToString(Session["usercode"]), request.sdt, request.edt.AddDays(1));
        }
        
        public string GetTreeGridJson()
        {
            return _flowperform.GetMyInitiateStatus(Convert.ToString(Session["usercode"]));
        }
        /// <summary>
        /// 撤销
        /// </summary>
        /// <param name="flowperformid"></param>
        /// <returns></returns>
        public ActionResult Cancel(string flowperformid)
        {
            return Content(_flowperform.Cancel(flowperformid).ToJson());
        }
        private class MyInitiate
        {
            public int status { get; set; }
            public DateTime sdt { get; set; }
            public DateTime edt { get; set; }
        }

    }
}
