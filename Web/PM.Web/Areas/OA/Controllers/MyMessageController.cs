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
    public class MyMessageController : Controller
    {
        FlowPerformLogic _flowperform = new FlowPerformLogic();

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult MessageView()
        {
            return View();
        }
        public ActionResult GetMyMessage(PageSearchRequest pt, string param)
        {
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
                state = Convert.ToInt32(jo["status"]);
                if (jo["SDT"].ToString() != "{}" && jo["SDT"].ToString() != "")
                {
                    SDT = Convert.ToDateTime(jo["SDT"]);
                }
                if (jo["EDT"].ToString() != "{}" && jo["EDT"].ToString() != "")
                {
                    EDT = Convert.ToDateTime(jo["EDT"]).AddDays(1);
                }
            }
            string usercode = Convert.ToString(Session["userid"]);
            var data= _flowperform.GetMyMessage(pt,usercode,state,SDT,EDT);
            return Content(data.ToJson());
        }
        public string GetMessageState(string param)
        {
            string usercode = Convert.ToString(Session["userid"]);
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
            return _flowperform.GetMessageState(usercode, SDT, EDT);
        }
        public string GetCount()
        {

            string UserId = OperatorProvider.Provider.CurrentUser.UserId;
            string ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            return _flowperform.GetCount(UserId, ProjectId);
        }
        //public ActionResult UpdateStatus(string ID)
        //{
        //    var data = _flowperform.UpdateStatus(Convert.ToInt32(ID));
        //    return Content(data.ToJson());
        //}


        #region 新版我的消息
        /// <summary>
        /// 获取我的消息中的消息状态
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public ActionResult GetMessageStateNew(string param) 
        {
            DateTime? SDT = null;
            DateTime? EDT = null;
            string UserId = OperatorProvider.Provider.CurrentUser.UserId;
            string ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            if (!string.IsNullOrEmpty(param))
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
            var data = _flowperform.GetMessageStateNew(UserId, ProjectId, SDT, EDT);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取我的消息中的消息状态的列表数据
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public ActionResult GetMessageStateList(PageSearchRequest pt,string param) 
        {
            int state = 0;
            DateTime? SDT = null;
            DateTime? EDT = null;
            string UserId = OperatorProvider.Provider.CurrentUser.UserId;
            string ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            if (!string.IsNullOrEmpty(param))
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(param);
                state = Convert.ToInt32(jo["status"]);
                if (jo["SDT"].ToString() != "{}" && jo["SDT"].ToString() != "")
                {
                    SDT = Convert.ToDateTime(jo["SDT"]);
                }
                if (jo["EDT"].ToString() != "{}" && jo["EDT"].ToString() != "")
                {
                    EDT = Convert.ToDateTime(jo["EDT"]).AddDays(1);
                }
            }
            var data = _flowperform.GetMessageStateList(pt, UserId, ProjectId, state, SDT, EDT);
            return Content(data.ToJson());
        }

        public ActionResult UpdateStatus(string ID)
        {
            var data = _flowperform.UpdateStatusNew(Convert.ToInt32(ID));
            return Content(data.ToJson());
        }

        #endregion
    }
}
