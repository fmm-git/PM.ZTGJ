using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PM.DataEntity.Distribution;
using PM.DataEntity.Distribution.ViewModel;
using PM.DataEntity;
using PM.Business.Distribution;
using PM.Common;
using PM.Common.Extension;
using System.Data;
using PM.Common.Helper;
using PM.Web.Models.ExcelModel;

namespace PM.Web.Areas.Distribution.Controllers
{
    [HandlerLogin]
    public class SemiFinishedSignController : Controller
    {
        //半成品签收 数据来源=站点卸货
        // GET: /Distribution/SemiFinishedSign/

        private readonly SemiFinishedSignLogic _wasRFLogic = new SemiFinishedSignLogic();

        #region 视图
        public ActionResult Index()
        {
            return View();
        }

        [HandlerLogin(Ignore = false)]
        public ActionResult Details()
        {
            return View();
        }
        #endregion

        #region 获取数据 查询
        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetGridJson(SemiFinishedSignRequest request)
        {
            var data = _wasRFLogic.GetDataListForPage(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 编辑/查看页获取数据
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>

        [HandlerLogin(Ignore = false)]
        public ActionResult GetFormJson(int keyValue)
        {
            var data = _wasRFLogic.FindEntity(keyValue);
            return Content(data.ToJson());
        }
        #endregion

        #region （编辑）数据
        /// <summary>
        /// 确认签收
        /// </summary>
        /// <returns></returns>
        public ActionResult SubmitForm(int keyValue)
        {
            try
            {
                var data = _wasRFLogic.Update(keyValue);
                return Content(data.ToJson());
            }
            catch (Exception ex)
            {
                return Content(ex.ToString()); ;
            }
        }

        #endregion

        #region  加工订单签收
        /// <summary>
        /// 签收
        /// </summary>
        /// <returns></returns>
        public ActionResult SemiFinishedSignNew(string OrderCode)
        {
            try
            {
                var data = _wasRFLogic.SemiFinishedSignNew(OrderCode);
                return Content(data.ToJson());
            }
            catch (Exception ex)
            {
                return Content(ex.ToString()); ;
            }
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
            var request = JsonEx.JsonToObj<SemiFinishedSignRequest>(jsonData);
            var data = _wasRFLogic.GetExportList(request);
            decimal zlhj = 0;
            if (data.Rows.Count > 0)
            {
                zlhj = Convert.ToDecimal(data.Compute("sum(SumTotal)", "true"));
            }
            string hzzfc = "合计(KG):" + zlhj;
            List<SemiFinishedSignExcel> dataList = ModelConvertHelper<SemiFinishedSignExcel>.ToList(data);
            var fileStream = ExcelHelper.EntityListToExcelStream<SemiFinishedSignExcel>(dataList, "半成品签收", hzzfc);
            return File(fileStream, "application/vnd.ms-excel", "半成品签收.xls");
        }

        #endregion

    }
}
