using PM.Business.CostManage;
using PM.Common;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataEntity;
using PM.DataEntity.CostManage.ViewModel;
using PM.Domain.WebBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.CostManage.Controllers
{
    public class MonthCostHeSuanController : BaseController
    {
        // 月成本核算
        private readonly TbMonthCostHeSuanLogic _monthCostHeSuanLogic = new TbMonthCostHeSuanLogic();

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
        public ActionResult GetGridJson(MonthCostHeSuanRequest request)
        {
            var data = _monthCostHeSuanLogic.GetDataListForPage(request);
            return Content(data.ToJson());
        }

        #endregion

        #region 新增、编辑、查看

        public ActionResult Form(string type)
        {
            if (type == "add")
            {
                ViewBag.HeSuanCode = CreateCode.GetTableMaxCode("MCHS", "HeSuanCode", "TbMonthCostHeSuan");
                ViewBag.UserName = base.UserName;
                ViewBag.OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
                ViewBag.CompanyId = OperatorProvider.Provider.CurrentUser.CompanyId;
                if (ViewBag.OrgType == "1")
                {
                    ViewBag.ProcessFactoryCode = OperatorProvider.Provider.CurrentUser.CompanyId;
                    ViewBag.ProcessFactoryName = OperatorProvider.Provider.CurrentUser.ComPanyName;
                }
            }
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
            var data = _monthCostHeSuanLogic.FindEntity(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="model">主表信息</param>
        /// <param name="itemModel">明细信息</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public ActionResult SubmitForm(string model, string incomeModel, string costModel, string otherCostModel, string type)
        {
            try
            {
                var monthCostHeSuan = JsonEx.JsonToObj<TbMonthCostHeSuan>(model);
                var monthCostHeSuanIncome = JsonEx.JsonToObj <List<TbMonthCostHeSuanIncome>>(incomeModel);
                var monthCostHeSuanCost = JsonEx.JsonToObj<List<TbMonthCostHeSuanCost>>(costModel);
                var monthCostHeSuanOtherCost = JsonEx.JsonToObj<List<TbMonthCostHeSuanOtherCost>>(otherCostModel);
                if (type == "add")
                {
                    var data = _monthCostHeSuanLogic.Insert(monthCostHeSuan, monthCostHeSuanIncome[0], monthCostHeSuanCost, monthCostHeSuanOtherCost);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _monthCostHeSuanLogic.Update(monthCostHeSuan, monthCostHeSuanIncome[0], monthCostHeSuanCost, monthCostHeSuanOtherCost);
                    return Content(data.ToJson());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 新增、修改其他成本信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="addType"></param>
        /// <returns></returns>
        public ActionResult FormOtherCost(string type,string addType)
        {
            return View();
        }
        /// <summary>
        /// 查看其他成本信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="addType"></param>
        /// <returns></returns>
        public ActionResult DetailsOtherCost(string type, string addType)
        {
            return View();
        }
        public ActionResult GetOtherCostList(string HeSuanCode, int? addType)
        {
            var data = _monthCostHeSuanLogic.GetOtherCostList(HeSuanCode, addType);
            return Content(data.ToJson());
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
            var data = _monthCostHeSuanLogic.Delete(keyValue);
            return Content(data.ToJson());
        }

        #endregion

        /// <summary>
        /// 获取成本项目
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetCostDataList()
        {
            var data = _monthCostHeSuanLogic.GetCostDataList();
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
            //核算编号、所属加工厂、核算月份、加工总量（T）、合计成本（元）、平均成本（元）
            //HeSuanCode,ProcessFactoryName,HeSuanMonth,MachiningTotal,TotalCost,AvgCost
            Dictionary<string, string> cellheader = new Dictionary<string, string> { 
                    { "HeSuanCode", "核算编号" }, 
                    { "ProcessFactoryName", "所属加工厂" }, 
                    { "HeSuanMonth", "核算月份" }, 
                    { "MachiningTotal", "加工总量（T）" }, 
                    { "TotalCost", "合计成本（元）" }, 
                    { "AvgCost", "平均成本（元）" },
                };
            var request = JsonEx.JsonToObj<MonthCostHeSuanRequest>(jsonData);
            var data = _monthCostHeSuanLogic.GetExportList(request);
            var fileStream = ExcelHelper.ExportToMemoryStream(cellheader, data, "", "", "月成本核算", "");
            return File(fileStream, "application/vnd.ms-excel", "月成本核算.xls");
        }
        #endregion
    }
}
