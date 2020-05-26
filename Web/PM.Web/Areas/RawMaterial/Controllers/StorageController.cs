using PM.Business.RawMaterial;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.RawMaterial.Controllers
{
    /// <summary>
    /// 仓库管理
    /// </summary>
    [HandlerLogin]
    public class StorageController : BaseController
    {
        private readonly StorageLogic _storage = new StorageLogic();

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
        public ActionResult GetGridJson(StorageRequest request)
        {
            var data = _storage.GetDataListForPage(request);
            return Content(data.ToJson());
        }

        #endregion

        #region 编辑

        /// <summary>
        /// 新增、修改页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Form()
        {
            ViewBag.UserName = base.UserName;
            return View();
        }

        /// <summary>
        /// 查看页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Details()
        {
            return View();
        }

        /// <summary>
        /// 编辑/查看页获取数据
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult GetFormJson(int keyValue)
        {
            var data = _storage.FindEntity(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitForm(TbStorage param, string type)
        {
            try
            {
                if (type == "add")
                {
                    var data = _storage.Insert(param);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _storage.Update(param);
                    return Content(data.ToJson());
                }
            }
            catch (Exception)
            {

                throw;
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
        public ActionResult DeleteForm(int keyValue)
        {
            var data = _storage.Delete(keyValue);
            return Content(data.ToJson());
        }

        #endregion

        #region 信息验证

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult AnyInfo(int keyValue)
        {
            var data = _storage.AnyInfo(keyValue);
            return Content(data.ToJson());
        }

        #endregion

        #region 导出

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="ProjectCode"></param>
        /// <returns></returns>
        public ActionResult OutputExcel(string jsonData)
        {
            //仓库名称、仓库归属、仓库属性、仓库区域、仓库地址、管理员、联系方式。
            //StorageName,ProcessFactoryName,StorageAttributeText,AreaCode,StorageAdd,UserName,Tel
            //导出数据列
            Dictionary<string, string> cellheader = new Dictionary<string, string> { 
                    { "StorageName", "仓库名称" }, 
                    { "ProcessFactoryName", "仓库归属" }, 
                    { "StorageAttributeText", "仓库属性" }, 
                    { "AreaCode", "仓库区域" }, 
                    { "StorageAdd", "仓库地址" }, 
                    { "UserName", "管理员" },
                    { "Tel", "联系方式" },
                };
            var request = JsonEx.JsonToObj<StorageRequest>(jsonData);
            var data = _storage.GetExportList(request);
            var fileStream = ExcelHelper.ExportToMemoryStream(cellheader, data, "", "", "仓库管理", "");
            return File(fileStream, "application/vnd.ms-excel", "仓库管理.xls");
        }
        #endregion

    }
}
