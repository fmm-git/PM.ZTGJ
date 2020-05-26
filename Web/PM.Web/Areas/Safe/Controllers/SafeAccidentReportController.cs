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
    public class SafeAccidentReportController : BaseController
    {
        //安全事故上报逻辑处理层类
        private readonly SafeAccidentReportLogic _sarLogic = new SafeAccidentReportLogic();
        //
        // GET: /Safe/SafeAccidentReport/

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
                ViewBag.MeetingCode = CreateCode.GetTableMaxCode("SAR", "MeetingCode", "TbSafeAccidentReport");
                ViewBag.UserName = base.UserName;
                ViewBag.UserCode = base.UserCode;
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
            var data = _sarLogic.Details(keyValue);
            ViewBag.BrowseCount = data.Rows[0]["BrowseCount"].ToString();
            ViewBag.ProcessFactoryName = data.Rows[0]["ProcessFactoryName"].ToString();
            ViewBag.MeetingTheme = data.Rows[0]["MeetingTheme"].ToString();
            ViewBag.StartTime = data.Rows[0]["StartTime"].ToString();
            ViewBag.MeetingContent = data.Rows[0]["MeetingContent"].ToString();
            ViewBag.ParticipantsName = data.Rows[0]["ParticipantsName"].ToString();
            ViewBag.InsertUserName = data.Rows[0]["InsertUserName"].ToString();
            ViewBag.InsertTime = data.Rows[0]["InsertTime"].ToString();
            ViewBag.Remarks = data.Rows[0]["Remarks"].ToString();
            return View();
        }

        #endregion

        #region 查询数据

        /// <summary>
        /// 查询本年所有信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAllWeekAccidentReport(PageSearchRequest psr) 
        {
            var data = _sarLogic.GetAllWeekAccidentReport(psr);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 查询本周
        /// </summary>
        /// <param name="sdate"></param>
        /// <param name="edate"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public ActionResult GetWeek(PageSearchRequest psr, string sdate, string edate) 
        {
            var data = _sarLogic.GetWeek(psr,sdate, edate);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 查询往周
        /// </summary>
        /// <param name="sdate"></param>
        /// <param name="edate"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public ActionResult GetWWeek(PageSearchRequest psr,string sdate, string edate,int num)
        {
            var data = _sarLogic.GetWWeek(psr,sdate, edate,num);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 以ID查询安全周例会信息
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult GetFormJson(int keyValue)
        {
            var data = _sarLogic.GetFormJson(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult AnyInfo(int keyValue)
        {
            var data = _sarLogic.AnyInfo(keyValue);
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
                var PlanModel = JsonEx.JsonToObj<TbSafeAccidentReport>(model);
                if (type == "add")
                {
                    var data = _sarLogic.Insert(PlanModel);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _sarLogic.Update(PlanModel);
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
