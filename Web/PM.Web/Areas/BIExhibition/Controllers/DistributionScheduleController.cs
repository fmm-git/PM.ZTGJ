using PM.Business;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.BIExhibition.Controllers
{
    /// <summary>
    /// 配送进度展示
    /// </summary>
    public class DistributionScheduleController : BaseController
    {
        /// <summary>
        /// 配送进度展示处理类
        /// </summary>
        private readonly DistributionScheduleLogic _dsLogic = new DistributionScheduleLogic();
        //
        // GET: /BIExhibition/DistributionSchedule/

        public ActionResult Index()
        {
            return View();
        }

        #region 展示

        /// <summary>
        /// 构件加工厂当月配送情况分析
        /// </summary>
        /// <returns></returns>
        public ActionResult GetGJCDYPSAnalysis(PageSearchRequest psr, string ProjectId) 
        {
            var data = _dsLogic.GetGJCDYPSAnalysis(psr, ProjectId);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 一号加工厂
        /// </summary>
        /// <returns></returns>
        public ActionResult GetOneJia(PageSearchRequest psr, string ProjectId) 
        {
            var data = _dsLogic.GetOneJia(psr, ProjectId);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 二号加工厂
        /// </summary>
        /// <returns></returns>
        public ActionResult GetTwoJia(PageSearchRequest psr, string ProjectId)
        {
            var data = _dsLogic.GetTwoJia(psr, ProjectId);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 三号加工厂
        /// </summary>
        /// <returns></returns>
        public ActionResult GetThreeJia(PageSearchRequest psr, string ProjectId)
        {
            var data = _dsLogic.GetThreeJia(psr, ProjectId);
            return Content(data.ToJson());
        }

        #endregion

        #region 新版配送统计

        /// <summary>
        /// 当月各个加工厂配送情况统计
        /// </summary>
        /// <param name="OrgType"></param>
        /// <param name="ProjectId"></param>
        /// <returns></returns>
        public ActionResult DisStatusTj(string OrgType, string ProjectId)
        {
            var data = _dsLogic.DisStatusTj(OrgType, ProjectId);
            return Content(data.ToJson());
        }

        public ActionResult OneJgc(string OrgType, string ProjectId)
        {
            var data = _dsLogic.OneJgc(OrgType, ProjectId);
            return Content(data.ToJson());
        }
        public ActionResult TwoJgc(string OrgType, string ProjectId)
        {
            var data = _dsLogic.TwoJgc(OrgType, ProjectId);
            return Content(data.ToJson());
        }
        public ActionResult ThreeJgc(string OrgType, string ProjectId)
        {
            var data = _dsLogic.ThreeJgc(OrgType, ProjectId);
            return Content(data.ToJson());
        }
        #endregion

    }
}
