using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PM.Business.RawMaterial;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataEntity;
using PM.Domain.WebBase;
using PM.Web.Models.ExcelModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.RawMaterial.Controllers
{
    /// <summary>
    /// 原材料取样订单
    /// </summary>
    [HandlerLogin]
    public class SampleOrderController : BaseController
    {
        private readonly SampleOrderLogic _sampleOrder = new SampleOrderLogic();

        #region 列表

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetGridJson(SampleOrderRequest request)
        {
            var data = _sampleOrder.GetDataListForPage(request);
            return Content(data.ToJson());
        }

        #endregion

        #region 编辑

        /// <summary>
        /// 新增、修改页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Form(string type)
        {
            if (type == "add")
            {
                ViewBag.UserName = base.UserName;
                ViewBag.SampleOrderCode = CreateCode.GetTableMaxCode("QY", "SampleOrderCode", "TbSampleOrder");
            }
            return View();
        }

        /// <summary>
        /// 查看页面
        /// </summary>
        /// <returns></returns>
        [HandlerLogin(Ignore = false)]
        public ActionResult Details()
        {
            return View();
        }

        /// <summary>
        /// 编辑/查看页获取数据
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>

        [HandlerLogin(Ignore = false)]
        public ActionResult GetFormJson(int keyValue)
        {
            var data = _sampleOrder.FindEntity(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitForm(string model, string itemModel, string type)
        {
            try
            {
                var sampleOrderModel = JsonEx.JsonToObj<TbSampleOrder>(model);
                var sampleOrderItem = JsonEx.JsonToObj<List<TbSampleOrderItem>>(itemModel);
                if (type == "add")
                {
                    var data = _sampleOrder.Insert(sampleOrderModel, sampleOrderItem);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _sampleOrder.Update(sampleOrderModel, sampleOrderItem);
                    return Content(data.ToJson());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region 删除

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteForm(int keyValue)
        {
            var data = _sampleOrder.Delete(keyValue);
            return Content(data.ToJson());
        }

        #endregion

        #region 信息验证

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult AnyInfo(int keyValue)
        {
            var data = _sampleOrder.AnyInfo(keyValue);
            return Content(data.ToJson());
        }

        #endregion

        #region 检测报告

        /// <summary>
        /// 检测报告页面
        /// </summary>
        /// <returns></returns>
        public ActionResult TestReport()
        {
            return View();
        }

        /// <summary>
        /// 质检报告数据提交
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitTestReport(string itemModel)
        {
            try
            {
                var sampleOrderItem = JsonEx.JsonToObj<List<TbSampleOrderItem>>(itemModel);
                var data = _sampleOrder.SubmitTestReport(sampleOrderItem);
                return Content(data.ToJson());
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region 加工完成

        [HttpPost]
        public ActionResult ProcessingOver(int keyValue)
        {
            try
            {
                var data = _sampleOrder.ProcessingOver(keyValue);
                return Content(data.ToJson());
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        /// <summary>
        /// 获取数据列表(入库单)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetInOrderGridJson(SampleOrderRequest request)
        {
            var data = _sampleOrder.GetInOrderDataList(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取数据列表(入库单明细)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetInOrderItemGridJson(SampleOrderRequest request)
        {
            var data = _sampleOrder.GetInOrderItemDataList(request);
            return Content(data.ToJson());
        }

        #region 导出

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="ProjectCode"></param>
        /// <returns></returns>
        public ActionResult OutputExcel(string jsonData)
        {
            var request = JsonEx.JsonToObj<SampleOrderRequest>(jsonData);
            request.IsOutPut = true;
            var ret = _sampleOrder.GetDataListForPage(request);
            decimal hj =0;
            var data = (DataTable)ret.rows;
            if (data.Rows.Count > 0)
            {
                hj = Convert.ToDecimal(data.Compute("sum(WeightSum)", "true"));
            }
            string hzzfc = "总量合计:"+hj+"(kg)";
            List<SampleOrderExcel> dataList = ModelConvertHelper<SampleOrderExcel>.ToList(data);
            var fileStream = ExcelHelper.EntityListToExcelStream<SampleOrderExcel>(dataList, "原材料取样订单", hzzfc);
            return File(fileStream, "application/vnd.ms-excel", "原材料取样订单.xls");
        }

        #endregion

    }
}
