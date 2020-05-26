using PM.Business.Production;
using PM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net.Http;
using PM.DataEntity.Production.ViewModel;
using PM.Business.RawMaterial;
using PM.Business;

namespace PM.Web.WebApi.WorkOrder
{
    public class WorkOrderController : ApiController
    {
        private readonly TbWorkOrderLogic _workorder = new TbWorkOrderLogic();
        private readonly ProblemOrderLogic _problemOrder = new ProblemOrderLogic();
        private readonly StatisticsReportFormLogic _reportLogic = new StatisticsReportFormLogic();
        private readonly ProcessingScheduleLogic _psLogic = new ProcessingScheduleLogic();

        #region 加急订单信息列表
        /// <summary>
        /// App加急订单信息首页
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetUrgentWorkOrderSumWeight(string ProjectId, string ProcessFactoryCode, string OrgId, string orgType)
        {
            try
            {
                var data = _workorder.GetUrgentWorkOrderSumWeight(ProjectId, ProcessFactoryCode, OrgId, orgType);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// App加急订单信息列表(更多)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetUrgentWorkOrderList(string ProjectId, string ProcessFactoryCode, string OrgId, string orgType)
        {
            try
            {
                var data = _workorder.GetUrgentWorkOrderList(ProjectId, ProcessFactoryCode, OrgId, orgType);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region App数据统计
        /// <summary>
        ///  库存数量、加工订单完成数量、配送总量、签收总量
        /// </summary>
        /// <param name="ProjectId">项目id</param>
        /// <param name="ProcessFactoryCode">加工厂id</param>
        /// <param name="OrgId">登录人的组织机构id</param>
        ///  <param name="orgType">登录人的组织机构类型</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetTotelNum(string ProjectId, string ProcessFactoryCode, string OrgId, string orgType)
        {
            try
            {
                DateTime now = DateTime.Now;
                var data = _workorder.GetTotelNum(ProjectId, ProcessFactoryCode, OrgId, orgType,now.Year, now.Month);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }
        #endregion

        #region 延时供货列表
        /// <summary>
        /// 延时供货列表(更多)
        /// </summary>
        /// <param name="ProjectId">项目id</param>
        /// <param name="ProcessFactoryCode">加工厂id</param>
        /// <param name="OrgId">组织机构id</param>
        /// <param name="orgType">组织机构类型</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetDelayedSupplyList(string ProjectId, string ProcessFactoryCode, string OrgId, string orgType)
        {
            try
            {
                var data = _workorder.GetDelayedSupplyList(ProjectId, ProcessFactoryCode, OrgId, orgType);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 延时供货列表（首页）
        /// </summary>
        /// <param name="ProjectId">项目id</param>
        /// <param name="ProcessFactoryCode">加工厂id</param>
        /// <param name="OrgId">组织机构id</param>
        /// <param name="orgType">组织机构类型</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetDelayedSupplySumWeight(string ProjectId, string ProcessFactoryCode, string OrgId, string orgType)
        {
            try
            {
                var data = _workorder.GetDelayedSupplySumWeight(ProjectId, ProcessFactoryCode, OrgId, orgType);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 订单报警
        /// <summary>
        /// 订单报警（获取所有的加急订单,加工状态为已接收或者是加工中）
        /// </summary>
        /// <param name="ProjectId"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetWorkOrderStartList(string ProjectId)
        {
            try
            {
                var data = _workorder.GetWorkOrderStartList(ProjectId);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 订单查询

        #region 订单查询一级

        public HttpResponseMessage GetWorkOrderOneList(string ProjectId)
        {
            try
            {
                var data = _workorder.GetWorkOrderOneList(ProjectId);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 订单查询二级

        public HttpResponseMessage GetWorkOrderTwoList(string OrderCode)
        {
            try
            {
                var data = _workorder.GetWorkOrderTwoList(OrderCode);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #endregion

        #region App订单重量查询

        [HttpGet]
        public HttpResponseMessage GetWorkOrderWeight([FromUri]WorkOrderRequest request)
        {
            try
            {
                var data = _workorder.GetWorkOrderWeight(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }
        #endregion

        #region App订单信息查询

        [HttpGet]
        public HttpResponseMessage GetGridJson([FromUri]WorkOrderRequest request)
        {
            try
            {
                var data = _workorder.GetDataListForPage(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region App订单进度查询

        [HttpGet]
        public HttpResponseMessage GetOrderProgress(string OrderCode)
        {
            try
            {
                var data = _workorder.GetOrderProgress(OrderCode);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region App变更订单

        [HttpGet]
        public HttpResponseMessage GetChangeOrder([FromUri]ProblemOrderRequest request)
        {
            try
            {
                var data = _problemOrder.GetDataListForPage(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 统计报表

        /// <summary>
        /// 订单状态量展示
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetWorkOrderStateReport([FromUri]WorkOrderRequest request)
        {
            try
            {
                var data = _workorder.GetWorkOrderReportForm(request);
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
