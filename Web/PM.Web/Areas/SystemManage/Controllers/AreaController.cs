using PM.Business;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.SystemManage.Controllers
{
    public class AreaController : Controller
    {
        private readonly TbAreaAdministrationLogic _area = new TbAreaAdministrationLogic();

        #region 列表

        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 获取树形列表数据
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetTreeGridJson(TbAreaAdministrationReauset request, string keyword)
        {
            if (string.IsNullOrEmpty(request.code))
                request.code = "";
            else
                keyword = "";
            var data = _area.GetListTreeBySearch(request, keyword);
            if (string.IsNullOrWhiteSpace(keyword))
            {
                var treeList = new List<TreeGridAsynModel<TbAreaAdministrationTreeReauset>>();
                foreach (var item in data)
                {
                    TreeGridAsynModel<TbAreaAdministrationTreeReauset> treeModel = new TreeGridAsynModel<TbAreaAdministrationTreeReauset>();
                    treeModel.isLeaf = item.ChildCount == 0;
                    treeModel.parentId = item.FK_AreaCode;
                    treeModel.nodeId = item.AreaCode;
                    treeModel.n_level = request.code == "" ? 0 : request.n_level + 1;
                    treeModel.entityJson = item;
                    treeList.Add(treeModel);
                }
                return Content(treeList.TreeGridAsynJson());
            }
            else
            {
                var treeList = new List<TreeGridModel>();
                foreach (var item in data)
                {
                    TreeGridModel treeModel = new TreeGridModel();
                    bool hasChildren = data.Count(t => t.FK_AreaCode == item.AreaCode) == 0 ? false : true;
                    if (data.Count(t => t.AreaCode == item.FK_AreaCode) == 0)
                    {
                        item.FK_AreaCode = "0";
                    }
                    treeModel.id = item.AreaCode;
                    treeModel.isLeaf = hasChildren;
                    treeModel.parentId = item.FK_AreaCode;
                    treeModel.expanded = true;
                    treeModel.entityJson = item.ToJson();
                    treeList.Add(treeModel);
                }
                return Content(treeList.TreeGridJson());
            }
        }

        public ActionResult GetParentJson(string AreaCode)
        {
            var data = _area.GetParentAreaJosn(AreaCode);
            var treeList = new List<TreeGridModel>();
            foreach (TbAreaAdministration item in data)
            {
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = data.Count(t => t.FK_AreaCode == item.AreaCode) == 0 ? false : true;
                treeModel.id = item.AreaCode;
                treeModel.text = item.AreaName;
                if (data.Count(t => t.AreaCode == item.FK_AreaCode) == 0)
                {
                    item.FK_AreaCode = "0";
                }
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.FK_AreaCode;
                treeModel.expanded = false;//是否展开子节点
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeGridJson());
        }

        public ActionResult GetAllAreaProvinceList(string AreaCode)
        {
            var data = _area.GetAllParentAreaJosn(AreaCode);
            return Content(data.ToJson());
        }

        #endregion
    }
}
