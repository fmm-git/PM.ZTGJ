using PM.Business.BIExhibition;
using System.Web.Mvc;

namespace PM.Web.Areas.BIExhibition.Controllers
{
    /// <summary>
    /// 质量指标展示
    /// </summary>
    public class QualityIndexController : BaseController
    {
        public readonly QualityIndexLogic _qiLogic = new QualityIndexLogic();
        //
        // GET: /BIExhibition/QualityIndex/

        public ActionResult Index()
        {
            return View();
        }

        ///// <summary>
        ///// 加工厂当月原材料到货分析
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult JGCYCLArrival(PageSearchRequest psr,string ProjectId) 
        //{
        //    var data = _qiLogic.JGCYCLArrival(psr, ProjectId);
        //    return Content(data.ToJson());
        //}

        ///// <summary>
        ///// 当月原材料到货合格数量分析
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult YCLArrivalQualified(PageSearchRequest psr, string ProjectId)
        //{
        //    var data = _qiLogic.YCLArrivalQualified(psr, ProjectId);
        //    return Content(data.ToJson());
        //}

        ///// <summary>
        ///// 加工厂当月配送到站卸货时间分析
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult JGCPSXHAnalysis(PageSearchRequest psr, string ProjectId)
        //{
        //    var data = _qiLogic.JGCPSXHAnalysis(psr, ProjectId);
        //    return Content(data.ToJson());
        //}

        ///// <summary>
        ///// 构件加工厂历史订单分析
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult JGCLSOrderAnalysis(PageSearchRequest psr, string ProjectId)
        //{
        //    var data = _qiLogic.JGCLSOrderAnalysis(psr, ProjectId);
        //    return Content(data.ToJson());
        //}

        /// <summary>
        /// 加工厂当月原材料到货分析
        /// </summary>
        /// <returns></returns>
        public ActionResult JGCYCLArrival(string OrgType, string ProjectId, string HistoryMonth1)
        {
            var data = _qiLogic.JGCYCLArrival(OrgType, ProjectId, HistoryMonth1);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 当月原材料到货合格数量分析
        /// </summary>
        /// <returns></returns>
        public ActionResult YCLArrivalQualified(string OrgType, string ProjectId)
        {
            var data = _qiLogic.YCLArrivalQualified(OrgType, ProjectId);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 加工厂当月配送到站各类问题次数统计
        /// </summary>
        /// <returns></returns>
        public ActionResult JGCPSXHAnalysis(string OrgType, string ProjectId,string HistoryMonth2)
        {
            var data = _qiLogic.JGCPSXHAnalysis(OrgType, ProjectId,HistoryMonth2);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 加工厂当月配送到站问题明细
        /// </summary>
        /// <param name="Jgc">加工厂</param>
        /// <param name="PsType">配送类型</param>
        /// <param name="OrgType">组织机构类型</param>
        /// <param name="ProjectId">项目Id</param>
        /// <returns></returns>
        public ActionResult GetPsDzWtMxList(string Jgc, string PsType, string OrgType, string ProjectId, string HistoryMonth2)
        {
            var data = _qiLogic.GetPsDzWtMxList(Jgc, PsType, OrgType, ProjectId, HistoryMonth2);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 一号加工厂历史订单分析
        /// </summary>
        /// <returns></returns>
        public ActionResult GetOneJgc(string OrgType, string ProjectId)
        {
            var data = _qiLogic.GetOneJgc(OrgType, ProjectId);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 二号加工厂历史订单分析
        /// </summary>
        /// <returns></returns>
        public ActionResult GetTwoJgc(string OrgType, string ProjectId)
        {
            var data = _qiLogic.GetTwoJgc(OrgType, ProjectId);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 三号加工厂历史订单分析
        /// </summary>
        /// <returns></returns>
        public ActionResult GetThreeJgc(string OrgType, string ProjectId)
        {
            var data = _qiLogic.GetThreeJgc(OrgType, ProjectId);
            return Content(data.ToJson());
        }

    }
}
