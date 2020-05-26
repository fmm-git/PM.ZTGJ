using Dos.ORM;
using Newtonsoft.Json.Linq;
using PM.Business.EarlyWarning;
using PM.Common;
using PM.Common.Extension;
using PM.DataEntity;
using PM.DataEntity.EarlyWarning.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace PM.Web.WebApi.EarlyWarning
{
    public class EarlyWarningController : ApiController
    {
        private readonly TbFormEarlyWarningNodeInfoLogic _earlywarning = new TbFormEarlyWarningNodeInfoLogic();

        #region 预警信息列表
        /// <summary>
        /// 预警信息列表
        /// </summary>
        /// <param name="UserCode">登录人编号</param>
        /// <param name="Start">预警状态</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetEarlyWarningInfoList(string UserCode, int Start)
        {
            try
            {
                var data = _earlywarning.GetEarlyWarningInfoList(UserCode, Start);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 流程预警信息列表
        /// </summary>
        /// <param name="UserCode">登录人编号</param>
        /// <param name="Start">预警状态</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetFlowEarlyWarningInfoList(string UserCode, int Start)
        {
            try
            {
                var data = _earlywarning.GetFlowEarlyWarningInfoList(UserCode, Start);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }
        /// <summary>
        /// 表单预警信息列表
        /// </summary>
        /// <param name="UserCode">登录人编号</param>
        /// <param name="Start">预警状态</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetFormEarlyWarningInfoList(string UserCode, int Start)
        {
            try
            {
                var data = _earlywarning.GetFormEarlyWarningInfoList(UserCode, Start);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }
        #endregion

        /// <summary>
        /// 处理流程预警信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage HandleEarlyWarning(int ID, string EwType)
        {
            try
            {
                var data = _earlywarning.HandleEarlyWarning(ID, EwType);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 配送预警信息列表
        /// </summary>
        /// <param name="UserCode">登录人编号</param>
        /// <param name="ProjectId">项目编号</param>
        /// <param name="Start">预警状态</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetDeliveryEarlyWarningList(int Start, string UserCode, string ProjectId)
        {
            try
            {
                var data = _earlywarning.GetDeliveryEarlyWarningList(Start, UserCode, ProjectId);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }


        #region App预警信息

        /// <summary>
        /// 报警饼图信息分析
        /// </summary>
        /// <param name="Year"></param>
        /// <param name="Month"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetAlarmMessage(string Year, string Month,string ProjectId,string ProcessFactoryCode,string CompanyId)
        {
            try
            {
                var data = _earlywarning.GetAlarmMessage(Year, Month, ProjectId,ProcessFactoryCode,CompanyId);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }
        /// <summary>
        /// 审批超时
        /// </summary>
        /// <param name="Year"></param>
        /// 
        /// <param name="Month"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetSpCsYjList(string Year, string Month, string ProjectId, string ProcessFactoryCode, string CompanyId)
        {
            try
            {
                var data = _earlywarning.GetSpCsYjList(Year, Month, ProjectId, ProcessFactoryCode, CompanyId);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 未按时提报月度需求计划
        /// </summary>
        /// <param name="DateType"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetWbydxqjhYjList(string Year, string Month, string ProjectId, string ProcessFactoryCode, string CompanyId)
        {
            try
            {
                var data = _earlywarning.GetWbydxqjhYjList(Year, Month, ProjectId, ProcessFactoryCode, CompanyId);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 原材料供货超时
        /// </summary>
        /// <param name="DateType"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetYclghCsYjList(string Year, string Month, string ProjectId, string ProcessFactoryCode, string CompanyId)
        {
            try
            {
                var data = _earlywarning.GetYclghCsYjList(Year, Month, ProjectId, ProcessFactoryCode, CompanyId);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 加急订单个数
        /// </summary>
        /// <param name="DateType"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetJjOrderYjList(string Year, string Month, string ProjectId, string ProcessFactoryCode, string CompanyId)
        {
            try
            {
                var data = _earlywarning.GetJjOrderYjList(Year, Month, ProjectId, ProcessFactoryCode, CompanyId);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 加工进度滞后
        /// </summary>
        /// <param name="DateType"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetJgOrderZhYjList(string Year, string Month, string ProjectId, string ProcessFactoryCode, string CompanyId)
        {
            try
            {
                var data = _earlywarning.GetJgOrderZhYjList(Year, Month, ProjectId, ProcessFactoryCode, CompanyId);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 配送超期
        /// </summary>
        /// <param name="DateType"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetPsCsYjList(string Year, string Month, string ProjectId, string ProcessFactoryCode, string CompanyId)
        {
            try
            {
                var data = _earlywarning.GetPsCsYjList(Year, Month, ProjectId, ProcessFactoryCode, CompanyId);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 卸货超期
        /// </summary>
        /// <param name="DateType"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetXhCsYjList(string Year, string Month, string ProjectId, string ProcessFactoryCode, string CompanyId)
        {
            try
            {
                var data = _earlywarning.GetXhCsYjList(Year, Month, ProjectId, ProcessFactoryCode, CompanyId);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 签收超期
        /// </summary>
        /// <param name="DateType"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetQsCsYjList(string Year, string Month, string ProjectId, string ProcessFactoryCode, string CompanyId)
        {
            try
            {
                var data = _earlywarning.GetQsCsYjList(Year, Month, ProjectId, ProcessFactoryCode, CompanyId);
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
