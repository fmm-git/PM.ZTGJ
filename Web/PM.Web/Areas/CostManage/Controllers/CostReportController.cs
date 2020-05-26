using PM.Business.CostManage;
using PM.DataEntity;
using PM.DataEntity.CostManage.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.CostManage.Controllers
{
    /// <summary>
    /// 成本总报表
    /// </summary>
    [HandlerLogin]
    public class CostReportController : BaseController
    {
        CostReportLogic _costReport = new CostReportLogic();
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 获取分页列表数据(订单)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetOrderGridJson(CostReportRequest request)
        {
            var data = _costReport.GetDataListForPage(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取订单费用明细报表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetOrderReport(CostReportRequest request)
        {
            var data = _costReport.GetOrderReport(request);
            return Content(data.ToJson());
        }


        /// <summary>
        /// 获取本月费用明细报表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetMonthReport(CostReportRequest request)
        {
            var data = _costReport.GetMonthReport(request);
            return Content(data.ToJson());
        }
    }
}
