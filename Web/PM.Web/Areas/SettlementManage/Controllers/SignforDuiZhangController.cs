using PM.Business.SettlementManage;
using PM.Common;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataEntity;
using PM.DataEntity.System.ViewModel;
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
    /// 签收对账单
    /// </summary>
    [HandlerLogin]
    public class SignforDuiZhangController : BaseController
    {
        private readonly SignforDuiZhangLogic _sdzLogic = new SignforDuiZhangLogic();
        //
        // GET: /SettlementManage/SignforDuiZhang/
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
                ViewBag.UserName = base.UserName;
                ViewBag.UserCode = base.UserCode;
                ViewBag.SigninNuber = CreateCode.GetTableMaxCode("QSDZ", "SigninNuber", "TbSignforDuiZhang");
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
        /// 查询页
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
        public ActionResult GetAllSignforDuiZhang(FPiCiXQPlan ent)
        {
            var data = _sdzLogic.GetAllSignforDuiZhang(ent);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 以ID查询签收对账
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult GetFormJson(int keyValue)
        {
            var data = _sdzLogic.GetFormJson(keyValue);
            return Content(data.ToJson());
        }

        #region 获取所有站点

        public ActionResult GetCompanyCompanyAllSiteList(TbCompanyRequest request,string orgType, string parentCode)
        {
            var data = _sdzLogic.GetCompanyCompanyAllSiteList(request,orgType, parentCode);
            return Content(data.ToJson());
        }

        #endregion

        /// <summary>
        /// 配送签收单弹窗（系统新增）
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPSList(FPiCiXQPlan entity, string keyword)
        {
            var data = _sdzLogic.GetAllQianShou(entity, keyword);
            return Content(data.ToJson());
        }


        /// <summary>
        /// 配送签收单弹窗（手动新增）
        /// </summary>
        /// <returns></returns>
        public ActionResult GetJgWcWpsList(FPiCiXQPlan entity, string keyword, string SigninNuber, string DzOrderCode)
        {
            var data = _sdzLogic.GetJgWcWpsList(entity, keyword, SigninNuber, DzOrderCode);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult AnyInfo(int keyValue)
        {
            var data = _sdzLogic.AnyInfo(keyValue);
            return Content(data.ToJson());
        }

        ///// <summary>
        ///// 各站点发料数量分析
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult GetSiteAnalysis(PageSearchRequest psr) 
        //{
        //    var data = _sdzLogic.GetSiteAnalysis(psr);
        //    return Content(data.ToJson());
        //}

        ///// <summary>
        ///// 各加工类型发料数量分析
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult GetMachiningTypeAnalysis(PageSearchRequest psr)
        //{
        //    var data = _sdzLogic.GetMachiningTypeAnalysis(psr);
        //    return Content(data.ToJson());
        //}

        #endregion

        #region 新增/编辑数据

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
                var SOModel = JsonEx.JsonToObj<TbSignforDuiZhang>(model);
                var SOModelItem = JsonEx.JsonToObj<List<TbSignforDuiZhangDetail>>(itemModel);
                if (type == "add")
                {
                    var data = _sdzLogic.Insert(SOModel, SOModelItem);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _sdzLogic.Update(SOModel, SOModelItem);
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
            var data = _sdzLogic.Delete(keyValue);
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
            //对账编号、所属加工厂、站点、对账时间、总量（T）、单位（T）、开始日期、结束日期、录入人、录入时间
            //SigninNuber,ProcessFactoryName,SiteName,SignforTime,HavingAmount,Unit,StartDate,EndDate,UserName,InsertTime
            Dictionary<string, string> cellheader = new Dictionary<string, string> { 
                    { "SigninNuber", "对账编号" }, 
                    { "ProcessFactoryName", "所属加工厂" }, 
                    { "SiteName", "站点" }, 
                    { "SignforTime", "对账时间" }, 
                    { "HavingAmount", "总量（T）" }, 
                    { "Unit", "单位（T）" },
                    { "StartDate", "开始日期" },
                    { "EndDate", "结束日期" },
                    { "UserName", "录入人" },
                    { "InsertTime", "录入时间" },
                };
            var request = JsonEx.JsonToObj<FPiCiXQPlan>(jsonData);
            var data = _sdzLogic.GetExportList(request);
            decimal zl = 0;
            if (data.Rows.Count>0)
            {
                zl = Convert.ToDecimal(data.Compute("sum(HavingAmount)", "true"));
            }
            string hzzfc = "合计(T):" + zl;
            var fileStream = ExcelHelper.ExportToMemoryStream(cellheader, data, "", "", "签收对账单", hzzfc);
            return File(fileStream, "application/vnd.ms-excel", "签收对账单.xls");
        }
        #endregion
    }
}
