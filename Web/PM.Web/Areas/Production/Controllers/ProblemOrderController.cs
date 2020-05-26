using PM.Business.Production;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataEntity;
using PM.DataEntity.Production.ViewModel;
using PM.Domain.WebBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.Production.Controllers
{
    /// <summary>
    /// 订单变更
    /// </summary>
    [HandlerLogin]
    public class ProblemOrderController : BaseController
    {

        private readonly ProblemOrderLogic _problemOrder = new ProblemOrderLogic();

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
        public ActionResult GetGridJson(ProblemOrderRequest request)
        {
            var data = _problemOrder.GetDataListForPage(request);
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
                ViewBag.ProblemOrderCode = CreateCode.GetTableMaxCode("POC", "ProblemOrderCode", "TbProblemOrder");
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
            var data = _problemOrder.FindEntity(keyValue);
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
                var inOrderModel = JsonEx.JsonToObj<TbProblemOrder>(model);
                var inOrderItem = JsonEx.JsonToObj<List<TbProblemOrderItem>>(itemModel);
                if (type == "add")
                {
                    var data = _problemOrder.Insert(inOrderModel, inOrderItem);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _problemOrder.Update(inOrderModel, inOrderItem);
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
            var data = _problemOrder.Delete(keyValue);
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
            var data = _problemOrder.AnyInfo(keyValue);
            return Content(data.ToJson());
        }

        #endregion

        /// <summary>
        /// 获取数据列表(原订单)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetOrderGridJson(ProblemOrderRequest request)
        {
            var data = _problemOrder.GetOrderDataList(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取数据列表(原订单明细)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetOrderItemGridJson(ProblemOrderRequest request)
        {
            var data = _problemOrder.GetOrderItemDataList(request);
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
            List<ExcelHead> cellheader = new List<ExcelHead>();
            cellheader.Add(new ExcelHead("ProblemOrderCode", "订单变更编号"));
            cellheader.Add(new ExcelHead("OrderCode", "原订单编号"));
            cellheader.Add(new ExcelHead("SiteName", "站点名称"));
            cellheader.Add(new ExcelHead("TypeCode", "类型编号"));
            cellheader.Add(new ExcelHead("TypeName", "类型名称"));
            cellheader.Add(new ExcelHead("UsePart", "使用部位"));
            cellheader.Add(new ExcelHead("RevokeStatus", "撤销状态"));
            cellheader.Add(new ExcelHead("DistributionTime", "配送时间"));
            cellheader.Add(new ExcelHead("DistributionAddress", "配送地址"));
            cellheader.Add(new ExcelHead("OldTotal", "原总量合计（kg）"));
            cellheader.Add(new ExcelHead("Total", "变更总量合计（kg）",30));
            var request = JsonEx.JsonToObj<ProblemOrderRequest>(jsonData);
            request.IsOutPut = true;
            var ret = _problemOrder.GetDataListForPage(request);
            var data = (DataTable)ret.rows;
            decimal hj = 0;
            if (data.Rows.Count > 0)
            {
                hj = Convert.ToDecimal(data.Compute("sum(Total)", "true"));
            }
            string hzzfc = "撤销重量合计(KG):" + hj;
            var fileStream = ExcelHelper.ExportToMemoryStream(cellheader, data, "订单变更", hzzfc);
            return File(fileStream, "application/vnd.ms-excel", "订单变更.xls");
        }
        #endregion
    }
}
