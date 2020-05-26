using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using PM.Common;
using PM.Business;
using PM.DataEntity;

namespace PM.Web.Areas.SystemManage.Controllers
{

    [HandlerLogin]
    public class MenuController : BaseController
    {
        private readonly TbSysMenuLogic _sysMenu = new TbSysMenuLogic();
        private readonly TbSysMenuTableLogic _sysMenuTable = new TbSysMenuTableLogic();

        #region 加载菜单导航栏

        [HttpGet]
        public ActionResult GetClientsDataJson(TbSysMenu request)
        {
            var data = new
            {
                dataItems = "",
                organize = "",
                role = "",
                duty = "",
                user = base.UserCode,
                projectId = base.CurrentUser.ProjectId,
                orgType = base.CurrentUser.OrgType,
                companyId = base.CurrentUser.CompanyId,
                authorizeMenu = GetMenuList(),
                authorizeButton = "",
            };
            return Content(data.ToJson());
        }
        private object GetMenuList()
        {
            if (base.IsSystem)
            {
                return ToMenuJson(_sysMenu.GetNavMenuList().ToList(), "0");
            }
            else
            {
                var list = new List<TbSysMenu>();
                if (base.CurrentUser != null)
                {
                    //判断人员角色菜单
                    if (!string.IsNullOrEmpty(base.CurrentUser.RoleCode))
                    {
                        var roleMenuList = _sysMenu.GetCurrentMenuListByRoleCodePC(base.CurrentUser.RoleCode);
                        if (roleMenuList.Count > 0)
                            list.AddRange(roleMenuList);
                    }
                    //判断人员菜单
                    if (!string.IsNullOrEmpty(base.CurrentUser.UserId))
                    {
                        var userMenuList = _sysMenu.GetCurrentMenuListByUserCodePC(base.CurrentUser.UserId);
                        if (userMenuList.Count > 0)
                            list.AddRange(userMenuList);
                    }
                    //去重
                    list = list.Where((x, firstId) => list.FindIndex(z => z.MenuCode == x.MenuCode) == firstId).OrderBy(x=>x.Sort).ToList();
                    //list.Distinct();
                }
                return ToMenuJson(list, "0");
            }
        }
        private string ToMenuJson(List<TbSysMenu> data, string parentId)
        {
            StringBuilder sbJson = new StringBuilder();
            sbJson.Append("[");
            List<TbSysMenu> entitys = data.FindAll(t => t.MenuPCode == parentId);
            if (entitys.Count > 0)
            {
                foreach (var item in entitys)
                {
                    string strJson = item.ToJson();
                    strJson = strJson.Insert(strJson.Length - 1, ",\"ChildNodes\":" + ToMenuJson(data, item.MenuCode) + "");
                    sbJson.Append(strJson + ",");
                }
                sbJson = sbJson.Remove(sbJson.Length - 1, 1);
            }
            sbJson.Append("]");
            return sbJson.ToString();
        }

        #endregion

        #region 视图方法

        #region 菜单视图
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
            ViewBag.type = type;
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

        #endregion

        #region 菜单指定表单视图
        public ActionResult AppointMenuTableIndex()
        {
            return View();
        }

        public ActionResult AppointMenuTableForm(string type)
        {
            ViewBag.type = type;
            return View();
        }
        public ActionResult AppointMenuTableDetails()
        {
            return View();
        }
        #endregion

        #endregion

        #region 页面数据操作方法

        #region 菜单页面的数据操作方法
        /// <summary>
        /// 页面初始化数据
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = _sysMenu.FindEntity(keyValue);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取树形列表数据
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetTreeGridJson(string keyword)
        {
            var data = _sysMenu.GetList(keyword).ToList();
            var treeList = new List<TreeGridModel>();
            foreach (TbSysMenu item in data)
            {
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = data.Count(t => t.MenuPCode == item.MenuCode) == 0 ? false : true;
                treeModel.id = item.MenuCode;
                treeModel.text = item.MenuName;
                if (data.Count(t => t.MenuCode == item.MenuPCode) == 0)
                {
                    item.MenuPCode = "0";
                } 
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.MenuPCode;
                treeModel.expanded = hasChildren;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeGridJson());
        }

        /// <summary>
        /// 获取树形列表数据
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetAuthorityTreeGridJson(string keyword)
        {
            var data = _sysMenu.GetAuthorityMenuList(keyword).ToList();
            var treeList = new List<TreeGridModel>();
            foreach (TbSysMenu item in data)
            {
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = data.Count(t => t.MenuPCode == item.MenuCode) == 0 ? false : true;
                treeModel.id = item.MenuCode;
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.MenuPCode;
                treeModel.expanded = hasChildren;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeGridJson());
        }

        /// <summary>
        /// 获取tree数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetTreeSelectJson(string keyword)
        {
            var data = _sysMenu.GetList(keyword).ToList();
            var treeList = new List<TreeSelectModel>();
            foreach (TbSysMenu item in data)
            {
                TreeSelectModel treeModel = new TreeSelectModel();
                treeModel.id = item.MenuCode;
                treeModel.text = item.MenuName;
                treeModel.parentId = item.MenuPCode;
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeSelectJson());
        }
        /// <summary>
        /// 新增、修改
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(TbSysMenuRequset request, string type)
        {
            if (type == "add")
            {
                var data = _sysMenu.Insert(request);
                return Content(data.ToJson());
            }
            else
            {
                var data = _sysMenu.Update(request);
                return Content(data.ToJson());
            }
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
            var data = _sysMenu.Delete(keyValue);
            return Content(data.ToJson());
        }

        #endregion

        #region 指定菜单表单页面数据操作方法

        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetAppointMenuTableGridJson(TbSysMenuTableRequset request, string keyword)
        {
            var data = new
            {
                rows = _sysMenuTable.GetDataListForPage(request, keyword),
                total = request.total,
                page = request.page,
                records = request.records
            };
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取单条数据
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetAppointMenuTableFormJson(int keyValue)
        {
            var data = _sysMenuTable.FindEntity(keyValue);
            return Content(data.ToJson());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitAppointMenuTableForm(TbSysMenuTableRequset request, string type)
        {
            if (type == "add")
            {
                var data = _sysMenuTable.Insert(request);
                return Content(data.ToJson());
            }
            else
            {
                var data = _sysMenuTable.Update(request);
                return Content(data.ToJson());
            }
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAppointMenuTableForm(int keyValue)
        {
            var data = _sysMenuTable.Delete(keyValue);
            return Content(data.ToJson());
        }
        #endregion

        #endregion

    }
}
