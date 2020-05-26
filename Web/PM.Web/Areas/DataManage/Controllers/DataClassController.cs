using PM.Business.DataManage;
using PM.DataEntity;
using PM.DataEntity.DataManage.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.DataManage.Controllers
{
    [HandlerLogin]
    public class DataClassController : BaseController
    {
        private readonly FileTypeBuessic _File = new FileTypeBuessic();
        //
        // GET: /DataManage/DataClass/
        //资料分类统计
        public ActionResult Index()
        {
            return View();
        }

         /// <summary>
        /// 获取菜单-模块/表单二级树形列表数据
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetTreeGridJson()
        {
            var data = _File.GetTwoNveProInfo().ToList();
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

        //资料分类统计数据查询
        public ActionResult GetDataList(TbDataClassRequest request)
        {
            var data = new
            {
                rows = _File.GetDataList(request).ToList(),
                total = request.total,
                page = request.page,
                records = request.records
            };
            return Content(data.ToJson());
        }
    }
}
