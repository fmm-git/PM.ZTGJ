using PM.Business.RawMaterial;
using PM.Common.Extension;
using PM.DataEntity;
using PM.DataEntity.RawMaterial.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.RawMaterial.Controllers
{
    /// <summary>
    /// 期末库存 报表
    /// </summary>
    [HandlerLogin]
    public class EndingStocksController : Controller
    {
        private readonly EndingStocksLogic _esLogic = new EndingStocksLogic();

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 首页查询/点击查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMaterials(FPiCiXQPlan ent) 
        {
            var data = _esLogic.GetMaterials(ent);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 原材料总库存用量统计
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetMaterialTotalStockReport(EndingStocksRequest request)
        {
            var data = _esLogic.GetMaterialTotalStockReport(request);
            return Content(data.ToJson());
        }


        /// <summary>
        /// 原材料总库存及订单需求量历史分析
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetMaterialTotalHistoryStockReport(EndingStocksRequest request)
        {
            var data = _esLogic.GetMaterialTotalHistoryStockReport(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 原材料用量总排行
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetMaterialRankingListReport(EndingStocksRequest request)
        {
            var data = _esLogic.GetMaterialRankingListReport(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 加工工艺用料总排行
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetProcessingRankingListReport(EndingStocksRequest request)
        {
            var data = _esLogic.GetProcessingRankingListReport(request);
            return Content(data.ToJson());
        }
    }
}
