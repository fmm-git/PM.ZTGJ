using PM.Business;
using PM.Business.Safe;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataEntity;
using PM.DataEntity.Safe.ViewModel;
using PM.Domain.WebBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.Safe.Controllers
{
    /// <summary>
    /// 安全检查
    /// </summary>
    [HandlerLogin]
    public class SafeCheckController : BaseController
    {

        private readonly SafeCheckLogic _safeCheck = new SafeCheckLogic();

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
        public ActionResult GetGridJson(SafeCheckRequest request)
        {
            var data = _safeCheck.GetDataListForPage(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取列表数据(明细)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetSafeCheckItemGridJson(SafeCheckRequest request)
        {
            var data = _safeCheck.GetSafeCheckItemDataList(request);
            return Content(data.ToJson());
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
                ViewBag.UserName = base.UserName;
                ViewBag.SafeCheckCode = CreateCode.GetTableMaxCode("AQJC", "SafeCheckCode", "TbSafeCheck");
                string factoryCode = "", factoryName = "";
                if (base.CurrentUser.OrgType == "1")
                {
                    factoryCode = base.CurrentUser.CompanyId;
                    factoryName = base.CurrentUser.ComPanyName;
                }
                ViewBag.FactoryCode = factoryCode;
                ViewBag.FactoryName = factoryName;
            }
            var safeState = new DataDictionaryLogic().GetDicByCode("SafeState")
              .Select(p => string.Join(",", string.Format("{0}:{1}", p.DictionaryCode, p.DictionaryText)))
              .ToJson().ToString().Replace("[", "").Replace("]", "").Replace("\"", "").Replace(",", ";");
            ViewBag.SafeState = safeState;
            return View();
        }

        /// <summary>
        /// 查看页面
        /// </summary>
        /// <returns></returns>
        [HandlerLogin(Ignore = false)]
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
            var data = _safeCheck.FindEntity(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitForm(string model, string itemModel, string type)
        {
            try
            {
                var safeCheckModel = JsonEx.JsonToObj<TbSafeCheck>(model);
                var safeCheckItem = JsonEx.JsonToObj<List<TbSafeCheckItem>>(itemModel);
                if (type == "add")
                {
                    safeCheckModel.InsertUserCode = base.UserCode;
                    var data = _safeCheck.Insert(safeCheckModel, safeCheckItem);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _safeCheck.Update(safeCheckModel, safeCheckItem);
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
            var data = _safeCheck.Delete(keyValue);
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
            var data = _safeCheck.AnyInfo(keyValue);
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
            //检查编号、加工厂名称、检查类型、参与人员、检查人、检查时间、问题描述
            //SafeCheckCode,ProcessFactoryName,CheckTypeName,PartInUsers,CheckUserName,CheckTime,Remark
            //导出数据列
            Dictionary<string, string> cellheader = new Dictionary<string, string> { 
                    { "SafeCheckCode", "检查编号" }, 
                    { "ProcessFactoryName", "加工厂名称" }, 
                    { "CheckTypeName", "检查类型" }, 
                    { "PartInUsers", "参与人员" }, 
                    { "CheckUserName", "检查人" },
                    { "CheckTime", "检查时间" },
                    { "Remark", "问题描述" },
                };
            var request = JsonEx.JsonToObj<SafeCheckRequest>(jsonData);
            var data = _safeCheck.GetExportList(request);
            var fileStream = ExcelHelper.ExportToMemoryStream(cellheader, data, "", "", "安全检查", "");
            return File(fileStream, "application/vnd.ms-excel", "安全检查.xls");
        }

        #endregion
    }
}
