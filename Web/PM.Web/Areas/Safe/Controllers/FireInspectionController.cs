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
    /// 消防检查
    /// </summary>
    [HandlerLogin]
    public class FireInspectionController : BaseController
    {
        private readonly TbFireInspectionLogic _fireInspection = new TbFireInspectionLogic();

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
        public ActionResult GetGridJson(FireInspectionRequest request)
        {
            var data = _fireInspection.GetDataListForPage(request);
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
                ViewBag.CheckCode = CreateCode.GetTableMaxCode("FIC", "CheckCode", "TbFireInspection");
                string factoryCode = "", factoryName = "";
                if (base.CurrentUser.OrgType == "1")
                {
                    factoryCode = base.CurrentUser.CompanyId;
                    factoryName = base.CurrentUser.ComPanyName;
                }
                ViewBag.FactoryCode = factoryCode;
                ViewBag.FactoryName = factoryName;
            }
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
            var data = _fireInspection.FindEntity(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitForm(TbFireInspection model, string type)
        {
            try
            {
                if (type == "add")
                {
                    model.InsertUserCode = base.UserCode;
                    var data = _fireInspection.Insert(model);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _fireInspection.Update(model);
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
            var data = _fireInspection.Delete(keyValue);
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
            var data = _fireInspection.AnyInfo(keyValue);
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
            //检查编号、加工厂名称、消防设施是否齐全、消防标语是否齐全、消防通道是否畅通、消防管道是否达到要求、检查人、检查时间、问题描述
            //CheckCode,ProcessFactoryName,FireImplement,FireSlogan,FirePassageway,FirePiping,CheckUserNames,CheckDate,ProblemDescription
            //导出数据列
            Dictionary<string, string> cellheader = new Dictionary<string, string> { 
                    { "CheckCode", "检查编号" }, 
                    { "ProcessFactoryName", "加工厂名称" }, 
                    { "FireImplement", "消防设施是否齐全" }, 
                    { "FireSlogan", "消防标语是否齐全" }, 
                    { "FirePassageway", "消防通道是否畅通" }, 
                    { "FirePiping", "消防管道是否达到要求" },
                    { "CheckUserNames", "检查人" },
                    { "CheckDate", "检查时间" },
                    { "ProblemDescription", "问题描述" },
                };
            var request = JsonEx.JsonToObj<FireInspectionRequest>(jsonData);
            var data = _fireInspection.GetExportList(request);
            var fileStream = ExcelHelper.ExportToMemoryStream(cellheader, data, "", "", "消防检查", "");
            return File(fileStream, "application/vnd.ms-excel", "消防检查.xls");
        }

        #endregion

    }
}
