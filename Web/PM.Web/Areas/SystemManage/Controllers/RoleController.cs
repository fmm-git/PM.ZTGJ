using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using System.Data;
using Newtonsoft.Json;
using PM.Business;
using PM.DataEntity;

namespace PM.Web.Areas.SystemManage.Controllers
{
    public class RoleController : BaseController
    {
        private readonly TbSysMenuLogic _sysMenu = new TbSysMenuLogic();
        private readonly TbRoleLogic _role = new TbRoleLogic();
        private readonly TbRoleMenuLogic _roleMenu = new TbRoleMenuLogic();

        #region 角色视图
        /// <summary>
        /// 列表页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 新增、修改页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Form(string type)
        {
            if (type == "add" || type == "copy")
            {
                ViewBag.RoleCode = _role.FindEntityNumber();
            }
            return View();
        }
        /// <summary>
        /// 查看页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Details()
        {
            return View();
        }

        public ActionResult RoleAuthority()
        {
            return View();
        }

        #endregion

        #region 页面数据操作方法

        /// <summary>
        /// 获取不分页的列表数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetNoPageGridJson()
        {
            var data = _role.GetNoPageGridList();
            return Content(data.ToJson());
        }

        ///// <summary>
        ///// 获取分页列表数据
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public ActionResult GetGridJson(TbRoleRequset request, string keyword)
        //{
        //    var data = new
        //    {
        //        rows = _role.GetDataListForPage(request, keyword),
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
        public ActionResult GetGridJson(TbRoleRequset request)
        {
            var data = _role.GetDataListForPage(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取单条数据
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = _role.FindEntity(keyValue);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 加载菜单、菜单按钮tree
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetPermissionTree(string roleId)
        {
            var moduledata = _sysMenu.GetList("");
            var treeList = new List<TreeViewModel>();
            foreach (TbSysMenu item in moduledata)
            {
                TreeViewModel tree = new TreeViewModel();
                bool hasChildren = moduledata.Count(t => t.MenuPCode == item.MenuCode) == 0 ? false : true;
                tree.id = item.MenuCode;
                tree.text = item.MenuName;
                tree.value = item.MenuCode;
                tree.parentId = item.MenuPCode;
                tree.isexpand = true;
                tree.complete = true;
                tree.showcheck = true;
                tree.checkstate = 0;
                tree.hasChildren = true;
                tree.img = "";
                treeList.Add(tree);
            }
            return Content(treeList.TreeViewJson());
        }
        /// <summary>
        /// 新增、修改
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(TbRoleRequset request, string type, string roleCodeOld)
        {
            if (type == "add")
            {
                var data = _role.Insert(request);
                return Content(data.ToJson());
            }
            else if (type == "copy")
            {
                var data = _role.CopyRole(request, roleCodeOld);
                return Content(data.ToJson());
            }
            else
            {
                var data = _role.Update(request);
                return Content(data.ToJson());
            }
        }

        /// <summary>
        /// 修改角色状态
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public ActionResult EditIsStart(string code, string val)
        {
            var data = _role.EditIsStart(code, val);
            return Content(data.ToJson()); ;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            var data = _role.Delete(keyValue);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 保存角色菜单
        /// </summary>
        /// <param name="postData"></param>
        /// <param name="RoleCode"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult submitForm1(string postData, string RoleCode)
        {
            int IndexofA = postData.IndexOf("[");
            int IndexofB = postData.IndexOf("]");
            string Ru = postData.Substring(IndexofA, IndexofB - IndexofA + 1);
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            List<TbRoleMenu> objs = Serializer.Deserialize<List<TbRoleMenu>>(Ru);
            var data = _roleMenu.Insert(objs, RoleCode);
            return Content(data.ToJson());
        }
        [HttpGet]
        public ActionResult GetGridRoleMenuJson(string RoleCode)
        {
            var data = _roleMenu.GetList(RoleCode);
            return Content(data.ToJson());
        }
        #endregion
    }
}
