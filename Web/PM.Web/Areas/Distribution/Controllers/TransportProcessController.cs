using PM.Business.Distribution;
using PM.Common;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataEntity;
using PM.Web.Models.ExcelModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.Distribution.Controllers
{
    /// <summary>
    /// 运输过程
    /// </summary>
    [HandlerLogin]
    public class TransportProcessController : BaseController
    {
        TransportFlowLogic _transportFlow = new TransportFlowLogic();
        TransportProcessLogic _transportProcess = new TransportProcessLogic();

        #region 运输过程

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetGridJson(TransportProcessRequest request)
        {
            bool IsDriver = OperatorProvider.Provider.CurrentUser.IsDriver;
            string UserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            var data = _transportProcess.GetDataListForPage(request, IsDriver, UserCode);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取车辆配送路线信息
        /// </summary>
        /// <param name="distributionCode">配送装车单号</param>
        /// <returns></returns>
        public ActionResult GetTransportLine(string distributionCode)
        {
            var data = _transportProcess.GetTransportLine(distributionCode);
            return Content(data.ToJson());
        }

        #endregion

        #region 车辆运输卸货统计

        public ActionResult CarReport()
        {
            return View();
        }

        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetCarReportGridJson(TransportProcessRequest request)
        {
            var data = _transportFlow.GetDataListForPage(request);
            return Content(data.ToJson());
        }

        #endregion

        #region 订单节点中配送完成页面

        public ActionResult OrderJdIndex()
        {
            return View();
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
            List<ExcelHead> cellheader = new List<ExcelHead>();
            cellheader.Add(new ExcelHead("SiteName", "卸货站点"));
            cellheader.Add(new ExcelHead("OrderCode", "订单编号"));
            cellheader.Add(new ExcelHead("DistributionCode", "装车配送编号"));
            cellheader.Add(new ExcelHead("FlowStateShow", "卸货过程状态"));
            cellheader.Add(new ExcelHead("TypeCode", "类型编号"));
            cellheader.Add(new ExcelHead("TypeName", "类型名称"));
            cellheader.Add(new ExcelHead("PlanDistributionTime", "计划配送时间"));
            cellheader.Add(new ExcelHead("LoadCompleteTime", "实际配送时间", 20, true));
            cellheader.Add(new ExcelHead("CarCph", "车牌号"));
            cellheader.Add(new ExcelHead("OutSpaceTime", "出场时间", 20, true));
            cellheader.Add(new ExcelHead("EnterSpaceTime", "运输到场时间", 20, true));
            cellheader.Add(new ExcelHead("WaitTime", "等待卸货时间"));
            cellheader.Add(new ExcelHead("StartDischargeTime", "开始卸货时间",20,true));
            cellheader.Add(new ExcelHead("EndDischargeTime", "卸货完成时间", 20, true));
            cellheader.Add(new ExcelHead("LeaveFactoryTime", "车辆出厂时间", 20, true));
            var request = JsonEx.JsonToObj<TransportProcessRequest>(jsonData);
            var data = _transportFlow.GetExportList(request);
            var fileStream = ExcelHelper.ExportToMemoryStream(cellheader, data, "车辆运输卸货统计","");
            return File(fileStream, "application/vnd.ms-excel", "车辆运输卸货统计.xls");
        }
        #endregion
    }
}
