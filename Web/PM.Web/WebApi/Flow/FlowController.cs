using Dos.ORM;
using Newtonsoft.Json.Linq;
using PM.Business;
using PM.Common;
using PM.Common.Extension;
using PM.DataEntity;
using PM.Web.Areas.OA.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PM.Web.WebApi.Flow
{
    public class FlowController : ApiController
    {
        private readonly FlowPerformLogic _flowlogic = new FlowPerformLogic();
        private string HostAddress = Configs.GetValue("HostAddress");
        #region 审批信息列表
        /// <summary>
        /// 审批信息列表
        /// </summary>
        /// <param name="UserCode">登录人编号</param>
        /// <param name="PerformState">审批状态</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetFlowList(string UserCode, int PerformState)
        {
            try
            {
                var data = _flowlogic.GetFlowList(UserCode, PerformState);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 加载审批原表单地址

        [HttpGet]
        public HttpResponseMessage LoadFormUrl(string formCode, int keyValue)
        {
            try
            {
                var data = _flowlogic.LoadFormUrl(HostAddress, formCode, keyValue);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 加载审批意见

        [HttpGet]
        public HttpResponseMessage LoadFormSpOptions(string flowperformid)
        {
            try
            {
                var data = _flowlogic.LoadFormSpOptions(flowperformid);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 审批同意/退回

        [HttpPost]
        public HttpResponseMessage Approval([FromBody]JObject jdata)
        {
           // RequestPara model = PM.Common.Extension.JsonEx.JsonToObj<RequestPara>(para);
            string modelstr = jdata["para"] == null ? "" : jdata["para"].ToString();
            RequestPara model = JsonEx.JsonToObj<RequestPara>(modelstr);
            //string UserId=
            var ret = PM.DataAccess.DbContext.Db.Context.From<TbUser>()
            .Select(TbUser._.UserId).Where(p => p.UserCode == model.UserCode).First();
            var data = _flowlogic.Approval(model.FlowCode, model.performState, model.performOpinions, model.FlowPerformID, model.flowNodeCode, ret.UserId, model.FlowTitle, model.FreeNodeUser);
            return data.ToJsonApi();
        }

        #endregion
    }
}
