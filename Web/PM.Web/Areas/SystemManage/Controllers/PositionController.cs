using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PM.Common.Helper;
using System.Web.Script.Serialization;
using PM.Business;
using PM.DataEntity;

namespace PM.Web.Areas.SystemManage.Controllers
{
    /// <summary>
    /// 岗位管理
    /// </summary>
    [HandlerLogin]
    public class PositionController : BaseController
    {
        private readonly PositionLogic _position = new PositionLogic();
        private readonly TbPositionMenuLogic _positionMenu = new TbPositionMenuLogic();

        #region 列表

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetGridJson(PositionSearchRequest request)
        {
            var data = _position.GetListBySearch(request);
            var treeList = new List<TreeGridModel>();
            foreach (var item in data)
            {
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = data.Count(t => t.ParentPositionCode == item.PositionCode) == 0 ? false : true;
                if (data.Count(t => t.PositionCode == item.ParentPositionCode) == 0)
                {
                    item.ParentPositionCode = "0";
                }
                treeModel.id = item.PositionCode;
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.ParentPositionCode;
                treeModel.expanded = hasChildren;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }

            return Content(treeList.TreeGridJson());
        }

        /// <summary>
        /// 获取组织机构数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetTreeJsonForList()
        {
            var treeList = new List<TreeGridModel>();
            //获取组织机构信息
            var department = _position.GetPositionTree();
            foreach (var item in department)
            {
                bool hasChildren = department.Count(p => p.ParentCode == item.Code) == 0 ? false : true;
                TreeGridModel treeModel = new TreeGridModel();
                treeModel.id = item.Code;
                treeModel.text = item.Name;
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.ParentCode;
                treeModel.expanded = true;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeGridJson());
        }

        #endregion

        #region 编辑

        /// <summary>
        /// 新增、修改页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Form(string type)
        {
            if (type == "add")
            {
                ViewBag.PositionNum = _position.GetPositionNum();
            }
            return View();
        }

        /// <summary>
        /// 查看页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Detail()
        {
            return View();
        }

        /// <summary>
        /// 编辑/查看页获取数据
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult GetFormJson(string keyValue)
        {
            var data = _position.FindEntity(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取上级岗位数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetTreeSelectJson(string deCode, string pDeCode, string code)
        {
            PositionSearchRequest request = new PositionSearchRequest()
            {
                DepartmentCode = deCode,
                PDepartmentCode = pDeCode,
                PositionCode = code
            };
            var data = _position.GetListBySearch(request);
            var treeList = new List<TreeSelectModel>();
            foreach (var item in data)
            {
                TreeSelectModel treeModel = new TreeSelectModel();
                treeModel.id = item.PositionCode;
                treeModel.text = item.PositionName;
                treeModel.parentId = item.ParentPositionCode ?? "";
                //判断是否是第一级
                if (data.Count(t => t.PositionCode == item.ParentPositionCode) == 0)
                {
                    treeModel.parentId = "0";
                }
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeSelectJson());
        }

        /// <summary>
        /// 获取部门信息数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetTreeSelectJsonForDep(string code)
        {
            var data = _position.GetDepartmentList(code);
            var treeList = new List<TreeSelectModel>();
            foreach (var item in data)
            {
                TreeSelectModel treeModel = new TreeSelectModel();
                treeModel.id = item.DepartmentCode;
                treeModel.text = item.DepartmentName;
                treeModel.parentId = item.ParentDepartmentCode;
                treeModel.data = item.ParentDepartmentCode;
                if (data.Count(t => t.DepartmentCode == item.ParentDepartmentCode) == 0)
                {
                    treeModel.parentId = "0";
                }
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeSelectJson());
        }

        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitForm(PositionRequest request, string type)
        {
            if (type == "add")
            {
                var data = _position.Insert(request);
                return Content(data.ToJson());
            }
            else
            {
                var data = _position.Update(request);
                return Content(data.ToJson());
            }
        }

        #endregion

        #region 删除

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteForm(string keyValue)
        {
            var data = _position.Delete(keyValue);
            return Content(data.ToJson());
        }

        #endregion

        #region 岗位菜单
        public ActionResult PositionAuthority()
        {
            return View();
        }
        /// <summary>
        /// 保存角色菜单
        /// </summary>
        /// <param name="postData"></param>
        /// <param name="RoleCode"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult submitForm1(string postData, string PositionCode)
        {
            int IndexofA = postData.IndexOf("[");
            int IndexofB = postData.IndexOf("]");
            string Ru = postData.Substring(IndexofA, IndexofB - IndexofA + 1);
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            List<TbPositionMenu> objs = Serializer.Deserialize<List<TbPositionMenu>>(Ru);
            var data = _positionMenu.Insert(objs, PositionCode);
            return Content(data.ToJson());
        }
        [HttpGet]
        public ActionResult GetGridPositionMenuJson(string PositionCode)
        {
            var data = _positionMenu.GetList(PositionCode);
            return Content(data.ToJson());
        }
        #endregion

        public ActionResult GetPositionTreeJson()
        {
            var treeList = new List<TreeGridModel>();
            //获取组织机构信息
            var department = _position.GetPositionTree(1);
            foreach (var item in department)
            {
                bool hasChildren = department.Count(p => p.ParentCode == item.Code) == 0 ? false : true;
                TreeGridModel treeModel = new TreeGridModel();
                treeModel.id = item.Code;
                treeModel.text = item.Name;
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.ParentCode;
                treeModel.expanded = true;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeGridJson());
        }
    }
}
