using PM.Business.Safe;
using PM.Common.Extension;
using PM.Domain.WebBase;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PM.Common;

namespace PM.Web.Areas.Safe.Controllers
{
    [HandlerLogin]
    public class SafeDatumManageController : BaseController
    {
        //班前讲话逻辑处理层类
        private readonly SafeDatumManageLogic _sdmlogic = new SafeDatumManageLogic();
        //
        // GET: /Safe/SafeDatumManage/

        #region 视图
        public ActionResult Index()
        {
            ViewBag.OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            return View();
        }
        public ActionResult Form(string type) 
        {
            if (type == "add")
            {
                ViewBag.ContentCode = CreateCode.GetTableMaxCode("SDM", "ContentCode", "TbSafeDatumManage");
                ViewBag.UserName = base.UserName;
                ViewBag.UserCode = base.UserCode;
                ViewBag.SpeechUser = base.UserCode;
                ViewBag.SpeechUserName = base.UserName;
                var OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
                ViewBag.pfType = OrgType;
                if (OrgType == "1")
                {
                    ViewBag.ProcessFactoryCode = OperatorProvider.Provider.CurrentUser.CompanyId;
                    ViewBag.ProcessFactoryName = OperatorProvider.Provider.CurrentUser.ComPanyName;
                }
                else
                {
                    ViewBag.ProcessFactoryCode = "";
                    ViewBag.ProcessFactoryName = "";
                }
            }
            return View();
        }

        [HandlerLogin(Ignore = false)]
        public ActionResult Details(int keyValue)
        {
            var data = _sdmlogic.Details(keyValue);
            ViewBag.BrowseCount = data.Rows[0]["BrowseCount"].ToString();
            ViewBag.ProcessFactoryName = data.Rows[0]["ProcessFactoryName"].ToString();
            ViewBag.SpeechTheme = data.Rows[0]["SpeechTheme"].ToString();
            ViewBag.StartTime = data.Rows[0]["StartTime"].ToString();
            ViewBag.SpeechContent = data.Rows[0]["SpeechContent"].ToString();
            ViewBag.SpeechUserName = data.Rows[0]["SpeechUserName"].ToString();
            ViewBag.InsertUserName = data.Rows[0]["InsertUserName"].ToString();
            ViewBag.InsertTime = data.Rows[0]["InsertTime"].ToString();
            ViewBag.Remarks = data.Rows[0]["Remarks"].ToString();
            return View();
        }

        #endregion

        #region 查询数据

        /// <summary>
        /// 查询本年本月所有信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAllDayDatum(PageSearchRequest psr,int month) 
        {
            var data = _sdmlogic.GetAllDayDatum(psr,month);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 以ID查询班前讲话
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult GetFormJson(int keyValue)
        {
            var data = _sdmlogic.GetFormJson(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult AnyInfo(int keyValue)
        {
            var data = _sdmlogic.AnyInfo(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 以当日查询班前讲话
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult GetToday(PageSearchRequest psr, string keyValue) 
        {
            var data = _sdmlogic.GetToday(psr,keyValue);
            return Content(data.ToJson());
        }

        #endregion

        #region （新增、编辑）数据

        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="model">主表信息</param>
        /// <param name="itemModel">明细信息</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public ActionResult SubmitForm(string model, string type)
        {
            try
            {
                var PlanModel = JsonEx.JsonToObj<TbSafeDatumManage>(model);
                if (type == "add")
                {
                    var data = _sdmlogic.Insert(PlanModel);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _sdmlogic.Update(PlanModel);
                    return Content(data.ToJson());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

    }
}
