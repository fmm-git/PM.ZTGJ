using PM.Business;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.SystemManage.Controllers
{
    public class WinDisplayFieldController : Controller
    {
        private readonly TbFormWindowsShowFieldsLogic _formFields = new TbFormWindowsShowFieldsLogic();
        #region 列表
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 获取分页的列表数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetGridJson(PageSearchRequest pt, string PhysicalTable, string keyword)
        {

            //var data = new
            //{
            //    rows = _formFieldsImp.GetDataListForPage(pt, PhysicalTable, keyword),
            //    total = pt.total,
            //    page = pt.page,
            //    records = pt.records
            //};
            var data = _formFields.GetDataListForPage(pt, PhysicalTable, keyword);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取所有弹框名称
        /// </summary>
        /// <returns></returns>
        public ActionResult GetNoPageGridJson()
        {
            var data = _formFields.GetNoPageGridList();
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取单条数据
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetFormJson(string keyValue, string keyValue1)
        {
            var data = _formFields.FindEntity(keyValue,keyValue1);
            return Content(data.ToJson());
        }
        public ActionResult GetGridHeadJson(string tableName) 
        {
            var data = _formFields.GetGridHeadList(tableName);
            return Content(data.ToJson()); 
        }
        #endregion

        #region 新增、修改
        public ActionResult Form()
        {
            return View();
        }

        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitForm(TbFormWindowsShowFields request, string type)
        {
            if (type == "add")
            {
                var data = _formFields.Insert(request);
                return Content(data.ToJson());
            }
            else
            {
                var data = _formFields.Update(request);
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
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue, string keyValue1)
        {
            var data = _formFields.Delete(keyValue, keyValue1);
            return Content(data.ToJson());
        }
        #endregion
    }
}
