using PM.Business.Production;
using PM.Business.System;
using PM.Common;
using PM.DataEntity.System.ViewModel;
using PM.Web.WebApi.Base;
using System;
using System.Net.Http;
using System.Web.Http;

namespace PM.Web.WebApi.Home
{
    public class HomeController : BaseApiController
    {
        private readonly HomeLogic _Home = new HomeLogic();
        private readonly TbWorkOrderLogic _workorder = new TbWorkOrderLogic();

        #region 登录页

        /// <summary>
        /// 加工厂数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetFactoryInfo([FromUri]HomeRequest request)
        {
            try
            {
                var data = _Home.GetFactoryInfo(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        #endregion

        #region 首页

        /// <summary>
        /// 产能信息
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage GetCapacityNum([FromUri]HomeRequest request)
        {
            try
            {
                var data = _workorder.GetCapacityNum(request.ProcessFactoryCode, request.DayMonthStr);
                if (data != null && data.Rows.Count > 0)
                {
                    var a = new
                    {
                        ycl = data.Rows[0]["Capacity"],
                        sjcz = data.Rows[0]["WeightSmallPlan"],
                        sjfh = data.Rows[0]["ActualLoadNew"]
                    };
                    return AjaxResult.Success(a).ToJsonApi();
                }
                else
                {
                    var a = new
                    {
                        ycl = 0,
                        sjcz = 0,
                        sjfh = 0
                    };
                    return AjaxResult.Success(a).ToJsonApi();
                }
            }
            catch (Exception ex)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 获取年月日数据
        /// </summary>
        /// <returns></returns> 
        [HttpGet]
        public HttpResponseMessage GetDMYData([FromUri]HomeRequest request)
        {
            try
            {
                var data = _Home.GetDMYData(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 订单信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>   
        [HttpGet]
        public HttpResponseMessage GetOrderData([FromUri]HomeRequest request)
        {
            try
            {
                var data = _Home.GetOrderData(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 获取库存信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetRawMaterialStockData([FromUri]HomeRequest request)
        {
            try
            {
                var data = _Home.GetRawMaterialStockData(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 获取生产计划阶段数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>  
        [HttpGet]
        public HttpResponseMessage GetRawMaterialStockPlanData([FromUri]HomeRequest request)
        {
            try
            {
                var data = _Home.GetRawMaterialStockPlanData(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 月度计划统计图
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>  
        [HttpGet]
        public HttpResponseMessage GetMonthPlanReport([FromUri]HomeRequest request)
        {
            try
            {
                var data = _Home.GetMonthPlanReport(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 加工订单统计图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetOrderInfoReport([FromUri]HomeRequest request)
        {
            try
            {
                var data = _Home.GetOrderInfoReport(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 供货单统计图
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns> 
        [HttpGet]
        public HttpResponseMessage GetSupplyDataReport([FromUri]HomeRequest request)
        {
            try
            {
                var data = _Home.GetSupplyDataReport(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 历史接单量统计
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>  
        [HttpGet]
        public HttpResponseMessage GetOrderMore([FromUri]HomeRequest request)
        {
            try
            {
                var data = _Home.GetOrderMore(request);
                return AjaxResult.Success(data).ToJsonApi();
            }
            catch (Exception)
            {
                return AjaxResult.Error("操作失败").ToJsonApi();
            }
        }

        /// <summary>
        /// 供应量需求量统计
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>    
        [HttpGet]
        public HttpResponseMessage GetSupplyrMore([FromUri]HomeRequest request)
        {
            try
            {
                var data = _Home.GetSupplyrMore(request);
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
