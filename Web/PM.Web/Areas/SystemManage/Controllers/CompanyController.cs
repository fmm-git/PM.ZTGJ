using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PM.Business;
using PM.DataEntity;
using PM.DataEntity.System.ViewModel;

namespace PM.Web.Areas.SystemManage.Controllers
{
    public class CompanyController : BaseController
    {
        private CompanyLogic cit = new CompanyLogic();
        //
        // GET: /SystemManage/SystemManages/
        [HttpGet]
        public ActionResult GetCompany()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Form()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Details()
        {
            return View();
        }

        /// <summary>
        /// 查询全部公司或者条件查询加分页
        /// </summary>
        /// <param name="postData"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetAllCompanyOrBySearch(string keyword)
        {
            //var data = new
            //{
            //    rows = cit.GetAllCompanyOrBySearch(pt, keyword),
            //    total = pt.total,
            //    page = pt.page,
            //    records = pt.records
            //};
            //return Content(data.ToJson());
            var data = cit.GetAllCompanyOrBySearch(keyword).ToList();
            var treeList = new List<TreeGridModel>();
            foreach (TbCompany item in data)
            {
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = data.Count(t => t.ParentCompanyCode == item.CompanyCode) == 0 ? false : true;
                treeModel.id = item.CompanyCode;
                treeModel.text = item.CompanyFullName;
                if (data.Count(t => t.CompanyCode == item.ParentCompanyCode) == 0)
                {
                    item.ParentCompanyCode = "0";
                }
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.ParentCompanyCode;
                treeModel.expanded = hasChildren;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeGridJson());
        }

        ///// <summary>
        ///// 查询全部公司或者条件查询(加工厂)
        ///// </summary>
        ///// <param name="postData"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public ActionResult GetAllCompanyOrBySearchJgc(string keyword)
        //{
        //    var data = cit.GetAllCompanyOrBySearchJgc(keyword).ToList();
        //    var treeList = new List<TreeGridModel>();
        //    foreach (TbCompany item in data)
        //    {
        //        TreeGridModel treeModel = new TreeGridModel();
        //        bool hasChildren = data.Count(t => t.ParentCompanyCode == item.CompanyCode) == 0 ? false : true;
        //        treeModel.id = item.CompanyCode;
        //        treeModel.text = item.CompanyFullName;
        //        if (data.Count(t => t.CompanyCode == item.ParentCompanyCode) == 0)
        //        {
        //            item.ParentCompanyCode = "0";
        //        }
        //        treeModel.isLeaf = hasChildren;
        //        treeModel.parentId = item.ParentCompanyCode;
        //        treeModel.expanded = hasChildren;
        //        treeModel.entityJson = item.ToJson();
        //        treeList.Add(treeModel);
        //    }
        //    return Content(treeList.TreeGridJson());
        //}

        /// <summary>
        /// 查询全部公司或者条件查询加分页
        /// </summary>
        /// <param name="postData"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetAllCompanyOrBySearchNew()
        {
            var data = cit.GetAllCompanyOrBySearchNew().ToList();
            var treeList = new List<TreeGridModel>();
            foreach (TbCompany item in data)
            {
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = data.Count(t => t.ParentCompanyCode == item.CompanyCode) == 0 ? false : true;
                treeModel.id = item.CompanyCode;
                treeModel.text = item.CompanyFullName;
                if (data.Count(t => t.CompanyCode == item.ParentCompanyCode) == 0)
                {
                    item.ParentCompanyCode = "0";
                }
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.ParentCompanyCode;
                treeModel.expanded = hasChildren;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeGridJson());
        }

        //同步项目
        [HttpGet]
        public ActionResult SynchronizationPro() 
        {
            var data = cit.SynchronizationPro();
            return Content(data.ToJson());
        }
        /// <summary>
        /// 根据编码查询公司
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult GetFormJson(string keyValue)
        {
            var data = cit.FindEntity(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 查询所有公司
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetSelectJson(string keyValue)
        {
            var data = cit.GetList().ToList();
            var List = new List<TreeSelectModel>();
            foreach (TbCompany item in data)
            {
                TreeSelectModel Model = new TreeSelectModel();
                Model.id = item.CompanyCode;
                Model.text = item.CompanyFullName;
                Model.parentId = item.ParentCompanyCode;
                List.Add(Model);
            }
            return Content(List.TreeSelectJson());
        }

        /// <summary>
        /// 修改公司状态
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public ActionResult EditIsEnable(string code, string val)
        {
            var data = cit.EditIsEnable(code, Convert.ToInt32(val));
            return Content(data.ToJson()); ;
        }

        /// <summary>
        /// 新增公司信息 Or 修改公司信息
        /// </summary>
        /// <param name="company"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public ActionResult SubmitForm(TbCompany company, string type)
        {
            if (type == "add")
            {
                var OnlyVerification = cit.VerificationMethod(company, "add");
                if (OnlyVerification > 0)
                {
                    return Error("公司名称重复！");
                }
                var data = cit.Insert(company);
                return Content(data.ToJson());
            }
            else
            {
                var OnlyVerification = cit.VerificationMethod(company, "edit");
                if (OnlyVerification > 0)
                {
                    return Error("公司名称重复！");
                }
                var data = cit.Update(company);
                return Content(data.ToJson());
            }
        }

        /// <summary>
        /// 删除公司信息
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            var data = cit.Delete(keyValue);
            return Content(data.ToJson());
        }

        #region 获取工区信息 by ProjectId
        public ActionResult GetWorkAreaByProjectId(TbCompanyRequest request)
        {
            var data = cit.GetWorkAreaByProjectId(request);
            return Content(data.ToJson());
        }

        #endregion
    }
}
