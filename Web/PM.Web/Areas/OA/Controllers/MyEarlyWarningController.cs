using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PM.Business.Flow;
using PM.Common;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.OA.Controllers
{
    public class MyEarlyWarningController : Controller
    {
        //
        //预警信息
        private readonly TbFlowEarlyWarningConditionLogic _earlyWarning = new TbFlowEarlyWarningConditionLogic();
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 获取流程预警信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetEarlyWarningInfo(PageSearchRequest pt, string param) 
        {
            DateTime? SDT = null;
            DateTime? EDT = null;
            int state = 0;
            string UserId = OperatorProvider.Provider.CurrentUser.UserId;
            string ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            if (string.IsNullOrEmpty(param))
            {
                state = 0;
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
            var data = _earlyWarning.GetEarlyWarningInfo(pt, UserId, ProjectId, state, SDT, EDT);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取表单预警
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public ActionResult GetEarlyWarningFormInfo(PageSearchRequest pt, string param) 
        {
            DateTime? SDT = null;
            DateTime? EDT = null;
            int state = 0;
            string UserId = OperatorProvider.Provider.CurrentUser.UserId;
            string ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            if (string.IsNullOrEmpty(param))
            {
                state = 0;
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
            var data = _earlyWarning.GetEarlyWarningFormInfo(pt, UserId,ProjectId, state, SDT, EDT);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 处理流程预警信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult HandleEarlyWarning(int ID, string EarlyType)
        {
            var data = _earlyWarning.HandleEarlyWarning(ID, EarlyType);
            return Content(data.ToJson());
        }
        private class MyEarlyWarningInfo
        {
            public int state { get; set; }
            public DateTime sdt { get; set; }
            public DateTime edt { get; set; }
        }
        /// <summary>
        /// 获取预警状态
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMyEarlyWarningState(string param)
        {
            DateTime? SDT = null;
            DateTime? EDT = null;
            string UserId = OperatorProvider.Provider.CurrentUser.UserId;
            string ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
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
            var data = _earlyWarning.GetMyEarlyWarningState(UserId, ProjectId, SDT, EDT);
            return Content(data.ToJson());
        }
    }
}
