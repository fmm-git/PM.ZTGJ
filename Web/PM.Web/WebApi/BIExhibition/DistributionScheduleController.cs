using Dos.ORM;
using Newtonsoft.Json.Linq;
using PM.Business;
using PM.Business.RawMaterial;
using PM.Common;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PM.Web.WebApi.BIExhibition
{
    public class DistributionScheduleController : ApiController
    {
        public readonly DistributionScheduleLogic _dsLogic = new DistributionScheduleLogic();
        private readonly StatisticsReportFormLogic _reportLogic = new StatisticsReportFormLogic();

        #region 配送进度展示

        /// <summary>
        /// 构件加工厂当月配送情况分析
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage DisStatusTj(string OrgType, string ProjectId)
        {
            try
            {
                var data = _dsLogic.DisStatusTj(OrgType, ProjectId);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 一号加工厂
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage OneJgc(string OrgType, string ProjectId)
        {
            try
            {
                var data = _dsLogic.OneJgc(OrgType, ProjectId);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 二号加工厂
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetTwoJia(string OrgType, string ProjectId)
        {
            try
            {
                var data = _dsLogic.TwoJgc(OrgType, ProjectId);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 三号加工厂
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetThreeJia(string OrgType, string ProjectId)
        {
            try
            {
                var data = _dsLogic.ThreeJgc(OrgType, ProjectId);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 订单量统计

        /// <summary>
        /// 当年各月度加工厂订单量分析
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetWorkOrderHistoryMonthReport(string OrgType, string Year, string ProjectId)
        {
            try
            {
                var data = _reportLogic.WorkOrderHistoryFx(OrgType, Year, ProjectId);
                return AjaxResult.Success(data.Item1).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 当月订单类型统计
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetWorkOrderTypeReport(string OrgType, string Year, string Month, string ProjectId)
        {
            try
            {
                var data = _reportLogic.SameMonthWorkOrderTypeTj(OrgType, Year, Month, ProjectId);
                return AjaxResult.Success(data.Item1).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 订单总量分布情况
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage UrgentWorkOrderFbQk(string OrgType, string Year, string Month, string ProjectId, string JgcCode, string TypeName)
        {
            try
            {
                var data = _reportLogic.UrgentWorkOrderFbQk(OrgType, Year, Month, ProjectId, JgcCode, TypeName);
                return AjaxResult.Success(data.Item1).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }
        /// <summary>
        /// 用料类型统计
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage MaterialsUsedTypeTj(string OrgType, string Year, string Month, string ProjectId)
        {
            try
            {
                var data = _reportLogic.MaterialsUsedTypeTj(OrgType, Year, Month, ProjectId);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }


        #endregion

    }
}
