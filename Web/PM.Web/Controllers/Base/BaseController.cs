using PM.Common;
using PM.Common.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace PM.Web
{
    /// <summary>
    /// 获取登录用户信息
    /// </summary>
    /// <returns></returns>
    public class BaseController : Controller
    {
        #region JsonMsg
        public JsonResult JsonMsg(bool isSuccess, string msg)
        {
            return Json(new { IsSuccess = isSuccess, Msg = msg }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult JsonMsg(bool isSuccess, object data, string msg="")
        {
            return Json(new { IsSuccess = isSuccess, Data = data, Msg = msg }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 操作成功失败提示信息

        protected virtual ActionResult Success(string message)
        {
            return Content(new AjaxResult { state = ResultType.success.ToString(), message = message }.ToJson());
        }
        protected virtual ActionResult Success(string message, object data)
        {
            return Content(new AjaxResult { state = ResultType.success.ToString(), message = message, data = data }.ToJson());
        }
        protected virtual ActionResult Error(string message)
        {
            return Content(new AjaxResult { state = ResultType.error.ToString(), message = message }.ToJson());
        }

        #endregion

        /// <summary>
        /// 登录用户信息
        /// </summary>
        public CurrentUserInfo CurrentUser = OperatorProvider.Provider.CurrentUser;
        public string UserCode = OperatorProvider.Provider.CurrentUser == null ? "" : OperatorProvider.Provider.CurrentUser.UserCode;
        public string UserName = OperatorProvider.Provider.CurrentUser == null ? "" : OperatorProvider.Provider.CurrentUser.UserName;
        public bool IsSystem = OperatorProvider.Provider.CurrentUser == null ? false : OperatorProvider.Provider.CurrentUser.IsSystem;

    }
}