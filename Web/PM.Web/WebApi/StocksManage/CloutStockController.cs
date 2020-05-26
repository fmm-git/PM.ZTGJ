using PM.Common;
using PM.DataEntity;
using System;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using PM.Web.WebApi.Base;
using PM.Business.RawMaterial;

namespace PM.Web.WebApi.StocksManage
{
    public class CloutStockController : BaseApiController
    {
        private readonly CloutStockLogic _cloutStock = new CloutStockLogic();

        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetGridJson([FromUri]CloutStockRequest request)
        {
            try
            {
                var data = _cloutStock.GetDataListForPage(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }
        /// <summary>
        /// 获取分页列表数据(明细)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetDetialGridJson([FromUri]CloutStockRequest request)
        {
            var data = _cloutStock.GetDataListForPage(request);
            return AjaxResult.Success(data).ToJsonApi();
        }

        #region 统计报表

        /// <summary>
        /// 订单状态量展示
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetCloutStockTotalReport([FromUri]CloutStockReportRequest request)
        {
            try
            {
                var data = _cloutStock.GetCloutStockTotalReport(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }
        /// <summary>
        /// 余料产生及使用量统计
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetCloutStockUseReport([FromUri]CloutStockReportRequest request)
        {
            try
            {
                var data = _cloutStock.GetCloutStockUseReport(request);
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
