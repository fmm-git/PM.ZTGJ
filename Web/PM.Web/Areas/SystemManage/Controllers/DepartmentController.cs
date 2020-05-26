using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PM.Business;
using PM.DataEntity;

namespace PM.Web.Areas.SystemManage.Controllers
{
    public class DepartmentController : BaseController
    {
        private DepartmentLogic dit = new DepartmentLogic();
        //
        // GET: /SystemManage/Department/

        /// <summary>
        /// 部门首页
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 部门查询页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Details()
        {
            return View();
        }
        /// <summary>
        /// 部门编辑页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Form(string type, string companyCode)
        {
            if (type == "add")
            {
                if (!string.IsNullOrEmpty(companyCode))
                {
                    ViewBag.DCode = dit.FindEntityNumber(companyCode);
                }
            }
            return View();
        }

        /// <summary>
        /// 部门公司分类导航
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCompanyMenu() 
        {
            var data = dit.GetCompanyMenu().ToList();
            var treeList = new List<TreeGridModel>();
            foreach (TbCompany item in data)
            {
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = data.Count(t => t.ParentCompanyCode == item.CompanyCode) == 0 ? false : true;
                treeModel.id = item.CompanyCode;
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.ParentCompanyCode;
                treeModel.expanded = false;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeGridJson());
        }

        /// <summary>
        /// 查询全部部门or条件查询
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ActionResult GetAllDepOrBySearch(string keyword)
        {
            var data = dit.GetAllDepOrBySearch(keyword).ToList();
            var treeList = new List<TreeGridModel>();
            foreach (TbDepartment item in data)
            {
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = data.Count(t => t.ParentDepartmentCode == item.DepartmentCode) == 0 ? false : true;
                treeModel.id = item.DepartmentCode;
                if (data.Count(t => t.DepartmentCode == item.ParentDepartmentCode) == 0)
                {
                    item.ParentDepartmentCode = "0";
                }
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.ParentDepartmentCode;
                treeModel.expanded = hasChildren;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeGridJson());
        }
        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetGridJson(TbDepartmentRequest request)
        {
            var data = dit.GetDataListForPage(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 根据公司编码进行查询部门信息
        /// </summary>
        /// <param name="vd"></param>
        /// <returns></returns>
        public ActionResult GetDepByCompany(string vd)
        {
            var data = dit.GetDepByCompany(vd).ToList();
            var treeList = new List<TreeGridModel>();
            foreach (TbDepartment item in data)
            {
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = data.Count(t => t.ParentDepartmentCode == item.DepartmentCode) == 0 ? false : true;
                treeModel.id = item.DepartmentCode;
                if (data.Count(t => t.DepartmentCode == item.ParentDepartmentCode) == 0)
                {
                    item.ParentDepartmentCode = "0";
                }
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.ParentDepartmentCode;
                treeModel.expanded = hasChildren;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeGridJson());
        }

        /// <summary>
        /// 根据部门Code查询用户信息
        /// </summary>
        /// <param name="psr"></param>
        /// <param name="keyword"></param>
        /// <param name="DepartmentCode"></param>
        /// <returns></returns>
        public ActionResult GetDepartmentUserList(PageSearchRequest psr, string keyword, string DCode,string CCode) 
        {
            var data = dit.GetDepartmentUserList(psr, keyword, DCode, CCode);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 根据公司Code查询所有所属部门
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetSelectJson(string value)
        {
            var data = dit.GetListByCode(value).ToList();
            var List = new List<TreeSelectModel>();
            foreach (TbDepartment item in data)
            {
                TreeSelectModel Model = new TreeSelectModel();
                Model.id = item.DepartmentCode;
                Model.text = item.DepartmentName;
                Model.parentId = item.ParentDepartmentCode;
                List.Add(Model);
            }
            return Content(List.TreeSelectJson());
        }

        /// <summary>
        /// 根据编码查询部门
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult GetFormJson(string keyValue,string ComCode)
        {
            var data = dit.FindEntity(keyValue, ComCode);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 查询公司是否存在部门
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult GetIsExistDep(string keyValue) 
        {
            var data = dit.GetIsExistDep(keyValue);
            if (data.message == "成功")
            {
                return Content("1");
            }
            return Content("2");
        }

        /// <summary>
        /// 复制部门
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult CopeDepartment(string keyValue,string CCode) 
        {
            var cname = base.UserName;
            var data = dit.CopeDepartment(keyValue, CCode, cname);
            if (data.message == "操作成功") 
            {
                return Content("1");
            }
            else if (data.message == "操作失败")
            {
                return Content("2");
            }
            else 
            {
                return Content(data.ToJson());
            }
        }

        /// <summary>
        /// 新增公司信息 Or 修改公司信息
        /// </summary>
        /// <param name="company"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public ActionResult SubmitForm(TbDepartment company, string type)
        {
            if (type == "add")
            {
                var data = dit.Insert(company);
                return Content(data.ToJson());
            }
            else
            {
                var data = dit.Update(company);
                return Content(data.ToJson());
            }
        }

        /// <summary>
        /// 删除部门信息
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue,string ComCode)
        {
            var data = dit.Delete(keyValue,ComCode);
            return Content(data.ToJson());
        }
    }
}
