using Dos.ORM;
using Newtonsoft.Json.Linq;
using PM.Business;
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
    /// <summary>
    /// 加工进度展示
    /// </summary>
    public class ProcessingScheduleController : ApiController
    {
        /// <summary>
        /// 加工进度展示 处理类
        /// </summary>
        public readonly ProcessingScheduleLogic _psLogic = new ProcessingScheduleLogic();
        //
        // GET: /ProcessingSchedule/


        /// <summary>
        /// 当月订单状态量统计
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage WorkOrderStatusTj(string OrgType, string ProjectId)
        {
            try
            {
                var data = _psLogic.WorkOrderStatusTj(OrgType, ProjectId);
                return AjaxResult.Success(data.Item1).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 当月订单完成量统计
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage WorkOrderWclTj(string OrgType, string ProjectId)
        {
            
            try
            {
                var data = _psLogic.WorkOrderWclTj(OrgType, ProjectId);
                return AjaxResult.Success(data.Item1).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 当月订单配送进度统计
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage WorkOrderProgressTj(string OrgType, string ProjectId)
        {
            try
            {
                var data = _psLogic.WorkOrderProgressTj(OrgType, ProjectId);
                return AjaxResult.Success(data.Item1).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }
    }
}
