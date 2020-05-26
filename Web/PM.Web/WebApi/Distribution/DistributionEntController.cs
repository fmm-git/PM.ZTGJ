using Newtonsoft.Json.Linq;
using PM.Business.Distribution;
using PM.Business.RawMaterial;
using PM.Business.ShortMessage;
using PM.Common;
using PM.Common.Extension;
using PM.DataEntity;
using PM.Domain.WebBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PM.Web.WebApi.Distribution
{
    /// <summary>
    /// 扫码装车
    /// </summary>
    public class DistributionEntController : ApiController
    {
        private readonly DistributionEntLogic _deBus = new DistributionEntLogic();
        private readonly CarInfoLogic _carInfo = new CarInfoLogic();

        #region 列表

        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetGridJson([FromUri]FPiCiXQPlan request)
        {
            try
            {
                var data = _deBus.GetAllOrBySearch(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 编辑

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
                int? keyValue1=null;
                var data = _deBus.GetFormJson(keyValue, keyValue1);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception e)
            {
                return AjaxResult.Error(e.Message).ToJsonApi();
            }
        }

        /// <summary>
        /// 新增数据提交
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage SubmitForm([FromBody]JObject jdata)
        {
            try
            {
                string modelstr = jdata["model"] == null ? "" : jdata["model"].ToString();
                string orderstr = jdata["orderModel"] == null ? "" : jdata["orderModel"].ToString();
                string itemstr = jdata["itemModel"] == null ? "" : jdata["itemModel"].ToString();
                if (string.IsNullOrWhiteSpace(modelstr))
                    return AjaxResult.Error("参数错误").ToJsonApi();
                var model = JsonEx.JsonToObj<TbDistributionEnt>(modelstr);
                var order = JsonEx.JsonToObj <List<TbDistributionEntOrder>>(orderstr);
                var items = JsonEx.JsonToObj<List<TbDistributionEntItem>>(itemstr);
                model.InsertTime = DateTime.Now;
                var data = _deBus.Insert(model, order, items, true);
                return data.ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 弹框

        ///// <summary>
        ///// 配送计划弹窗
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public HttpResponseMessage GetPSJHGridJson([FromUri]DistributionPlanRequest request)
        //{
        //    var data = _deBus.GetPSJHGridJson(request);
        //    return data.ToJsonApi();
        //}

        ///// <summary>
        ///// 新增明细弹窗
        ///// </summary>
        ///// <param name="keyValue"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public HttpResponseMessage GetPSJHItemGridJson([FromUri]DistributionPlanRequest request)
        //{
        //    var data = _deBus.GetPSJHItemGridJson(request);
        //    return data.ToJsonApi();
        //}
        [HttpGet]
        /// <summary>
        /// 可配送订单弹窗
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public HttpResponseMessage GetJgcPsOrder([FromUri]DistributionPlanRequest request, string ProcessFactoryCode)
        {
            var data = _deBus.GetJgcPsOrder(request, ProcessFactoryCode);
            return data.ToJsonApi();
        }
        [HttpGet]
        /// <summary>
        /// 选择可配送订单明细信息
        /// </summary>
        /// <param name="request"></param>
        /// <param name="PSJHCode"></param>
        /// <returns></returns>
        public HttpResponseMessage GetPsOrderItem([FromUri]DistributionPlanRequest request, string OrderCode)
        {
            var data = _deBus.GetPsOrderItem(request, OrderCode);
            return data.ToJsonApi();
        }
        #endregion

        #region 自动生成编号

        [HttpGet]
        public HttpResponseMessage GetTableMaxCode()
        {
            var DistributionCode = CreateCode.GetTableMaxCode("PSZC", "DistributionCode", "TbDistributionEnt");
            return AjaxResult.Success(DistributionCode).ToJsonApi();
        }

        #endregion

        #region 获取二维码信息

        /// <summary>
        /// 获取二维码信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetOrderItemByQRCodeNew(int id)
        {
            try
            {
                var data = _deBus.GetOrderItemByQRCode(id);
                return data.ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 获取车辆驾驶员信息

        /// <summary>
        /// 获取车辆信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetCarInfoList([FromUri]CarInfoRequest request)
        {
            try
            {
                var data = _carInfo.GetDataListForPage(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 获取驾驶员信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetDriverInfoList([FromUri]CarInfoRequest request)
        {
            try
            {
                var data = _carInfo.GetDataItemListForPage(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 获取站点联系人
        /// <summary>
        /// 获取站点联系人
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetWorkAreaUser([FromUri]TbUserRequest request, string CompanyCode)
        {
            try
            {
                var data = _deBus.GetWorkAreaUser(request, CompanyCode);
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