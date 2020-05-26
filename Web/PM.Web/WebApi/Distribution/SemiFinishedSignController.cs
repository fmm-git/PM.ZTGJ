using PM.Business.Distribution;
using PM.Common;
using PM.DataEntity.Distribution.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PM.Web.WebApi.Distribution
{
    /// <summary>
    /// 半成品签收
    /// </summary>
    public class SemiFinishedSignController : ApiController
    {
        private readonly SemiFinishedSignLogic _wasRFLogic = new SemiFinishedSignLogic();
        private readonly SiteDischargeCargoLogic _siteDischargeCargo = new SiteDischargeCargoLogic();

        #region 半成品签收

        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetGridJson([FromUri]SemiFinishedSignRequest request)
        {
            try
            {
                var data = _wasRFLogic.GetDataListForPage(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 编辑页获取数据
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetFormJson(int keyValue)
        {
            try
            {
                var data = _wasRFLogic.FindEntity(keyValue);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 确认签收（配送计划）
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage SubmitForm(int keyValue, string userCode)
        {
            try
            {
                var data = _wasRFLogic.Update(keyValue, userCode,true);
                    return data.ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }
        /// <summary>
        /// 确认签收（线下补录）
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage SemiFinishedSignNew(string OrderCode)
        {
            try
            {
                var data = _wasRFLogic.SemiFinishedSignNew(OrderCode);
                return data.ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        } 
        #endregion

        #region 确认卸货

        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetDischargeCargoList([FromUri]SiteDischargeCargoRequest request)
        {
            try
            {
                var data = _siteDischargeCargo.GetDataListForPage(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 编辑页获取数据
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetDischargeCargoInfo(int keyValue)
        {
            try
            {
                var data = _siteDischargeCargo.FindEntity(keyValue);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

         /// <summary>
        /// 确认卸货
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage DischargeCargoConfirm(int keyValue)
        {
            try
            {
                var data = _siteDischargeCargo.DischargeCargoConfirm(keyValue, true);
                    return data.ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }
        #endregion
    }
}