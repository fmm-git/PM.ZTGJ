using PM.Business.Production;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataEntity;
using PM.DataEntity.Production.ViewModel;
using PM.Domain.WebBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.Production.Controllers
{
    [HandlerLogin]
    public class OrderProgressController : Controller
    {
        //订单进度 数据来源 加工订单
        // GET: /Production/OrderPregress/
        private readonly OrderProgressLogic _orderProLogic = new OrderProgressLogic();

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
        public ActionResult GetGridJson(OrderProgressRequest request)
        {
            var data = _orderProLogic.GetDataListForPage(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 加载报表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetProgressForm(OrderProgressRequest request)
        {
            var data = _orderProLogic.GetProgressForm(request);
            return Content(data.ToJson());
        }
        #endregion

        #region 新增、修改、查看
        public ActionResult Form(string type)
        {
            return View();
        }

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
            var data = _orderProLogic.FindEntity(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="model">主表信息</param>
        /// <param name="itemModel">明细信息</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public ActionResult SubmitForm(string model, string itemModel, string type)
        {
            try
            {

                var OrderProgressModel = JsonEx.JsonToObj<TbOrderProgress>(model);
                var OrderProDetailModel = JsonEx.JsonToObj<List<TbOrderProgressDetail>>(itemModel);
                var data = _orderProLogic.Update(OrderProgressModel, OrderProDetailModel);

                return Content(data.ToJson());
            }
            catch (Exception ex)
            {
                return Content(ex.ToString()); ;
            }
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
            var data = _orderProLogic.AnyInfo(keyValue);
            return Content(data.ToJson());
        }

        #endregion

        #region 一键填报
        public ActionResult SubmitOneKeyForm(int Id)
        {
            try
            {
                var data = _orderProLogic.UpdateOneKey(Id);
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
            List<ExcelHead> cellheader = new List<ExcelHead>();
            cellheader.Add(new ExcelHead("SiteName", "站点名称"));
            cellheader.Add(new ExcelHead("OrderCode", "订单编号"));
            cellheader.Add(new ExcelHead("OrderStart", "订单类型"));
            cellheader.Add(new ExcelHead("WeightTotal", "总量合计(kg)"));
            cellheader.Add(new ExcelHead("AccumulativeQuantity", "开累加工量（kg）"));
            cellheader.Add(new ExcelHead("TypeCode", "类型编号"));
            cellheader.Add(new ExcelHead("TypeName", "类型名称"));
            cellheader.Add(new ExcelHead("UsePart", "使用部位"));
            cellheader.Add(new ExcelHead("DistributionTime", "计划配送时间"));
            cellheader.Add(new ExcelHead("ProcessFactoryName", "加工厂名称"));
            var request = JsonEx.JsonToObj<OrderProgressRequest>(jsonData);
            var data = _orderProLogic.GetExportList(request);
            decimal zlhj = 0;
            decimal kljglhj = 0;
            if (data.Rows.Count > 0)
            {
                zlhj = Convert.ToDecimal(data.Compute("sum(WeightTotal)", "true"));
                kljglhj = Convert.ToDecimal(data.Compute("sum(AccumulativeQuantity)", "true"));
            }
            string hzzfc = "总量合计(KG):" + zlhj + ",开累加工量合计(KG):"+kljglhj;
            var fileStream = ExcelHelper.ExportToMemoryStream(cellheader, data, "订单进度", hzzfc);
            return File(fileStream, "application/vnd.ms-excel", "订单进度.xls");
        }

        #endregion

    }
}
