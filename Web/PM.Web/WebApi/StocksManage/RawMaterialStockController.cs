using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using PM.Web.WebApi.Base;
using PM.Business.RawMaterial;
using PM.DataEntity;
using PM.Common;
using PM.DataEntity.RawMaterial.ViewModel;

namespace PM.Web.WebApi.StocksManage
{
    public class RawMaterialStockController  : BaseApiController
    {
        //
        // GET: /RawMaterialStock/
        private readonly RawMaterialStockLogic _rawMaterialStock = new RawMaterialStockLogic();
        private readonly EndingStocksLogic _esLogic = new EndingStocksLogic();
        private readonly StatisticsReportFormLogic _reportLogic = new StatisticsReportFormLogic();

        #region 列表

        /// <summary>
        /// 获取总量信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetTotalInfoJson([FromUri]RawMaterialStockRequest request)
        {
            try
            {
                var data = _rawMaterialStock.GetIndexReportInfo(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }


        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetGridJson([FromUri]RawMaterialStockRequest request)
        {
            try
            {
                var data = _rawMaterialStock.GetIndexListInfo(request);
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
        public HttpResponseMessage GetDetialGridJson([FromUri]RawMaterialStockRequest request)
        {
            var data = _rawMaterialStock.GetDataDetialForPage(request);
            return AjaxResult.Success(data).ToJsonApi();
        }
        #endregion

        #region 统计图表

         /// <summary>
         /// 原材料总库存用量统计
         /// </summary>
         /// <param name="request"></param>
         /// <returns></returns>
         [HttpGet]
         public HttpResponseMessage GetMaterialTotalStockReport([FromUri]EndingStocksRequest request)
         {
             try
             {
                 var data = _esLogic.GetMaterialTotalStockReport(request);
                 return AjaxResult.Success(data).ToJsonApi();
             }
             catch (Exception)
             {
                 return AjaxResult.Error("操作失败").ToJsonApi();
             }
         }

         /// <summary>
         /// 进料与发料对比分析
         /// </summary>
         /// <param name="request"></param>
         /// <returns></returns>
         [HttpGet]
         public HttpResponseMessage GetFeedIssueReport([FromUri]StatisticsReportFormRequest request)
         {
             try
             {
                 var data = _reportLogic.GetReportForm1(request);
                 return AjaxResult.Success(data.Item1).ToJsonApi();
             }
             catch (Exception)
             {
                 return AjaxResult.Error("操作失败").ToJsonApi();
             }
         }

         /// <summary>
         /// 原材料累计用量
         /// </summary>
         /// <param name="request"></param>
         /// <returns></returns>
         [HttpGet]
         public HttpResponseMessage GetMaterialRankingListReport([FromUri]EndingStocksRequest request)
         {
             try
             {
                 var data = _esLogic.GetMaterialRankingListReport(request);
                 return AjaxResult.Success(data).ToJsonApi();
             }
             catch (Exception)
             {
                 return AjaxResult.Error("操作失败").ToJsonApi();
             }
         }
         /// <summary>
         /// 材料月度计划量及批次计划量统计
         /// </summary>
         /// <param name="request"></param>
         /// <returns></returns>
         [HttpGet]
         public HttpResponseMessage GetMonthBatchPlanReport([FromUri]EndingStocksRequest request)
         {
             try
             {
                 var data = _esLogic.GetMonthBatchPlanReport(request);
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
