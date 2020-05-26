using PM.Business.RawMaterial;
using PM.Business.ShortMessage;
using PM.Common;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataEntity;
using PM.DataEntity.Production.ViewModel;
using PM.DataEntity.System.ViewModel;
using PM.Domain.WebBase;
using PM.Web.Models.ExcelModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.RawMaterial.Controllers
{
    // 加工厂批次需求计划 
    [HandlerLogin]
    public class FactoryBatchNeedPlanController : BaseController
    {
        //加工厂批次需求计划逻辑处理层类
        private readonly FactoryBatchNeedPlanLogic _fbnpBus = new FactoryBatchNeedPlanLogic();

        // GET: /RawMaterial/FactoryBatchNeedPlan/

        #region 视图

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.LoginUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
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
                ViewBag.BatchPlanNum = CreateCode.GetTableMaxCode("PCJH", "BatchPlanNum", "TbFactoryBatchNeedPlan");
                if (ViewBag.OrgType == "1")
                {
                    ViewBag.ProcessFactoryCode = OperatorProvider.Provider.CurrentUser.CompanyId;
                    ViewBag.ProcessFactoryName = OperatorProvider.Provider.CurrentUser.ComPanyName;
                }

                ViewBag.UserName = base.UserName;
                ViewBag.UserCode = base.UserCode;
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

        #endregion

        #region 查询数据

        /// <summary>
        /// 首页查询
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult GetAllOrBySearch(FPiCiXQPlan ent)
        {
            var data = _fbnpBus.GetAllOrBySearch(ent);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 以ID查询批次需求计划
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>

        [HandlerLogin(Ignore = false)]
        public ActionResult GetFormJson(int keyValue)
        {
            var data = _fbnpBus.GetFormJson(keyValue);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 以ID查询批次需求计划
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>

        [HandlerLogin(Ignore = false)]
        public ActionResult GetFormDetailJson(int keyValue)
        {
            var data = _fbnpBus.GetFormDetailJson(keyValue);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 月度需求计划弹窗
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetXQJHGridJson(RawMonthDemPlanRequest request, string keyword)
        {
            var data = _fbnpBus.GetXQJHGridJson(request, keyword);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 自动带入需求计划明细
        /// </summary>
        /// <returns></returns>
        public ActionResult GetXQJHDetail(string number,string ProjectId, string ProcessFactoryCode, string WorkAreaCode, string BranchCode)
        {
            var data = _fbnpBus.GetXQJHDetail(number, ProjectId, ProcessFactoryCode, WorkAreaCode, BranchCode);
            return Content(data.ToJson());
        }

        public ActionResult GetProcessFactoryUser(TbCompanyRequest request)
        {
            var data = _fbnpBus.GetProcessFactoryUser(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult AnyInfo(int keyValue)
        {
            var data = _fbnpBus.AnyInfo(keyValue);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取默认的验收人与电话
        /// </summary>
        /// <param name="ProcessFactoryCode">加工厂编号</param>
        /// <returns></returns>
        public ActionResult GetIsDefault(string ProcessFactoryCode)
        {
            var data = _fbnpBus.GetIsDefault(ProcessFactoryCode);
            return Content(data.ToJson());
        }

        public ActionResult GetMaterialGridJson(RawMaterialStockRequest request) 
        {
            var data = _fbnpBus.GetMaterialDataList(request);
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
        public ActionResult SubmitForm(string model, string itemModel, string type)
        {
            try
            {
                var PlanModel = JsonEx.JsonToObj<TbFactoryBatchNeedPlan>(model);
                var PlanItem = JsonEx.JsonToObj<List<TbFactoryBatchNeedPlanItem>>(itemModel);

                ////供应清单
                //var supList = JsonEx.JsonToObj<TbSupplyList>(model);
                //var supDetail = JsonEx.JsonToObj<List<TbSupplyListDetail>>(itemModel);
                if (type == "add")
                {
                    //var data = _fbnpBus.Insert(PlanModel, PlanItem, supList, supDetail);
                    var data = _fbnpBus.Insert(PlanModel, PlanItem);
                    return Content(data.ToJson());
                }
                else
                {
                    //var data = _fbnpBus.Update(PlanModel, PlanItem, supList, supDetail);
                    var data = _fbnpBus.Update(PlanModel, PlanItem);
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
            var data = _fbnpBus.Delete(keyValue);
            return Content(data.ToJson());
        }

        #endregion

        #region 导入

        /// <summary>
        /// 导入数据提交
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitInput()
        {
            StringBuilder errorMsg = new StringBuilder(); // 错误信息
            try
            {
                HttpPostedFileBase fostFile = Request.Files["Filedata"];
                Stream streamfile = fostFile.InputStream;
                string exName = Path.GetExtension(fostFile.FileName); //得到扩展名

                Dictionary<string, string> cellheader = new Dictionary<string, string>
                {
                    {"RawMaterialNum", "原材料编号"},
                    {"MaterialName", "原材料名称"},
                    {"Standard", "规格"},
                    {"MeasurementUnitText", "计量单位"},
                    {"BatchPlanQuantity", "批次计划数量"},
                    {"TechnicalRequirement", "技术要求"},
                    {"Remarks", "备注"},
                };
                var dataList =
                    ExcelHelper.ExcelToEntityList<TbFactoryBatchNeedPlanItem>(cellheader, streamfile, exName,
                        out errorMsg);
                var retList = new List<TbFactoryBatchNeedPlanItem>();

                var index = 1;
                foreach (var item in dataList)
                {
                    index++;
                    if (string.IsNullOrWhiteSpace(item.RawMaterialNum))
                    {
                        errorMsg.AppendFormat("第{0}行计划明细名称{1}不能为空！", index, item.RawMaterialNum);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(item.MaterialName))
                    {
                        errorMsg.AppendFormat("第{0}行计划明细名称{1}不能为空！", index, item.MaterialName);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(item.Standard))
                    {
                        errorMsg.AppendFormat("第{0}行计划明细规格{1}不能为空！", index, item.Standard);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(item.MeasurementUnitText))
                    {
                        errorMsg.AppendFormat("第{0}行计划明细单位{1}不能为空！", index, item.MeasurementUnitText);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(item.BatchPlanQuantity.ToString()))
                    {
                        errorMsg.AppendFormat("第{0}行计划明细数量{1}不能为空！", index, item.BatchPlanQuantity);
                        continue;
                    }

                    item.RawMaterialNum = item.RawMaterialNum.Replace(" ", "");
                    item.MaterialName = item.MaterialName.Replace(" ", "");
                    item.BatchPlanQuantity = Convert.ToDecimal(item.BatchPlanQuantity.ToString().Replace(" ", ""));

                    //var iindex = 1;
                    //var flag = false;
                    //foreach (var tep in dataList)
                    //{
                    //    flag = false;
                    //    iindex++;
                    //    if (item.RawMaterialNum == tep.MaterialCode && index != iindex && index < iindex)
                    //    {
                    //        errorMsg.AppendFormat("第{0}行与第{1}行需求计划明细名称{2}重复！", index, iindex, item.MaterialCode);
                    //        break;
                    //    }
                    //    flag = true;
                    //}
                    //if (!flag)
                    //{
                    //    continue;
                    //}
                    //查询单位编号
                    DataTable data = _fbnpBus.GetUnitCode("Unit", item.MeasurementUnitText);
                    if (data != null && data.Rows.Count > 0)
                    {
                        item.MeasurementUnit = data.Rows[0][0].ToString();
                    }
                    else
                    {
                        errorMsg.AppendFormat("第{0}行计划明细名称{0}单位错误！", index, item.MeasurementUnitText);
                        continue;
                    }

                    retList.Add(item);
                }

                if (errorMsg.Length > 0)
                {
                    return JsonMsg(false, retList, errorMsg.ToString());
                }
                else
                {
                    return JsonMsg(true, retList);
                }
            }
            catch (Exception ex)
            {
                return JsonMsg(false, ex.ToString());
            }
        }

        #endregion

        #region 根据用户编码查询联系方式

        public ActionResult GetUserPhone(string uid)
        {
            CensusdemoTask ct = new CensusdemoTask();
            var json = ct.up(uid);
            return Json(json, JsonRequestBehavior.AllowGet);
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
            var request = JsonEx.JsonToObj<FPiCiXQPlan>(jsonData);
            request.IsOutPut = true;
            var ret = _fbnpBus.GetAllOrBySearch(request);
            decimal zzl = 0;
            var data = (DataTable)ret.rows;
            if (data.Rows.Count > 0)
            {
                zzl = Convert.ToDecimal(data.Compute("sum(BatchPlanTotal)", "true"));
            }

            string hzzfc = "合计总量(KG):" + zzl;
            List<FactoryBatchNeedPlanExcel> dataList = ModelConvertHelper<FactoryBatchNeedPlanExcel>.ToList(data);
            var fileStream = ExcelHelper.EntityListToExcelStream<FactoryBatchNeedPlanExcel>(dataList, "加工厂批次需求计划", hzzfc);
            return File(fileStream, "application/vnd.ms-excel", "加工厂批次需求计划.xls");
        }

        #endregion

        #region 图形报表

        /// <summary>
        /// 图形1
        /// </summary>
        /// <param name="RebarType">钢筋类型</param>
        /// <returns></returns>
        public ActionResult Img1(FPiCiXQPlan ent)
        {
            var data = _fbnpBus.Img1(ent);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 图形2
        /// </summary>
        /// <param name="RebarType">钢筋类型</param>
        /// <returns></returns>
        public ActionResult Img2(FPiCiXQPlan ent)
        {
            var data = _fbnpBus.Img2(ent);
            return Content(data.ToJson());
        }

        public ActionResult FormImg2()
        {
            return View();
        }

        /// <summary>
        /// 图形3
        /// </summary>
        /// <param name="RebarType">钢筋类型</param>
        /// <returns></returns>
        public ActionResult Img3(FPiCiXQPlan ent)
        {
            var data = _fbnpBus.Img3(ent);
            return Content(data.ToJson());
        }

        #endregion

        #region 判断当前登录是否是物贸、分部工区物机部人员

        public ActionResult GetUserIsWmOrFbGqWjb() 
        {
            var data = _fbnpBus.GetUserIsWmOrFbGqWjb();
            return Content(data.ToJson());
        }

        #endregion


    }
}
