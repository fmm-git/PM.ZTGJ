using PM.Business.RawMaterial;
using PM.Common;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataEntity.RawMaterial.ViewModel;
using PM.DataEntity.System.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.RawMaterial.Controllers
{
    public class StatisticsReportFormController : BaseController
    {
        //
        // 收发存统计报表
        private readonly StatisticsReportFormLogic _reportLogic = new StatisticsReportFormLogic();

        #region 收发存统计报表
        public ActionResult TransceiverReportForm()
        {
            string ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            ViewBag.ProjectId = ProjectId;
            return View();
        }

        /// 原材料名称查询
        public ActionResult MaterialNameSelect()
        {
            var data = _reportLogic.MaterialNameSelect();
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取报表1的数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetReportForm1(StatisticsReportFormRequest request)
        {
            var data = _reportLogic.GetReportForm1(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 列表数据(余料发料量统计表)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetYLDataGridJson(StatisticsReportFormRequest request)
        {
            var data = _reportLogic.GetYLDataList(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetTransceiverGridJson(StatisticsReportFormRequest request)
        {
            var data = _reportLogic.GetTransceiverGridJson(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 月度需求计划明细
        /// </summary>
        /// <returns></returns>
        public ActionResult RawMonthPlan()
        {
            return View();
        }
        /// <summary>
        /// 月度需求计划明细列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetRawPlanGridJson(StatisticsReportFormRequest request)
        {
            var data = _reportLogic.GetRawPlanGridJson(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 批次计划明细
        /// </summary>
        /// <returns></returns>
        public ActionResult BatchPlan()
        {
            return View();
        }
        /// <summary>
        /// 批次计划明细列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetBatchPlanGridJson(StatisticsReportFormRequest request)
        {
            var data = _reportLogic.GetBatchPlanGridJson(request);
            return Content(data.ToJson());
        }
        #endregion

        #region 加工订单统计报表
        public ActionResult WorkOrderReportForm()
        {
            return View();
        }
        public ActionResult WorkOrderWeightTypeFx(string ProjectId)
        {
            var data = _reportLogic.WorkOrderWeightTypeFx(ProjectId);
            return Content(data.ToJson());
        }
        public ActionResult WorkOrderMonthWeightTypeFx(string ProjectId)
        {
            var data = _reportLogic.WorkOrderMonthWeightTypeFx(ProjectId);
            return Content(data.ToJson());
        }
        public ActionResult WorkOrderMonthWeightFx(string ProjectId)
        {
            var data = _reportLogic.WorkOrderMonthWeightFx(ProjectId);
            return Content(data.ToJson());
        }

        public ActionResult WorkOrderAllMonthWeightFx(string ProjectId)
        {
            var data = _reportLogic.WorkOrderAllMonthWeightFx(ProjectId);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取加工订单的所有年份
        /// </summary>
        /// <returns></returns>
        public ActionResult GetWorkYear()
        {
            var data = _reportLogic.GetWorkYear();
            return Content(data.ToJson());
        }
        /// <summary>
        /// 加工厂历史订单量分析
        /// </summary>
        /// <param name="OrgType"></param>
        /// <param name="Year"></param>
        /// <param name="ProjectId"></param>
        /// <returns></returns>
        public ActionResult WorkOrderHistoryFx(string OrgType, string Year, string ProjectId)
        {
            var data = _reportLogic.WorkOrderHistoryFx(OrgType, Year, ProjectId);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 当月订单类型统计
        /// </summary>
        /// <param name="OrgType"></param>
        /// <param name="Year"></param>
        /// <param name="ProjectId"></param>
        /// <returns></returns>
        public ActionResult SameMonthWorkOrderTypeTj(string OrgType, string Year, string Month, string ProjectId)
        {
            var data = _reportLogic.SameMonthWorkOrderTypeTj(OrgType, Year, Month, ProjectId);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 订单类型分布情况
        /// </summary>
        /// <param name="OrgType"></param>
        /// <param name="Year"></param>
        /// <param name="ProjectId"></param>
        /// <returns></returns>
        public ActionResult UrgentWorkOrderFbQk(string OrgType, string Year, string Month, string ProjectId, string JgcCode, string TypeName)
        {
            var data = _reportLogic.UrgentWorkOrderFbQk(OrgType, Year, Month, ProjectId, JgcCode, TypeName);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 订单领料类型占比
        /// </summary>
        /// <param name="OrgType"></param>
        /// <param name="Year"></param>
        /// <param name="ProjectId"></param>
        /// <returns></returns>
        public ActionResult MaterialsUsedTypeTj(string OrgType, string Year, string Month, string ProjectId)
        {
            var data = _reportLogic.MaterialsUsedTypeTj(OrgType, Year, Month, ProjectId);
            return Content(data.ToJson());
        }
        #endregion

        #region 导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="ProjectCode"></param>
        /// <returns></returns>
        public ActionResult OutputExcel(string jsonData)
        {
            var request = JsonEx.JsonToObj<StatisticsReportFormRequest>(jsonData);
            var data = _reportLogic.GetExportList(request);
            decimal hj = 0;
            if (data.Rows.Count > 0)
            {
                hj = Convert.ToDecimal(data.Compute("sum(HistoryMonthCount)", "true"));
            }
            string hzzfc = "月加工总量(kg):" + hj;
            MemoryStream fileStream = new MemoryStream();
            if (request.level == "3")
            {
                //原材料编号,原材料名称,规格,分部,工区,月加工完成量
                //MaterialCode,MaterialName,SpecificationModel,BranchName,WorkAreaName,HistoryMonthCount
                //导出数据列
                Dictionary<string, string> cellheader = new Dictionary<string, string> { 
                    { "MaterialCode", "原材料编号" }, 
                    { "MaterialName", "原材料名称" }, 
                    { "SpecificationModel", "规格" }, 
                    { "BranchName", "分部" }, 
                    { "WorkAreaName", "工区" }, 
                    { "HistoryMonthCount", "月加工完成量" }, 
                };
                fileStream = ExcelHelper.ExportToMemoryStream(cellheader, data, "", "", "车辆运输卸货统计", hzzfc);
            }
            else
            {
                //原材料编号,原材料名称,规格,分部,工区,原材料实时库存,计划数量,批次计划数量,供货量,月加工完成量
                //MaterialCode,MaterialName,SpecificationModel,BranchName,WorkAreaName,InitialCount,LjDemandNum,LjBatchPlanQuantity,LjHasSupplier,HistoryMonthCount
                //导出数据列
                Dictionary<string, string> cellheader = new Dictionary<string, string> { 
                    { "MaterialCode", "原材料编号" }, 
                    { "MaterialName", "原材料名称" }, 
                    { "SpecificationModel", "规格" }, 
                    { "BranchName", "分部" }, 
                    { "WorkAreaName", "工区" }, 
                    { "InitialCount", "原材料实时库存" }, 
                    { "LjDemandNum", "计划数量" }, 
                    { "LjBatchPlanQuantity", "批次计划数量" }, 
                    { "LjHasSupplier", "供货量" }, 
                    { "HistoryMonthCount", "月加工完成量" }, 
                };
                fileStream = ExcelHelper.ExportToMemoryStream(cellheader, data, "", "", "收发存统计报表", hzzfc);

            }


            return File(fileStream, "application/vnd.ms-excel", "收发存统计报表.xls");


        }
        #endregion
    }
}
