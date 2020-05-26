using PM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net.Http;
using PM.Business.RawMaterial;
using PM.DataEntity.Production.ViewModel;
using PM.DataEntity;

namespace PM.Web.WebApi.PlanManage
{
    public class RawMonthDemandPlanController : ApiController
    {

        private readonly TbRawMaterialMonthDemandPlanLogic _rawPlan = new TbRawMaterialMonthDemandPlanLogic();
        private readonly FactoryBatchNeedPlanLogic _fbnPlan = new FactoryBatchNeedPlanLogic();
        private readonly InOrderLogic _inOrder = new InOrderLogic();
        private readonly SampleOrderLogic _spOrder = new SampleOrderLogic();

        #region 原材料月度需求计划

        #region 统计信息

        [HttpGet]
        /// <summary>
        /// 获取钢筋类型
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage GetRebarType()
        {
            var data = _rawPlan.GetRebarType();
            return AjaxResult.Success(data).ToJsonApi();
        }

        [HttpGet]
        /// <summary>
        /// 获取原材料月度计划完成情况
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage Img1([FromUri]RawMonthDemPlanRequest pt)
        {
            var data = _rawPlan.Img1New(pt);
            return AjaxResult.Success(data).ToJsonApi();
        }

        [HttpGet]
        /// <summary>
        /// 工区材料月度计划提交情况
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage Img2([FromUri]RawMonthDemPlanRequest pt)
        {
            var data = _rawPlan.Img2(pt);
            return AjaxResult.Success(data).ToJsonApi();
        }
        [HttpGet]
        /// <summary>
        /// 工区材料月度计划提交情况
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage Img3([FromUri]RawMonthDemPlanRequest pt)
        {
            var data = _rawPlan.Img3(pt);
            return AjaxResult.Success(data).ToJsonApi();
        }
        [HttpGet]
        /// <summary>
        /// 工区材料月度计划提交情况
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage Img4([FromUri]RawMonthDemPlanRequest pt)
        {
            var data = _rawPlan.Img4(pt);
            return AjaxResult.Success(data).ToJsonApi();
        }

        #endregion

        #region 所有条目
        [HttpGet]
        /// <summary>
        /// 获取所有列表信息
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage GetGridJson([FromUri]RawMonthDemPlanRequest pt)
        {
            var data = _rawPlan.GetDataListForPage(pt);
            return AjaxResult.Success(data).ToJsonApi();
        }
        /// <summary>
        /// 获取详细信息
        /// </summary>
        /// <param name="ID">ID</param>
        /// <returns></returns>
        public HttpResponseMessage GetFormJson(int ID)
        {
            var data = _rawPlan.FindEntity(ID);
            return AjaxResult.Success(data).ToJsonApi();
        }
        #endregion

        #endregion

        #region 加工厂批次需求计划

        #region 统计信息

        [HttpGet]
        /// <summary>
        /// 批次需求计划完成情况
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage fbnImg1([FromUri]FPiCiXQPlan pt)
        {
            var data = _fbnPlan.Img1(pt);
            return AjaxResult.Success(data).ToJsonApi();
        }
        [HttpGet]
        /// <summary>
        /// 供货质量问题统计
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage fbnImg3([FromUri]FPiCiXQPlan pt)
        {
            var data = _fbnPlan.Img3(pt);
            return AjaxResult.Success(data).ToJsonApi();
        }
        #endregion

        #region 所有条目

        [HttpGet]
        /// <summary>
        /// 获取所有列表信息
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage fbnGetGridJson([FromUri]FPiCiXQPlan pt)
        {
            var data = _fbnPlan.GetAllOrBySearch(pt);
            return AjaxResult.Success(data).ToJsonApi();
        }
        [HttpGet]
        /// <summary>
        /// 获取供货列表详细信息
        /// </summary>
        /// <param name="ID">ID</param>
        /// <returns></returns>
        public HttpResponseMessage fbnGetFormDetailJson(int ID)
        {
            var data = _fbnPlan.AppDetail(ID);
            return AjaxResult.Success(data).ToJsonApi();
        }
        [HttpGet]
        /// <summary>
        /// 获取供货历史列表数据
        /// </summary>
        /// <param name="ID">ID</param>
        /// <returns></returns>
        public HttpResponseMessage fbnGetFormDetailItemJson(int BatchPlanItemId)
        {
            var data = _fbnPlan.AppDetailItem(BatchPlanItemId);
            return AjaxResult.Success(data).ToJsonApi();
        }
        #endregion

        #endregion

        #region 原材料到货入库


        [HttpGet]
        /// <summary>
        /// 获取所有列表信息
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage inoImg1([FromUri]InOrderRequest pt)
        {
            var data = _inOrder.Img1(pt);
            return AjaxResult.Success(data).ToJsonApi();
        }
        [HttpGet]
        /// <summary>
        /// 获取所有列表信息
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage inoImg2([FromUri]InOrderRequest pt)
        {
            var data = _inOrder.Img2(pt);
            return AjaxResult.Success(data).ToJsonApi();
        }

        [HttpGet]
        /// <summary>
        /// 获取所有列表信息
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage inoGetGridJson([FromUri]InOrderRequest pt)
        {
            var data = _inOrder.GetDataListForPage(pt);
            return AjaxResult.Success(data).ToJsonApi();
        }
        /// <summary>
        /// 获取详细信息
        /// </summary>
        /// <param name="ID">ID</param>
        /// <returns></returns>
        public HttpResponseMessage inoGetFormJson(int ID)
        {
            var data = _inOrder.FindEntity(ID);
            return AjaxResult.Success(data).ToJsonApi();
        }
        #endregion

        #region 原材料取样订单

        #region 统计消息

        [HttpGet]
        /// <summary>
        /// 图形提取数据
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage spoImg1([FromUri]SampleOrderRequest pt)
        
        {
            var data = _spOrder.Img1(pt);
            return AjaxResult.Success(data).ToJsonApi();
        }

        [HttpGet]
        /// <summary>
        /// 原材料取样订单统计
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage spoImg2([FromUri]SampleOrderRequest pt)
        {
            var data = _spOrder.Img2(pt);
            return AjaxResult.Success(data).ToJsonApi();
        }

        [HttpGet]
        /// <summary>
        /// 取样订单检测结果统计
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage spoImg3([FromUri]SampleOrderRequest pt)
        {
            var data = _spOrder.Img3(pt);
            return AjaxResult.Success(data).ToJsonApi();
        } 

        #endregion

        #region 所有条目

        [HttpGet]
        /// <summary>
        /// 获取所有列表信息
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage spoGetGridJson([FromUri]SampleOrderRequest pt)
        {
            var data = _spOrder.GetDataListForPage(pt);
            return AjaxResult.Success(data).ToJsonApi();
        }
        /// <summary>
        /// 获取详细信息
        /// </summary>
        /// <param name="ID">ID</param>
        /// <returns></returns>
        public HttpResponseMessage spoGetFormJson(int ID)
        {
            var data = _spOrder.FindEntity(ID);
            return AjaxResult.Success(data).ToJsonApi();
        }
        #endregion

        #endregion
    }
}
