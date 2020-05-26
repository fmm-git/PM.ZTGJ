using PM.Business.BIExhibition;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.BIExhibition.Controllers
{
    /// <summary>
    /// 主要设备展示
    /// </summary>
    public class EquipmentExhibitionController : BaseController
    {
        private readonly EquipmentExhibitionLogic _eeLogic = new EquipmentExhibitionLogic();
       
        public ActionResult Index()
        {
            return View();
        }

        ///// <summary>
        ///// 加工厂设备当月产能分析
        ///// </summary>
        ///// <param name="psr"></param>
        ///// <param name="PId"></param>
        ///// <returns></returns>
        //public ActionResult JGCLSEquipmentAnalysis(PageSearchRequest psr)
        //{
        //    var data = _eeLogic.JGCLSEquipmentAnalysis(psr);
        //    return Content(data.ToJson());
        //}

        ///// <summary>
        ///// 加工厂当月设备能耗产能分析
        ///// </summary>
        ///// <param name="psr"></param>
        ///// <param name="PId"></param>
        ///// <returns></returns>
        //public ActionResult JGCThisMonthEquipmentExhibitionAnalysis(PageSearchRequest psr)
        //{
        //    var data = _eeLogic.JGCThisMonthEquipmentExhibitionAnalysis(psr);
        //    return Content(data.ToJson());
        //}

    }
}
