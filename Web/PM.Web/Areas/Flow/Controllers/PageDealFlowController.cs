using Dos.ORM;
using PM.Business;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.Flow.Controllers
{
    public class PageDealFlowController : Controller
    {
        private readonly FlowDefineLogic _flowDefineImp = new FlowDefineLogic();
        private readonly FlowPerformLogic _flowPerform = new FlowPerformLogic();

        #region 列表信息

        public ActionResult ChangeFlow()
        {
            ViewBag.UserCode = Session["usercode"];
            return View();
        }
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 获取该表单下的流程
        /// </summary>
        /// <param name="FormCode"></param>
        /// <returns></returns>
        public ActionResult GetGridJson(string FormCode,string ProjectId) 
        {
            var data = _flowDefineImp.GetChangeFlow(FormCode, ProjectId).ToList();
            return Content(data.ToJson());
        }
        [HttpPost]
        public string InitFlowPerform(string param)
        {
            var json = PM.Common.Extension.JsonEx.JsonToObj<InitFlowPerformModel>(param);
            return _flowPerform.InitFlowPerform(json.FlowCode, json.FormCode, json.FormDataCode, json.UserCode, json.FlowTitle, json.FlowLevel, json.FinalCutoffTime).ToJson();
        }
        /// <summary>
        /// 获取审批信息
        /// </summary>
        /// <param name="FormCode"></param>
        /// <param name="FormDataCode"></param>
        /// <returns></returns>
        public string GetApprovalInfo(string FormCode, string FormDataCode)
        {
            return _flowPerform.GetApprovalInfo(FormCode,FormDataCode);
        }
        #endregion
    }
}
