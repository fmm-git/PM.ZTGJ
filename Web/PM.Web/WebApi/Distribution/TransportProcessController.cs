using Newtonsoft.Json.Linq;
using PM.Business.Distribution;
using PM.Common;
using PM.Common.Extension;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PM.Web.WebApi.Distribution
{
    /// <summary>
    /// 配送过程
    /// </summary>
    public class TransportProcessController : ApiController
    {
        TransportProcessLogic _transportProcess = new TransportProcessLogic();

        /// <summary>
        /// 获取配送列表信息
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetGridJson([FromUri]TransportProcessRequest request, bool IsDriver, string UserCode)
        {
            try
            {
                var data = _transportProcess.GetDataListForPage(request, IsDriver, UserCode);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 获取车辆配送路线信息
        /// </summary>
        /// <param name="distributionCode">配送装车单号</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetTransportLine(string distributionCode)
        {
            try
            {
                var data = _transportProcess.GetTransportLine(distributionCode);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 提交车辆当前位置信息
        /// </summary>
        /// <param name="jdata"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage InsertPositionInfo([FromBody]JObject jdata)
        {
            try
            {
                string modelstr = jdata["model"] == null ? "" : jdata["model"].ToString();
                if (string.IsNullOrWhiteSpace(modelstr))
                    return AjaxResult.Error("参数错误").ToJsonApi();
                var model = JsonEx.JsonToObj<TbTransportLine>(modelstr);
                var data = _transportProcess.Insert(model);
                return data.ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }
    }
}