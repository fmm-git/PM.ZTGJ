using PM.Business.RawMaterial;
using PM.Common;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataEntity;
using PM.DataEntity.Production.ViewModel;
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
    /// 原材料生产领料
    /// </summary>
    [HandlerLogin]
    public class RMProductionMaterialController : BaseController
    {
        private readonly RMProductionMaterialMark _RM = new RMProductionMaterialMark();
        private readonly RawMaterialStockRecordLogic _rawMaterialStock = new RawMaterialStockRecordLogic();

        #region 列表

        public ActionResult Index()
        {
            ViewBag.UserCode = base.UserCode;
            return View();
        }
        public ActionResult RMIndexSelect(TbRMProductionMaterialRequest request)
        {
            var data = _RM.GetDataListForPage(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 原材料生产领料数据列表页-饼状图数据查询
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult PieSelect(TbRMProductionMaterialRequest request)
        {
            var data = _RM.PieSelect(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 各站点领料量分析
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult SitaDataReport(TbRMProductionMaterialRequest request)
        {
            var data = _RM.SitaDataReport(request);
            return Content(data.ToJson());
        }

        #endregion

        #region 编辑

        /// <summary>
        /// 原材料生产领料添加/修改界面
        /// </summary>
        /// <param name="type"></param>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult Form(string type, string keyValue)
        {
            ViewBag.InsertUserCode = base.UserCode;
            ViewBag.InsertUserName = base.UserName;
            ViewBag.keyValue = keyValue;
            if (type == "add")
            {
                ViewBag.CollarCode = CreateCode.GetTableMaxCode("RM", "CollarCode", "TbRMProductionMaterial");
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

        [HandlerLogin(Ignore = false)]
        public ActionResult GetFormJson(int keyValue)
        {
            var data = _RM.GetFormJson(keyValue);
            return Content(data.ToJson());
        }
        public ActionResult TypeNameSelect(RMProductionMaterialRequest request)
        {
            var data = _RM.TypeNameSelect(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 查询对应的加工订单明细信息
        /// </summary>
        /// <returns></returns>
        public ActionResult RMProductionMDetailSelect(RMProductionMaterialRequest request)
        {
            var data = _RM.RMProductionMDetailSelect(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 查询对应的加工订单明细信息(All)
        /// </summary>
        /// <returns></returns>
        public ActionResult RMProductionMDetailSelectAll(RMProductionMaterialRequest request)
        {
            var data = _RM.RMProductionMDetailSelectAll(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteForm(int keyValue)
        {
            var data = _RM.DeleteForm(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult AnyInfo(int keyValue)
        {
            var data = _RM.AnyInfo(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitForm(string model, string itemModel, string itemModelBack, string type)
        {
            try
            {
                var rMProductionMaterialModel = JsonEx.JsonToObj<TbRMProductionMaterial>(model);
                var item = JsonEx.JsonToObj<List<TbRMProductionMaterialDetail>>(itemModel);
                var itemBack = JsonEx.JsonToObj<List<RMProductionMaterialItmeBackRequest>>(itemModelBack);
                if (string.IsNullOrEmpty(rMProductionMaterialModel.ProjectId))
                    return Content(AjaxResult.Warning("项目Id不能为空").ToJson());
                if (type == "add")
                {
                    var data = _RM.Insert(rMProductionMaterialModel, item, itemBack);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _RM.Update(rMProductionMaterialModel, item, itemBack);
                    return Content(data.ToJson());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region 打印

        public ActionResult Print(int keyValue)
        {
            var data = _RM.GetPrinJson(keyValue);
            return View(data);
        }

        #endregion

        #region 推荐方案

        public ActionResult Plan()
        {
            return View();
        }

        #endregion

        #region 数据查询（弹框）

        /// <summary>
        /// 领用人信息查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult InsertUserNameSelect(RMProductionMaterialRequest request)
        {
            var data = _RM.InsertUserNameSelect(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取数据列表(厂家，炉批号，质检报告)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetTestReportList(InOrderRequest request)
        {
            var data = _RM.GetTestReportList(request);
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
            var request = JsonEx.JsonToObj<TbRMProductionMaterialRequest>(jsonData);
            var data = _RM.GetExportList(request);
            decimal zl = 0;
            decimal lyzl = 0;
            if (data.Rows.Count > 0)
            {
                zl = Convert.ToDecimal(data.Compute("sum(WeightSum)" , "true"));
                lyzl = Convert.ToDecimal(data.Compute("sum(Total)", "true"));
            }
            string hzzfc = "总量合计(KG):" + zl + ",领用总量合计(KG):" + lyzl;
            List<RMProductionMaterialExcel> dataList = ModelConvertHelper<RMProductionMaterialExcel>.ToList(data);
            var fileStream = ExcelHelper.EntityListToExcelStream<RMProductionMaterialExcel>(dataList, "原材料生产领料", hzzfc);
            return File(fileStream, "application/vnd.ms-excel", "原材料生产领料.xls");
        }
        #endregion
    }
}
