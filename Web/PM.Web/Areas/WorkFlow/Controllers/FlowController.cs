using Newtonsoft.Json;
using PM.Business;
using PM.Business.Flow;
using PM.Common;
using PM.Domain.WebBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.WorkFlow.Controllers
{
    public class FlowController : BaseController
    {

        #region 视图
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult JudgeCriteria(string FlowCode, string JudgeValue = "", string JudgeSymbol = "", string JudgeRelation = "", string FieldCode = "")
        {
            ViewBag.FlowCode = FlowCode;
            ViewBag.JudgeValue = JudgeValue;
            ViewBag.JudgeSymbol = JudgeSymbol;
            ViewBag.JudgeRelation = JudgeRelation;
            ViewBag.FieldCode = FieldCode;
            return View();
        }
        public ActionResult FlowNodeDefine()
        {
            return View();
        }
        public ActionResult RowEditing()
        {
            return Content("");
        }
        public ActionResult FlowNodeJudgeCriteria(string FlowCode, string FlowNodeCode)
        {
            ViewBag.FlowNodeCode = FlowNodeCode;
            ViewBag.FlowCode = FlowCode;
            return View();
        }
        /// <summary>
        /// 流程图设计
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <param name="FormCode"></param>
        /// <param name="readOnly"></param>
        /// <returns></returns>
        public ActionResult FlowDesign(string FlowCode, string FormCode, bool readOnly = false)
        {
            ViewBag.ReadOnly = readOnly;
            ViewBag.FlowCode = FlowCode;
            ViewBag.FormCode = FormCode;
            return View();
        }

        /// <summary>
        /// 预警条件列表页面
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <param name="FlowNodeCode"></param>
        /// <returns></returns>
        public ActionResult FlowNodeEarlyWarningCondition(string FlowCode, string FlowNodeCode)
        {
            ViewBag.FlowNodeCode = FlowNodeCode;
            ViewBag.FlowCode = FlowCode;
            return View();
        }
        /// <summary>
        /// 预警条件新增、修改页面
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <param name="FlowNodeCode"></param>
        /// <returns></returns>
        public ActionResult EarlyWarningCriteria(string FlowCode, string FlowNodeCode, string type)
        {
            if (type != "view")
            {
                ViewBag.FlowNodeCode = FlowNodeCode;
                ViewBag.FlowCode = FlowCode;
                ViewBag.EarlyWarningCode = CreateCode.GetTableMaxCode("EW", "EarlyWarningCode", "TbFlowEarlyWarningCondition");
            }
            return View();
        }

        public ActionResult FreeChooseEarlyWarningUser(string FlowCode, string FlowNodeCode)
        {
            ViewBag.FlowNodeCode = FlowNodeCode;
            ViewBag.FlowCode = FlowCode;
            return View();
        }
        #endregion

        #region 操作
        [HttpPost]
        public string AddNode(string NodeInfo)
        {
            string json = new FlowNodeLogic().AddNodeInfo(NodeInfo).ToJson();
            return json;
        }
        [HttpPost]
        public ActionResult UpdateNode(string NodeInfo)
        {
            AjaxResult ajaxResut = new FlowNodeLogic().UpdateNode(NodeInfo);
            return Content(ajaxResut.ToJson());
        }
        [HttpPost]
        public string VerificationNode(string processData)
        {
            return AjaxResult.Success().ToString();
        }
        [HttpPost]
        public string GetNodePersonnel(string FlowCode, string FlowNodeCode)
        {
            return new FlowNodeLogic().GetNodePersonnel(FlowCode, FlowNodeCode);
        }
        #endregion

        #region 节点条件
        public string GetFiled(string FlowCode)
        {
            return new FlowNodeJudgeCriteriaLogic().GetFields(FlowCode);
        }
        public string GetTableName(string FlowCode)
        {
            return new FlowNodeJudgeCriteriaLogic().GetTableNameByFlowCode(FlowCode);
        }
        public string GetOptions()
        {
            return new FlowNodeJudgeCriteriaLogic().GetOperator();
        }
        public string GetLogical()
        {
            return new FlowNodeJudgeCriteriaLogic().GetLogical();
        }
        [HttpPost]
        public ActionResult AddJudgeCriteria(string JcInfo)
        {
            return Content(new FlowNodeJudgeCriteriaLogic().AddJudgeCriteria(JcInfo).ToJson());
        }
        public string GetJudgeCriteriaByFlowNode(string FlowCode, string FlowNodeCode)
        {
            return new FlowNodeJudgeCriteriaLogic().GetJudgeCriteriaByFlowNode(FlowCode, FlowNodeCode);
        }
        public string MathNodeJudgeCriter(string FlowCode, string FlowNodeCode)
        {
            return new FlowNodeJudgeCriteriaLogic().MathNodeJudgeCriter(FlowCode, FlowNodeCode);
        }
        #endregion

        #region 预警条件

        /// <summary>
        /// 获取预警条件列表信息
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <param name="FlowNodeCode"></param>
        /// <returns></returns>
        public string GetEarlyWarningConditionGridJson(string FlowCode, string FlowNodeCode)
        {
            return JsonConvert.SerializeObject(new TbFlowEarlyWarningConditionLogic().GetEarlyWarningConditionGridJson(FlowCode, FlowNodeCode));
        }
        /// <summary>
        /// 获取单条预警条件
        /// </summary>
        /// <param name="EarlyWarningCode"></param>
        /// <returns></returns>
        public ActionResult GetEarlyWarningFormJson(string EarlyWarningCode)
        {
            var data = new TbFlowEarlyWarningConditionLogic().GetEarlyWarningFormJson(EarlyWarningCode);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 添加预警条件
        /// </summary>
        /// <param name="EarylWarningInfo"></param>
        /// <param name="FlowCode"></param>
        /// <param name="FlowNodeCode"></param>
        /// <returns></returns>
        [HttpPost]
        public string AddEarylWarning(string EarylWarningInfo, string FlowCode, string FlowNodeCode)
        {
            string json = new TbFlowEarlyWarningConditionLogic().AddEarylWarning(EarylWarningInfo, FlowCode, FlowNodeCode).ToJson();
            return json;
        }
        /// <summary>
        /// 获取该预警条件下的预警人员信息
        /// </summary>
        /// <param name="EarlyWarningCode"></param>
        /// <returns></returns>
        public string GetNodeEarlyWaringPersonnel(string EarlyWarningCode)
        {
            return JsonConvert.SerializeObject(new TbFlowEarlyWarningConditionLogic().GetNodeEarlyWaringPersonnel(EarlyWarningCode));
        }
        /// <summary>
        /// 获取预警信息
        /// </summary>
        /// <returns></returns>
        public string GetEarlyWarningCount()
        {
            return JsonConvert.SerializeObject(new TbFlowEarlyWarningConditionLogic().GetEarlyWarningCount(Convert.ToString(Session["userid"])));
        }
        #endregion

        #region 新版流程TQ

        /// <summary>
        /// 流程节点定义页面
        /// </summary>
        /// <returns></returns>
        public ActionResult FlowNodeDefineNew()
        {
            return View();
        }

        public ActionResult FlowNodeEvent() 
        {
            return View();
        }
        /// <summary>
        /// 流程节点审批超时预警定义页面
        /// </summary>
        /// <returns></returns>
        public ActionResult FlowNodeEarlyWarningDefineNew(string type)
        {
            if (type == "add")
            {
                ViewBag.EarlyWarningCode = CreateCode.GetTableMaxCode("EW", "EarlyWarningCode", "TbFlowEarlyWarningCondition");
            }
            return View();
        }

        #endregion
    }
}
