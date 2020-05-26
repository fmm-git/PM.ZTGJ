using PM.Business.RawMaterial;
using PM.Common;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataEntity;
using PM.Domain.WebBase;
using PM.Web.Models.ExcelModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.RawMaterial.Controllers
{
    /// <summary>
    /// 原材料到货入库
    /// </summary>
    [HandlerLogin]
    public class InOrderController : BaseController
    {
        private readonly InOrderLogic _inOrder = new InOrderLogic();

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
        public ActionResult GetGridJson(InOrderRequest request)
        {
            var data = _inOrder.GetDataListForPage(request);
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
                ViewBag.InOrderCode = CreateCode.GetTableMaxCode("RK", "InOrderCode", "TbInOrder");
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
            var data = _inOrder.FindEntity(keyValue);
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
                var inOrderModel = JsonEx.JsonToObj<TbInOrder>(model);
                var inOrderItem = JsonEx.JsonToObj<List<TbInOrderItem>>(itemModel);
                if (string.IsNullOrEmpty(inOrderModel.SiteCode))
                    inOrderModel.SiteCode = "";
                if (type == "add")
                {
                    var data = _inOrder.Insert(inOrderModel, inOrderItem);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _inOrder.Update(inOrderModel, inOrderItem);
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
            var data = _inOrder.Delete(keyValue);
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
            var data = _inOrder.AnyInfo(keyValue);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 判断信息是否可退回
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult AnyReturn(int keyValue)
        {
            var data = _inOrder.GetReturnDataList(keyValue);
            if (data.Rows.Count > 0)
            {
                return Content(AjaxResult.Success().ToJson());
            }
            else
            {
                return Content(AjaxResult.Warning("所选入库批次中无可退回的‘不合格’材料").ToJson());
            }
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
        public ActionResult SubmitTestReport(string enclosure, string itemModel)
        {
            try
            {
                var inOrderItem = JsonEx.JsonToObj<List<TbInOrderItem>>(itemModel);
                var data = _inOrder.SubmitTestReport(enclosure, inOrderItem);
                return Content(data.ToJson());
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region 退回处理

        /// <summary>
        /// 退回处理页面
        /// </summary>
        /// <returns></returns>
        public ActionResult ReturnView()
        {
            return View();
        }


        /// <summary>
        /// 获取列表数据
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult GetReturnGridJson(int keyValue)
        {
            var data = _inOrder.GetReturnDataList(keyValue);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 退回处理数据提交
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ReturnSubmit(string itemModel)
        {
            try
            {
                var inOrderItem = JsonEx.JsonToObj<List<TbInOrderItem>>(itemModel);
                var data = _inOrder.ReturnSubmit(inOrderItem);
                return Content(data.ToJson());
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        /// <summary>
        /// 获取数据列表(供应商)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetSupplierGridJson(InOrderRequest request)
        {
            var data = _inOrder.GetSupplierDataList(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取数据列表(批次计划)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetBatchPlanGridJson(InOrderRequest request)
        {
            var data = _inOrder.GetBatchPlanDataList(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取数据列表(批次计划明细)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetBatchPlanItemGridJson(InOrderRequest request)
        {
            var data = _inOrder.GetBatchPlanItemDataList(request);
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
            var request = JsonEx.JsonToObj<InOrderRequest>(jsonData);
            var data = _inOrder.GetExportList(request);
            decimal zlhj = 0;
            if (data.Rows.Count > 0)
            {
                zlhj = Convert.ToDecimal(data.Compute("sum(InCount)", "true"));
            }
            string hzzfc = "总量合计(KG):" + zlhj;
            List<InOrderExcel> dataList = ModelConvertHelper<InOrderExcel>.ToList(data);
            var fileStream = ExcelHelper.EntityListToExcelStream<InOrderExcel>(dataList, "原材料到货入库", hzzfc);
            return File(fileStream, "application/vnd.ms-excel", "原材料到货入库.xls");
        }

        #endregion

        #region 获取实验室人员

        public ActionResult GetSysUser(string ProjectId, string BranchCode, string WorkAreaCode)
        {
            var data = _inOrder.GetSysUser(ProjectId, BranchCode, WorkAreaCode);
            return Content(data.ToJson());
        }

        #endregion

        #region 通过入库单号获取入库信息

        public ActionResult GetInOrderData(string InOrderCode)
        {
            var data = _inOrder.GetInOrderData(InOrderCode);
            return Content(data.ToJson());
        }

        #endregion

        #region 原材料到货入库通知
        public ActionResult SendNoticeNews(string InOrderCode)
        {
            var data = _inOrder.SendNoticeNews(InOrderCode);
            return Content(data.ToJson());

        }

        #endregion

    }
}
