using PM.Common;
using PM.Common.Helper;
using System;
using System.Web;
using System.Web.Mvc;

namespace PM.Web
{
    /// <summary>
    /// 验证是否登录
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class HandlerLoginAttribute : AuthorizeAttribute
    {
        public bool Ignore { get; set; }//是否需要验证登录
        private bool _hasLogin = false;//是否登录
        public HandlerLoginAttribute()
        {
            Ignore = true;
        }
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            try
            {
                if (Ignore == false)
                {
                    _hasLogin = true;
                    return;
                }
                if (OperatorProvider.Provider.GetCurrent() != null)
                {
                    _hasLogin = true;
                }
                else { _hasLogin = false; }
                if (!_hasLogin) return;
            }
            finally
            {
                base.OnAuthorization(filterContext);
            }
        }

        /// <summary>
        /// 自定义授权检查
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
                return false;

            return _hasLogin;
        }

        /// <summary>
        /// 处理授权失败的处理
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);
            WebHelper.WriteCookie("nfine_login_error", "overdue");
            var req = filterContext.HttpContext.Request;
            if (req.IsAjaxRequest())
            {
               var ret= new AjaxResult { state = ResultType.error.ToString(), message = "登录超时，请重新登录" };
               filterContext.Result = new JsonResult() { Data = ret};
            }
            else
            {
                filterContext.HttpContext.Response.Redirect("/Login/Index");
            }
        }
    }
}