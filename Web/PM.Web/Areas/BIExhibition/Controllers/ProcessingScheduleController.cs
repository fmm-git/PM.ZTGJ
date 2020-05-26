using PM.Business;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.BIExhibition.Controllers
{
    /// <summary>
    /// 加工进度展示
    /// </summary>
    public class ProcessingScheduleController : BaseController
    {
        /// <summary>
        /// 加工进度展示处理类
        /// </summary>
        private readonly ProcessingScheduleLogic _psLogic = new ProcessingScheduleLogic();
        //
        // GET: /BIExhibition/ProcessingSchedule/

        public ActionResult Index()
        {
            return View();
        }

        #region 展示

        /// <summary>
        /// 甘特图查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetGanttListJson(PageSearchRequest psr,string PId) 
        {
            string strLinks = "[]";//线条数据信息
            string strDatas = "[]";
            var data = _psLogic.GetGanttList(psr,PId);
            if (data.Rows.Count > 0)
            {
                data.Columns.Add(new DataColumn("open"));
                for (var i = 0; i < data.Rows.Count; i++)
                {
                    var prog = decimal.Round(decimal.Parse(data.Rows[i]["progress"].ToString()), 2);
                    data.Rows[i]["progress"] = prog;
                    data.Rows[0]["open"] = "true";
                }
            }
            strDatas = data.ToJson();
            var strJSON = "{\"data\":" + strDatas + ",\"links\":" + strLinks + "}";
            return Content(strJSON);
        }

        /// <summary>
        /// 站点订单进度分析
        /// </summary>
        /// <returns></returns>
        public ActionResult GetSiteSchedule(PageSearchRequest psr,string PId) 
        {
            var data = _psLogic.GetSiteSchedule(psr,PId);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 加工厂各类订单状态量分析
        /// </summary>
        /// <returns></returns>
        public ActionResult PFKOSAnalysis(PageSearchRequest psr,string PId)
        {
            var data = _psLogic.PFKOSAnalysis(psr,PId);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 订单进度
        /// </summary>
        /// <returns></returns>
        public ActionResult OrderJinDu(PageSearchRequest psr, string PId) 
        {
            var data = _psLogic.OrderJinDu(psr,PId);
            return Content(data.ToJson());
        }

        #endregion

        #region 新版统计报表
        /// <summary>
        /// 当月订单状态量统计
        /// </summary>
        /// <param name="OrgType"></param>
        /// <param name="ProjectId"></param>
        /// <returns></returns>
        public ActionResult WorkOrderStatusTj(string OrgType, string ProjectId) 
        {
            var data = _psLogic.WorkOrderStatusTj(OrgType, ProjectId);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 当月订单完成量统计
        /// </summary>
        /// <param name="OrgType"></param>
        /// <param name="ProjectId"></param>
        /// <returns></returns>
        public ActionResult WorkOrderWclTj(string OrgType, string ProjectId)
        {
            var data = _psLogic.WorkOrderWclTj(OrgType, ProjectId);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 当月订单配送进度统计
        /// </summary>
        /// <param name="OrgType"></param>
        /// <param name="ProjectId"></param>
        /// <returns></returns>
        public ActionResult WorkOrderProgressTj(string OrgType, string ProjectId)
        {
            var data = _psLogic.WorkOrderProgressTj(OrgType, ProjectId);
            return Content(data.ToJson());
        }

        #endregion
    }
}
