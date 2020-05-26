using PM.Business;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.SystemManage.Controllers
{
    /// <summary>
    /// 单条数据授权管理
    /// </summary>
    [HandlerLogin]
    public class AuthorizeUserController :BaseController
    {
        private readonly AuthorizeUserLogic _authorizeUser = new AuthorizeUserLogic();

        #region 编辑

        /// <summary>
        /// 新增、修改页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Form()
        {
            ViewBag.SQR = base.UserCode;
            return View();
        }

        /// <summary>
        /// 编辑/查看页获取数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetFormJson(string menuCode, int dataID)
        {
            var data = _authorizeUser.FindEntity(menuCode, dataID);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitForm(string param, string type)
        {
            if (string.IsNullOrEmpty(param))
                Error("参数错误");
            var request = PM.Common.Extension.JsonEx.JsonToObj<AuthorizeUserRequest>(param);
            if (type == "add")
            {
                var data = _authorizeUser.Insert(request);
                return Content(data.ToJson());
            }
            else
            {
                var data = _authorizeUser.Update(request);
                return Content(data.ToJson());
            }
        }

        #endregion

        #region 删除
        
        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteForm(string menuCode, int dataID)
        {
            var data = _authorizeUser.Delete(menuCode, dataID);
            return Content(data.ToJson());
        }
        #endregion
    }
}
