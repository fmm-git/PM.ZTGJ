using PM.Business.Distribution;
using PM.Common;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataEntity;
using PM.Domain.WebBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;

namespace PM.Web.Areas.Distribution.Controllers
{
    // 配送管理：配送装车
    [HandlerLogin]
    public class DistributionEntController : BaseController
    {
        //配送装车逻辑处理层类
        private readonly DistributionEntLogic _deBus = new DistributionEntLogic();

        // GET: /Distribution/DistributionEnt/

        #region 视图

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 新增/编辑页
        /// </summary>
        /// <returns></returns>
        public ActionResult Form(string type)
        {
            if (type == "add")
            {
                ViewBag.DistributionCode = CreateCode.GetTableMaxCode("PSZC", "DistributionCode", "TbDistributionEnt");
                ViewBag.UserName = base.UserName;
                ViewBag.UserCode = base.UserCode;
                if (!string.IsNullOrWhiteSpace(OperatorProvider.Provider.CurrentUser.ProcessFactoryCode))
                {
                    ViewBag.ProcessFactoryCode = OperatorProvider.Provider.CurrentUser.ProcessFactoryCode;
                    ViewBag.ProcessFactoryName = OperatorProvider.Provider.CurrentUser.ProcessFactoryName;
                }
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

        public ActionResult Print() 
        {
            return View();
        }
        #endregion

        #region 查询数据

        /// <summary>
        /// 首页 查询全部信息
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult GetAllOrBySearch(FPiCiXQPlan entity)
        {
            var data = _deBus.GetAllOrBySearch(entity);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 以ID查询装车信息
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>

        [HandlerLogin(Ignore = false)]
        public ActionResult GetFormJson(int keyValue, int? keyValue1)
        {
            var data = _deBus.GetFormJson(keyValue, keyValue1);
            data.Item1.Columns.Add(new DataColumn("FHDW"));
            data.Item1.Rows[0]["FHDW"] = OperatorProvider.Provider.CurrentUser.ComPanyName;
            return Content(data.ToJson());
        }

        /// <summary>
        /// 配送计划弹窗
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetPSJHGridJson(DistributionPlanRequest request, string keyword)
        {
            request.keyword = keyword;
            var data = _deBus.GetPSJHGridJson(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 新增明细弹窗
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult GetPSJHItemGridJson(DistributionPlanRequest request, string keyValue, string PSJHCode)
        {
            request.keyword = keyValue;
            request.PSJHCode = PSJHCode;
            var data = _deBus.GetPSJHItemGridJson(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult AnyInfo(int keyValue)
        {
            var data = _deBus.AnyInfo(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 打印增加次数
        /// </summary>
        /// <returns></returns>
        public ActionResult AddCount(int keyValue) 
        {
            var data = _deBus.AddCount(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 根据站点查询站点用户
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetSiteUser(TbUserRequest ur) 
        {
            var data = _deBus.GetSiteUser(ur);
            return Content(data.ToJson());
        }

        #endregion

        #region （新增、编辑）数据

        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="model">主表信息</param>
        /// <param name="itemModel">明细信息</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public ActionResult SubmitForm(string model, string orderModel, string itemModel, string type, int? keyValue1)
        {
            try
            {
                var PlanModel = JsonEx.JsonToObj<TbDistributionEnt>(model);
                var OrderModel = JsonEx.JsonToObj <List<TbDistributionEntOrder>>(orderModel);
                var PlanItem = JsonEx.JsonToObj<List<TbDistributionEntItem>>(itemModel);
                if (type == "add")
                {
                    PlanModel.InsertUserCode = base.UserCode;
                    var data = _deBus.Insert(PlanModel, OrderModel, PlanItem);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _deBus.Update(PlanModel, OrderModel, PlanItem, keyValue1);
                    return Content(data.ToJson());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 确认装车
        /// </summary>
        /// <returns></returns>
        public ActionResult ConfirmZC(int keyValue) 
        {
            var data = _deBus.ConfirmZC(keyValue);
            return Content(data.ToJson());
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
            var data = _deBus.Delete(keyValue);
            return Content(data.ToJson());
        }

        #endregion

        #region 获取该站点所属工区的用户
        /// <summary>
        /// 获取该站点所属工区的用户
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult GetWorkAreaUser(TbUserRequest request, string CompanyCode)
        {
            var data = _deBus.GetWorkAreaUser(request, CompanyCode);
            return Content(data.ToJson());
        }

        #endregion

        #region 获取加工厂下可以配送的加工订单信息

        /// <summary>
        /// 可配送订单弹窗
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetJgcPsOrder(DistributionPlanRequest request, string ProcessFactoryCode)
        {
            var data = _deBus.GetJgcPsOrder(request, ProcessFactoryCode);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 选择可配送订单明细信息
        /// </summary>
        /// <param name="request"></param>
        /// <param name="PSJHCode"></param>
        /// <returns></returns>
        public ActionResult GetPsOrderItem(DistributionPlanRequest request, string OrderCode) 
        {
            var data = _deBus.GetPsOrderItem(request, OrderCode);
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
            List<ExcelHead> cellheader = new List<ExcelHead>();
            cellheader.Add(new ExcelHead("SiteName", "站点名称"));
            cellheader.Add(new ExcelHead("DistributionCode", "配送装车编号"));
            cellheader.Add(new ExcelHead("OrderCode", "订单编号"));
            cellheader.Add(new ExcelHead("TypeCode", "类型编号"));
            cellheader.Add(new ExcelHead("TypeName", "类型名称"));
            cellheader.Add(new ExcelHead("UnloadingState", "卸货状态"));
            cellheader.Add(new ExcelHead("SignState", "签收状态"));
            cellheader.Add(new ExcelHead("UsePart", "使用部位"));
            cellheader.Add(new ExcelHead("PlanDistributionTime", "计划配送时间"));
            cellheader.Add(new ExcelHead("DistributionAddress", "配送地址"));
            cellheader.Add(new ExcelHead("TotalAggregate", "合计总量(kg)"));
            cellheader.Add(new ExcelHead("CarCph", "车辆编号"));
            cellheader.Add(new ExcelHead("CarUser", "驾驶员"));
            cellheader.Add(new ExcelHead("LoadCompleteTime", "装车完成时间"));
            cellheader.Add(new ExcelHead("ContactsName", "站点联系人"));
            cellheader.Add(new ExcelHead("ContactWay", "联系方式"));
            var request = JsonEx.JsonToObj<FPiCiXQPlan>(jsonData);
            var data = _deBus.GetExportList(request);
            decimal zzl = Convert.ToDecimal(data.Compute("sum(TotalAggregate)", "true"));
            string hzzfc = "合计(KG):" + zzl;
            var fileStream = ExcelHelper.ExportToMemoryStream(cellheader, data, "配送装车", hzzfc);
            return File(fileStream, "application/vnd.ms-excel", "配送装车.xls");
        }
        #endregion

    }
}
