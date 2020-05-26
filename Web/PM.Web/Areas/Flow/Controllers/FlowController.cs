using ExecuteDAL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PM.Business;
using PM.Collector;
using PM.Common;
using PM.Common.Helper;
using PM.DataEntity;
using PM.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.Flow.Controllers
{
    /// <summary>
    /// 流程模块功能管理
    /// </summary>
    public class FlowController : BaseController
    {
        private readonly FlowPerformManageLogic flowPerformImp = new FlowPerformManageLogic();
        //流程节点
        private readonly FlowNodeLogic FlowNode = new FlowNodeLogic();

        //流程节点UI配置
        private readonly FlowNodeUILogic FlowNodeUI = new FlowNodeUILogic();


        private readonly FlowPerformLogic _flowperform = new FlowPerformLogic();

        #region View

        /// <summary>
        /// 流程及审批意见查看
        /// </summary>
        /// <returns></returns>
        public ActionResult FlowOpinionsView(string flowPerformID)
        {
            ViewBag.FlowPerformID = flowPerformID;
            return View();
        }
        public ActionResult FreeChooseFlowUser()
        {
            return View();
        }
        /// <summary>
        /// 进度计划
        /// </summary>
        /// <returns></returns>
        public ActionResult FlowProjectTask(string RegProjcetCode)
        {
            ViewBag.RegProjcetCode = RegProjcetCode;
            return View();
        }

        /// <summary>
        /// 审批流程图查看
        /// </summary>
        /// <returns></returns>
        public ActionResult FlowView(string flowPerformID)
        {
            StringBuilder sbStr = new StringBuilder();
            DataSet dsGrid = (DataSet)Collector.AshxSql.AutomaticCollection("getFlowPerformNodeColor", null, "", true);
            DataTable dt = dsGrid.Tables[0];
            sbStr.Append("<ul style='list-style:none; font-size:12px;'>");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                sbStr.AppendFormat("<li style='float:left; height:35px; text-align:center; margin-right:10px;'><div style='width:10px; height:10px; float:left; margin-top:12px; background-color:{0};'></div><div style='float:left; line-height:35px; margin-left:4px;'>{1}</div></li>", dt.Rows[i]["background"], dt.Rows[i]["StateDetail"]);
            }
            sbStr.Append("</ul>");
            ViewBag.ColorDescription = sbStr.ToString();
            ViewBag.FlowPerformID = flowPerformID;
            return View();
        }


        /// <summary>
        /// 流程图设计
        /// </summary>
        /// <returns></returns>
        public ActionResult FlowChartDesign(string FlowCode, string FormCode, bool readOnly = false)
        {
            ViewBag.ReadOnly = readOnly;
            ViewBag.FlowCode = FlowCode;
            ViewBag.FormCode = FormCode;
            return View();
        }

        /// <summary>
        /// 流程定义
        /// </summary>
        /// <returns></returns>
        public ActionResult FlowDefine()
        {
            return View();
        }

        /// <summary>
        /// 流程图展示
        /// </summary>
        /// <returns></returns>
        public ActionResult FlowDisplay(string FlowPerformID, string FlowCode, string Jurisdiction)
        {
            StringBuilder sbStr = new StringBuilder();
            DataSet dsGrid = (DataSet)AshxSql.AutomaticCollection("getFlowPerformNodeColor", null, "", true);
            DataTable dt = dsGrid.Tables[0];
            sbStr.Append("<ul style='list-style:none; font-size:12px;'>");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                sbStr.AppendFormat("<li style='float:left; height:35px; text-align:center; margin-right:10px;'><div style='width:10px; height:10px; float:left; margin-top:12px; background-color:{0};'></div><div style='float:left; line-height:35px; margin-left:4px;'>{1}</div></li>", dt.Rows[i]["background"], dt.Rows[i]["StateDetail"]);
            }
            sbStr.Append("</ul>");
            ViewBag.ColorDescription = sbStr.ToString();
            ViewBag.FlowPerformID = FlowPerformID;
            ViewBag.FlowCode = FlowCode;
            ViewBag.Jurisdiction = Jurisdiction;
            return View();
        }

        /// <summary>
        /// 事件设置
        /// </summary>
        /// <returns></returns>
        public ActionResult FlowEvent(string action, string eventtype, string FlowCode, string FlowNodeCode)
        {
            ViewBag.action = action;
            ViewBag.eventtype = eventtype;
            ViewBag.FlowCode = FlowCode;
            ViewBag.FlowNodeCode = FlowNodeCode;
            return View();
        }

        /// <summary>
        /// 流程审批
        /// </summary>
        /// <returns></returns>
        public ActionResult FlowExamine()
        {
            ViewBag.FlowPerformID = RequestHelper.GetQueryString("FlowPerformID");
            ViewBag.FormCode = RequestHelper.GetQueryString("FormCode");
            ViewBag.FlowNodeCode = RequestHelper.GetQueryString("FlowNodeCode");
            ViewBag.USER_CODE = UserInfo.USER_CODE;
            ViewBag.Examination = RequestHelper.GetQueryString("Examination");
            ViewBag.PerformState = RequestHelper.GetQueryString("PerformState");
            ViewBag.User_Type = RequestHelper.GetQueryString("User_Type");
            ViewBag.FormTitle = RequestHelper.GetQueryString("FormTitle");
            return View();
        }

        /// <summary>
        /// 表单流程节点区域设置
        /// </summary>
        /// <returns></returns>
        public ActionResult FlowNodeFormPart(string action, string FlowCode, string FlowNodeCode)
        {
            ViewBag.action = action;
            ViewBag.FlowCode = FlowCode;
            ViewBag.FlowNodeCode = FlowNodeCode;
            return View();
        }

        /// <summary>
        /// 流程过程定义
        /// </summary>
        /// <returns></returns>
        public ActionResult ProcessDefinition()
        {
            ViewBag.action = RequestHelper.GetQueryString("action");
            ViewBag.FlowCode = RequestHelper.GetQueryString("FlowCode");
            ViewBag.FlowNodeCode = RequestHelper.GetQueryString("FlowNodeCode");
            ViewBag.Left = RequestHelper.GetQueryString("Left");
            ViewBag.Top = RequestHelper.GetQueryString("Top");
            ViewBag.sourceId = RequestHelper.GetQueryString("sourceId");
            ViewBag.targetId = RequestHelper.GetQueryString("targetId");
            ViewBag.Coordinate = RequestHelper.GetQueryString("Coordinate");
            ViewBag.Jurisdiction = RequestHelper.GetQueryString("Jurisdiction");
            ViewBag.Sign = RequestHelper.GetQueryString("Sign");
            ViewBag.addSign = RequestHelper.GetQueryString("addSign");
            ViewBag.FormCode = RequestHelper.GetQueryString("FormCode");
            return View();
        }

        /// <summary>
        /// 我的办文编辑界面
        /// </summary>
        /// <returns></returns>
        public ActionResult OfficeWindow()
        {
            ViewBag.action = RequestHelper.GetQueryString("action");
            ViewBag.FormDataID = RequestHelper.GetQueryString("FormDataID");
            return View();
        }

        /// <summary>
        /// 设置自由选人节点的执行人
        /// </summary>
        /// <returns></returns>
        public ActionResult FreeChildNodeUser()
        {
            return View();
        }

        #endregion

        #region 新版
        /// <summary>
        /// 获取节点信息
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <returns></returns>
        public string GetNodeList(string FlowCode)
        {
            return FlowNode.GetNodeList(FlowCode);
        }
        public ActionResult UpdataFlowNodeUI(string FlowCode, string processInfo)
        {
            AjaxResult result = FlowNodeUI.UpdataFlowNodeUI(FlowCode, processInfo);
            return Content(result.ToJson());
        }
        public string GetFlowNodeName(string FlowCode)
        {
            return FlowNode.GetFlowNodeName(FlowCode);
        }
        [HttpPost]
        public ActionResult GetNode(string FlowCode, string FlowNodeCode)
        {
            AjaxResult a = new AjaxResult();
            a.data = FlowNode.GetNode(FlowCode, FlowNodeCode).data.ToJson().ToString();
            a.state = "success";
            a.message = "";
            return Content(a.ToJson());
        }
        public string GetLastNodeInfo(string FlowCode, string ChildNodeCode)
        {
            return FlowNode.GetLastNodeInfo(FlowCode, ChildNodeCode);
        }
        [HttpPost]
        public string GetDepartmentByOrgType(int OrgType)
        {
            return FlowNode.GetDepartmentByOrgType(OrgType);
        }
        [HttpPost]
        public string GetRoleByDepCode(string DepCode)
        {
            return FlowNode.GetRoleByDepCode(DepCode);
        }

        public ActionResult GetDeptOrRoleOrUser(int OrgType, string CompanyId)
        {
            var list = FlowNode.GetDeptOrRoleOrUser(OrgType, CompanyId);
            var treeList = new List<TreeGridModel>();

            foreach (var item in list)
            {
                bool hasChildren = list.Count(p => p.pid == item.id) == 0 ? false : true;
                TreeGridModel treeModel = new TreeGridModel();
                treeModel.id = item.id;
                treeModel.text = item.Name;
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.pid;
                treeModel.expanded = true;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeGridJson());

        }

        public ActionResult GetOrg()
        {
            var list = FlowNode.GetOrg();
            var treeList = new List<TreeGridModel>();

            foreach (var item in list)
            {
                bool hasChildren = list.Count(p => p.pid == item.id) == 0 ? false : true;
                TreeGridModel treeModel = new TreeGridModel();
                treeModel.id = item.id;
                treeModel.text = item.Name;
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.pid;
                treeModel.expanded = true;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeGridJson());

        }
        #endregion

        #region 新版TQ

        /// <summary>
        /// 选择审批人、抄送人
        /// </summary>
        /// <returns></returns>
        public ActionResult FreeChooseFlowUserNew()
        {
            return View();
        }

        public ActionResult GetDeptOrRoleOrUserNew(string OrgType = "")
        {
            if (string.IsNullOrWhiteSpace(OrgType))
            {
                OrgType = "2";
            }
            var list = FlowNode.GetDeptOrRoleOrUserNew(OrgType);
            var treeList = new List<TreeGridModel>();

            foreach (var item in list)
            {
                bool hasChildren = list.Count(p => p.pid == item.id) == 0 ? false : true;
                TreeGridModel treeModel = new TreeGridModel();
                treeModel.id = item.id;
                treeModel.text = item.Name;
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.pid;
                treeModel.expanded = false;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeGridJson());
        }
        /// <summary>
        /// 获取当前节点
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <param name="FlowNodeCode"></param>
        /// <returns></returns>
        public ActionResult GetNodeNew(string FlowCode, string FlowNodeCode)
        {
            var data = FlowNode.GetNodeNew(FlowCode, FlowNodeCode);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取当前节点下审批、抄送人员信息
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <param name="FlowNodeCode"></param>
        /// <returns></returns>
        public ActionResult GetNodePersonnelNew(string FlowCode, string FlowNodeCode, string OrgType)
        {
            var data = FlowNode.GetNodePersonnelNew(FlowCode, FlowNodeCode, OrgType);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="NodeInfo"></param>
        /// <returns></returns>
        public ActionResult AddNodeNew(string NodeInfo)
        {
            var data = FlowNode.AddNodeInfo(NodeInfo);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 修改节点
        /// </summary>
        /// <param name="NodeInfo"></param>
        /// <returns></returns>
        public ActionResult UpdateNodeNew(string NodeInfo)
        {
            var data = FlowNode.UpdateNode(NodeInfo);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <param name="FlowNodeCode"></param>
        /// <returns></returns>
        public ActionResult DeleteNodeNew(string FlowCode, string FlowNodeCode)
        {
            var result = FlowNode.DeleteNode(FlowCode, FlowNodeCode);
            return Content(result.ToJson());
        }

        public ActionResult GetFlowNodeEvent(string FormCode = "") 
        {
            var data = FlowNode.GetFlowNodeEvent(FormCode);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 查询预警条件信息
        /// </summary>
        /// <param name="EarlyWarningCode"></param>
        /// <returns></returns>
        public ActionResult GetNodeEarlyWarningNew(string EarlyWarningCode) 
        {
            var data = FlowNode.GetNodeEarlyWarningNew(EarlyWarningCode);
            return Content(data.ToJson());
        }
        public ActionResult GetNodeEarlyWarningPersonnelNew(string FlowCode, string FlowNodeCode, string EarlyWarningCode, string OrgType) 
        {
            var data = FlowNode.GetNodeEarlyWarningPersonnelNew(FlowCode, FlowNodeCode, EarlyWarningCode, OrgType);
            return Content(data.ToJson());
        }
        public ActionResult AddEarylWarning(string EarylWarningInfo)
        {
            var data = FlowNode.AddEarylWarning(EarylWarningInfo).ToJson();
            return Content(data);
        }
        public ActionResult UpdateEarylWarning(string EarylWarningInfo)
        {
            var data = FlowNode.UpdateEarylWarning(EarylWarningInfo).ToJson();
            return Content(data);
        }
        #endregion
    }
}
