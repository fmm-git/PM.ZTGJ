using Newtonsoft.Json.Linq;
using PM.Business.Distribution;
using PM.Common;
using PM.Common.Extension;
using PM.DataEntity;
using PM.Web.WebApi.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PM.Web.WebApi.Distribution
{
    /// <summary>
    /// 运输流程确认
    /// </summary>
    public class TransportFlowController : BaseApiController
    {
        TransportFlowLogic _transportFlow = new TransportFlowLogic();

        /// <summary>
        /// 获取当前司机所在的运输流程信息
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetTransportFlowInfo(string userCode)
        {
            try
            {
                var data = _transportFlow.GetTransportFlowInfo(userCode);
                return data.ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 运输流程出厂确认
        /// </summary>
        /// <param name="jdata"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage FactoryFlowConfirm([FromBody]JObject jdata)
        {
            try
            {
                string modelstr = jdata["model"] == null ? "" : jdata["model"].ToString();
                if (string.IsNullOrWhiteSpace(modelstr))
                    return AjaxResult.Error("参数错误").ToJsonApi();
                var model = JsonEx.JsonToObj<TransportFlowRequestNew>(modelstr);
                model.FlowState = 1;
                var data = _transportFlow.FactoryFlowConfirm(model);
                return data.ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 获取当前司机所在的运输订单流程信息
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetDisEntOrderFlowInfo(int DisEntOrderId)
        {
            try
            {
                var data = _transportFlow.GetDisEntOrderFlowInfo(DisEntOrderId);
                return data.ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 运输订单流程确认
        /// </summary>
        /// <param name="jdata"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage OrderFlowConfirm([FromBody]JObject jdata)
        {
            try
            {
                string modelstr = jdata["model"] == null ? "" : jdata["model"].ToString();
                if (string.IsNullOrWhiteSpace(modelstr))
                    return AjaxResult.Error("参数错误").ToJsonApi();
                var model = JsonEx.JsonToObj<TransportFlowRequestNew>(modelstr);
                if (!string.IsNullOrWhiteSpace(model.Enclosure))
                {
                    var files = JsonEx.JsonToObj<List<FileData>>(model.Enclosure);
                    //保存图片文件
                    if (files.Any())
                    {
                        base.UserCode = model.UserCode;
                        string pictureName = "";
                        switch (model.FlowState)
                        {
                            case 2://进场
                                pictureName = "车辆进场";
                                break;
                            case 3://开始卸货
                                pictureName = "开始卸货";
                                break;
                            case 4://卸货完成
                                pictureName = "卸货完成";
                                break;
                            case 5://出场
                                pictureName = "车辆出场";
                                break;
                            default:
                                break;
                        }
                        foreach (var item in files)
                        {
                            if (!string.IsNullOrWhiteSpace(item.fileName))
                            {
                                string[] extension = item.fileName.Split('.');
                                if (extension.Length == 2)
                                {
                                    item.fileName = pictureName + "." + extension[1];
                                }
                            }
                        }
                        var retFile = base.UploadFileData(files, true);
                        if (retFile == null)
                            return AjaxResult.Error("图片上传失败").ToJsonApi();
                        model.Enclosure = retFile.Item1;
                        model.fileIds = retFile.Item2;
                    }
                }
                var data = _transportFlow.OrderFlowConfirm(model);
                return data.ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 运输流程确认
        /// </summary>
        /// <param name="jdata"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage FlowConfirm([FromBody]JObject jdata)
        {
            try
            {
                string modelstr = jdata["model"] == null ? "" : jdata["model"].ToString();
                if (string.IsNullOrWhiteSpace(modelstr))
                    return AjaxResult.Error("参数错误").ToJsonApi();
                var model = JsonEx.JsonToObj<TransportFlowRequest>(modelstr);
                if (!string.IsNullOrWhiteSpace(model.Enclosure))
                {
                    var files = JsonEx.JsonToObj<List<FileData>>(model.Enclosure);
                    //保存图片文件
                    if (files.Any())
                    {
                        base.UserCode = model.UserCode;
                        string pictureName = "";
                        switch (model.FlowState)
                        {
                            case 1://出厂
                                pictureName = "车辆出厂";
                                break;
                            case 2://进场
                                pictureName = "车辆进场";
                                break;
                            case 3://开始卸货
                                pictureName = "开始卸货";
                                break;
                            case 4://卸货完成
                                pictureName = "卸货完成";
                                break;
                            case 5://出场
                                pictureName = "车辆出场";
                                break;
                            default:
                                break;
                        }
                        foreach (var item in files)
                        {
                            if (!string.IsNullOrWhiteSpace(item.fileName))
                            {
                                string[] extension = item.fileName.Split('.');
                                if (extension.Length == 2)
                                {
                                    item.fileName = pictureName + "." + extension[1];
                                }
                            }
                        }
                        var retFile = base.UploadFileData(files, true);
                        if (retFile == null)
                            return AjaxResult.Error("图片上传失败").ToJsonApi();
                        model.Enclosure = retFile.Item1;
                        model.fileIds = retFile.Item2;
                    }
                }
                var data = _transportFlow.FlowConfirm(model);
                return data.ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 问题填报
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage ProblemReport([FromBody]JObject jdata)
        {
            try
            {
                string modelstr = jdata["model"] == null ? "" : jdata["model"].ToString();
                if (string.IsNullOrWhiteSpace(modelstr))
                    return AjaxResult.Error("参数错误").ToJsonApi();
                var model = JsonEx.JsonToObj<ProblemReportRequest>(modelstr);
                var data = _transportFlow.ProblemReport(model);
                return data.ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }
    }
}