using PM.Business.SettlementManage;
using PM.Common;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataEntity;
using PM.Domain.WebBase;
using PM.DomainEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.SettlementManage.Controllers
{
    /// <summary>
    /// 结算单
    /// </summary>
    [HandlerLogin]
    public class SettlementOrderController : BaseController
    {
        /// <summary>
        /// 结算单处理
        /// </summary>
        private readonly SettlementOrderLogic _soLogic = new SettlementOrderLogic();
        //
        // GET: /SettlementManage/SettlementOrder/

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 编辑页
        /// </summary>
        /// <returns></returns>
        public ActionResult Form(string type)
        {
            if (type == "add")
            {
                ViewBag.SettlementCode = CreateCode.GetTableMaxCode("JJD", "SettlementCode", "TbSettlementOrder");
                ViewBag.UserName = base.UserName;
                ViewBag.UserCode = base.UserCode;
            }
            ViewBag.OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
            if (OperatorProvider.Provider.CurrentUser.OrgType == "1")
            {
                ViewBag.ProcessFactoryCode = OperatorProvider.Provider.CurrentUser.ProcessFactoryCode;
                ViewBag.ProcessFactoryName = OperatorProvider.Provider.CurrentUser.ProcessFactoryName;
            }
            return View();
        }
        /// <summary>
        /// 查看页
        /// </summary>
        /// <returns></returns>
        [HandlerLogin(Ignore = false)]
        public ActionResult Details()
        {
            return View();
        }

        #region 查询数据

        /// <summary>
        /// 全部查询/条件查询
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        public ActionResult GetAllSettlement(FPiCiXQPlan ent) 
        {
            var data = _soLogic.GetAllSettlement(ent);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 以ID查询结算
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult GetFormJson(int keyValue)
        {
            var data = _soLogic.GetFormJson(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 新增明细对账弹窗
        /// </summary>
        /// <returns></returns>
        public ActionResult GetBalanceOfAccountOrder(FPiCiXQPlan entity, string keyword) 
        {
            var data = _soLogic.GetBalanceOfAccountOrder(entity, keyword);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult AnyInfo(int keyValue)
        {
            var data = _soLogic.AnyInfo(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 图形数据查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAnalysis(PageSearchRequest psr) 
        {
            var data = _soLogic.GetAnalysis(psr);
            return Content(data.ToJson());
        }

        #endregion

        #region 新增数据

        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="model">主表信息</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public ActionResult SubmitForm(string model, string itemModel, string type)
        {
            try
            {
                var SOModel = JsonEx.JsonToObj<TbSettlementOrder>(model);
                var SOModelItem = JsonEx.JsonToObj<List<TbSettlementOrderDetail>>(itemModel);
                if (type == "add")
                {
                    var data = _soLogic.Insert(SOModel, SOModelItem);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _soLogic.Update(SOModel, SOModelItem);
                    return Content(data.ToJson());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region 删除数据

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteForm(int keyValue)
        {
            var data = _soLogic.Delete(keyValue);
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
            //计价单号、所属加工厂、站点、开始日期、结束日期、总重量（T）、总金额（元）、录入人、录入时间
            //SettlementCode,ProcessFactoryName,SiteName,StartDate,EndDate,TotalWeight,AmountOfMoney,UserName,InsertTime
            Dictionary<string, string> cellheader = new Dictionary<string, string> { 
                    { "SettlementCode", "计价单号" }, 
                    { "ProcessFactoryName", "所属加工厂" }, 
                    { "SiteName", "站点" }, 
                    { "StartDate", "开始日期" }, 
                    { "EndDate", "结束日期" }, 
                    { "TotalWeight", "总重量（T）" },
                    { "AmountOfMoney", "总金额（元）" },
                    { "UserName", "录入人" },
                    { "InsertTime", "录入时间" },
                };
            var request = JsonEx.JsonToObj<FPiCiXQPlan>(jsonData);
            var data = _soLogic.GetExportList(request);
            decimal zlhj = 0;
            decimal zjehj = 0;
            if (data.Rows.Count>0)
            {
                zlhj = Convert.ToDecimal(data.Compute("sum(TotalWeight)", "true"));
                zjehj = Convert.ToDecimal(data.Compute("sum(AmountOfMoney)", "true"));
            }
            string hzzfc = "总重量合计（T）:" + zlhj + "," + "总金额合计（元）:"+zjehj;
            var fileStream = ExcelHelper.ExportToMemoryStream(cellheader, data, "", "", "结算单", hzzfc);
            return File(fileStream, "application/vnd.ms-excel", "结算单.xls");
        }
        #endregion

    }
}
