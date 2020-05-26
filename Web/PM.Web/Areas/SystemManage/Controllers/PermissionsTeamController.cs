using Newtonsoft.Json;
using PM.Business;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.SystemManage.Controllers
{
    /// <summary>
    /// 团队管理
    /// </summary>
    [HandlerLogin]
    public class PermissionsTeamController : BaseController
    {
        TbPermissionsTeamLogic pti = new TbPermissionsTeamLogic();

        #region 视图
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Form()
        {
            return View();
        }
        public ActionResult Detail()
        {
            return View();
        }
        #endregion

        #region 数据操作
        public ActionResult Add(string param)
        {
            if (string.IsNullOrEmpty(param))
                Error("参数错误");
            try
            {
                var model = PM.Common.Extension.JsonEx.JsonToObj<PermissionsTeamRequest>(param);
                var result = pti.Add(model);
                return Content(result.ToJson());
            }
            catch (Exception)
            {
                throw;
            }
        }
        public ActionResult UpdateTeam(string param)
        {
            if (string.IsNullOrEmpty(param))
                Error("参数错误");
            try
            {
                var model = PM.Common.Extension.JsonEx.JsonToObj<PermissionsTeamRequest>(param);
                var result = pti.UpdateTeam(model);
                return Content(result.ToJson());
            }
            catch (Exception)
            {
                throw;
            }
        }
        public string GetList(string Key, string MenuCode)
        {
            if (string.IsNullOrEmpty(Key)) Key = "";
            DataTable table = pti.GetList(Key, MenuCode);
            return JsonConvert.SerializeObject(table);
        }
        public string GetMenuList()
        {
            var list = pti.GetMenuList();
            var treeList = new List<TreeGridModel>();
            foreach (TbSysMenu item in list)
            {
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = list.Count(t => t.MenuPCode == item.MenuCode) == 0 ? false : true;
                treeModel.id = item.MenuCode;
                treeModel.text = item.MenuName;
                if (list.Count(t => t.MenuCode == item.MenuPCode) == 0)
                {
                    item.MenuPCode = "0";
                }
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.MenuPCode;
                treeModel.expanded = hasChildren;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return treeList.TreeGridJson();
        }
        public ActionResult DeleteTeam(string Key)
        {
            var result = pti.DeleteTeam(Key);
            return Content(result.ToJson());
        }
        public ActionResult GetMenuSelectList()
        {
            var list = pti.GetMenuList();
            var treeList = new List<TreeSelectModel>();
            foreach (TbSysMenu item in list)
            {
                TreeSelectModel treeModel = new TreeSelectModel();
                bool hasChildren = list.Count(t => t.MenuPCode == item.MenuCode) == 0 ? false : true;
                treeModel.id = item.MenuCode;
                treeModel.text = item.MenuName;
                if (list.Count(t => t.MenuCode == item.MenuPCode) == 0)
                {
                    item.MenuPCode = "0";
                }
                treeModel.id = item.MenuCode;
                treeModel.text = item.MenuName;
                treeModel.parentId = item.MenuPCode;
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeSelectJson());
        }
        public ActionResult GetTeamJson(string key)
        {
            var data = pti.GetModel(key);
            return Content(data.ToJson());
        }
        #endregion
    }
}
