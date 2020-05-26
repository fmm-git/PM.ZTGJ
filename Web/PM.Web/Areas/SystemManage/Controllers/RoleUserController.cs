using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PM.Business;
using PM.DataEntity;

namespace PM.Web.Areas.SystemManage.Controllers
{
    public class RoleUserController : BaseController
    {
        private readonly TbUserRoleLogic _userRole = new TbUserRoleLogic();

        #region 页面视图
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Form(string type)
        {
            ViewBag.type = type;
            return View();
        }
        public ActionResult Details()
        {
            return View();
        }
        public ActionResult UserList()
        {
            return View();
        }

        #endregion

        #region 页面数据操作方法

        /// <summary>
        /// 获取不分页的用户列表数据
        /// </summary>
        /// <param name="code">编码</param>
        /// <param name="type">1：角色 2：岗位</param>
        /// <returns></returns>
        public ActionResult GetUserGridList(UserListRequset request)
        {
            var data = _userRole.GetUserGridList(request);
            return Content(data.ToJson());
        }

        ///// <summary>
        ///// 获取分页列表数据
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public ActionResult GetGridJson(TbUserRoleRequset request, string RoleCode, string keyword)
        //{
        //    var data = new
        //    {
        //        rows = _userRole.GetDataListForPage(request, RoleCode, keyword),
        //        total = request.total,
        //        page = request.page,
        //        records = request.records
        //    };
        //    return Content(data.ToJson());
        //}
        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetGridJson(TbUserRoleRequset request)
        {
            var data =_userRole.GetDataListForPage(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 新增、修改
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(string RoleCode, string strUserCode)
        {
            var strUserCodeNew = strUserCode.TrimEnd(',').Split(',');
            var userList = new List<TbUserRole>();
            for (int i = 0; i < strUserCodeNew.Length; i++)
            {
                TbUserRole userroleEnt = new TbUserRole();
                userroleEnt.UserCode = strUserCodeNew[i];
                userroleEnt.RoleCode = RoleCode;
                userList.Add(userroleEnt);
            }
            var data = _userRole.Insert(userList);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string roleCode, string userCode)
        {
            var data = _userRole.Delete(roleCode, userCode);
            return Content(data.ToJson());

        }

        #endregion

    }
}
