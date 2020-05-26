using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PM.Business;
using PM.Common;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.OA.Controllers
{
    public class MyApprovalController : BaseController
    {
        private readonly FlowPerformLogic _flowperform = new FlowPerformLogic();

        #region 视图
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Approval()
        {
            return View();
        }

        public ActionResult SeeApproval()
        {
            return View();
        }
        #endregion

        #region 查询
        /// <summary>
        /// 查看审批消息列表
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="para"></param>
        /// <returns></returns>
        public string GetMyApproval(PageSearchRequest pt, string param)
        {
            //int state = -1;
            //DateTime? SDT, EDT;
            //if (string.IsNullOrEmpty(para))
            //{
            //    state = -1;
            //    SDT = null;
            //    EDT = null;
            //}
            //else
            //{
            //    JObject jo = (JObject)JsonConvert.DeserializeObject(para);
            //    state = Convert.ToInt32(jo["state"]);
            //    SDT = null;
            //    EDT = null;
            //    if (jo["SDT"].ToString() != "{}" && jo["SDT"].ToString() != "")
            //    {
            //        SDT = Convert.ToDateTime(jo["SDT"]);
            //    }
            //    if (jo["EDT"].ToString() != "{}" && jo["EDT"].ToString() != "")
            //    {
            //        EDT = Convert.ToDateTime(jo["EDT"]).AddDays(1);
            //    }

            //}
            //var data = _flowperform.GetMyApproval(pt, Convert.ToString(Session["userid"]), state, SDT, EDT);
            //return Content(data.ToJson());

            int state = -1;
            DateTime? SDT = null;
            DateTime? EDT = null;
            if (string.IsNullOrEmpty(param))
            {
                state = -1;
                SDT = null;
                EDT = null;
            }
            else
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(param);
                state = Convert.ToInt32(jo["state"]);
                if (jo["SDT"].ToString() != "{}" && jo["SDT"].ToString() != "")
                {
                    SDT = Convert.ToDateTime(jo["SDT"]);
                }
                if (jo["EDT"].ToString() != "{}" && jo["EDT"].ToString() != "")
                {
                    EDT = Convert.ToDateTime(jo["EDT"]).AddDays(1);
                }
            }
            string  userId = OperatorProvider.Provider.CurrentUser.UserId;
            return _flowperform.GetMyApproval(pt, userId, state, SDT, EDT);
        }
        /// <summary>
        /// 获取审批状态
        /// </summary>
        /// <returns></returns>
        public string GetMyApprovalState(string param)
        {

            //if (SDT!=null)
            //{
            //    SDT = Convert.ToDateTime(SDT);
            //}
            //if (EDT!=null)
            //{
            //    EDT = Convert.ToDateTime(EDT).AddDays(1);
            //}
            DateTime? SDT = null;
            DateTime? EDT = null;
            if (string.IsNullOrEmpty(param))
            {
                SDT = null;
                EDT = null;
            }
            else
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(param);
                if (jo["SDT"].ToString() != "{}" && jo["SDT"].ToString() != "")
                {
                    SDT = Convert.ToDateTime(jo["SDT"]);
                }
                if (jo["EDT"].ToString() != "{}" && jo["EDT"].ToString() != "")
                {
                    EDT = Convert.ToDateTime(jo["EDT"]).AddDays(1);
                }
            }
            string userId = OperatorProvider.Provider.CurrentUser.UserId;
            return _flowperform.GetMyApprovalState(userId, SDT, EDT);
        }
        /// <summary>
        /// 加载原表单
        /// </summary>
        /// <param name="formCode"></param>
        /// <returns></returns>
        public string LoadOrderForm(string formCode)
        {
            return _flowperform.LoadOrderForm(formCode);
        }
        /// <summary>
        /// 加载审批意见
        /// </summary>
        /// <param name="FlowPerformID"></param>
        /// <returns></returns>
        public string LoadApprovalOptions(string FlowPerformID)
        {
            return _flowperform.LoadApprovalOptions(FlowPerformID);
        }

        #region 获取原材料到货入库信息是否是审核完成，如果是审核完成调用短信接口，发送短信信息

        /// <summary>
        /// 获取原材料到货入库信息是否是审核完成，如果是审核完成调用短信接口，发送短信信息
        /// </summary>
        /// <param name="formCode"></param>
        /// <returns></returns>
        public string LoadInOrder(int ID)
        {
            var data = _flowperform.LoadInOrder(ID);
            return data;
        }

        #endregion

        #endregion

        #region 审批

        [HttpPost]
        public ActionResult Approval(string para)
        {
            RequestPara model = PM.Common.Extension.JsonEx.JsonToObj<RequestPara>(para);
            var data = _flowperform.Approval(model.FlowCode, model.performState, model.performOpinions, model.FlowPerformID, model.flowNodeCode, Convert.ToString(Session["userid"]), model.FlowTitle, model.FreeNodeUser);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 判断订单流程是否在加工厂接收阶段
        /// </summary>
        /// <param name="FlowCode"></param>
        /// <returns></returns>
        public ActionResult GetFlowWorkNode(string para)
        {
            RequestPara model = PM.Common.Extension.JsonEx.JsonToObj<RequestPara>(para);
            var data = _flowperform.GetFlowWorkNode(model.FlowCode, model.performState, model.performOpinions, model.FlowPerformID, model.flowNodeCode, Convert.ToString(Session["userid"]), model.FlowTitle, model.FreeNodeUser);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 修改审批状态（抄送人）
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public ActionResult UpdatePerformState(string FlowPerformID, string FlowCode, string FlowNodeCode, int performState, string UserType)
        {
            var data = _flowperform.UpdatePerformState(FlowPerformID, FlowCode, FlowNodeCode, performState, UserType, Convert.ToString(Session["userid"]));
            return Content(data.ToJson());
        }
        /// <summary>
        /// 修改审批状态（执行人）
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public ActionResult UpdatePerformStateZxr(string FlowPerformID, string FlowCode, string FlowNodeCode, int performState, string UserType)
        {
            var data = _flowperform.UpdatePerformStateZxr(FlowPerformID, FlowCode, FlowNodeCode, performState, UserType, Convert.ToString(Session["userid"]));
            return Content(data.ToJson());
        }

        #endregion

        #region 查询数据流程信息

        public ActionResult GetDataExaminestatus(string FormCode = "", int DataId = 0)
        {
            var Data = _flowperform.GetDataExaminestatus(FormCode, DataId);
            return Content(Data.ToJson());
        }
        public ActionResult GetFlowDefine(string FormCode = "", string ProjectId = "", string OtherParma = "")
        {
            var Data = _flowperform.GetFlowDefine(FormCode, ProjectId, OtherParma);
            return Content(Data.ToJson());
        }
        /// <summary>
        /// 发起流程
        /// </summary>
        /// <returns></returns>
        public ActionResult InitFlowPerformNew(string param)
        {
            var json = PM.Common.Extension.JsonEx.JsonToObj<InitFlowPerformModel>(param);
            var Data = _flowperform.InitFlowPerformNew(json.FlowCode, json.FormCode, json.FormDataCode, json.UserCode, json.FlowTitle, Convert.ToInt32(json.FlowLevel));
            return Content(Data.ToJson());
        }

        public ActionResult ApprovalNew(string para)
        {
            RequestPara model = PM.Common.Extension.JsonEx.JsonToObj<RequestPara>(para);
            var data = _flowperform.ApprovalNew(model.FlowCode, model.performState, model.performOpinions, model.FlowPerformID, model.flowNodeCode, Convert.ToString(Session["userid"]), model.FlowTitle);
            return Content(data.ToJson());
        }

        #endregion
    }

    #region 接收参数
    public class RequestPara
    {
        public string FlowCode { get; set; }
        public int performState { get; set; }
        public string performOpinions { get; set; }
        public string FlowPerformID { get; set; }
        public string flowNodeCode { get; set; }
        public string FlowTitle { get; set; }
        public string FreeNodeUser { get; set; }
        public string UserCode { get; set; }
    }
    #endregion
}
