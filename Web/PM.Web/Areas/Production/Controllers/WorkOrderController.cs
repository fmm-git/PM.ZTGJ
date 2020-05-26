using Dos.Common;
using PM.Business.Production;
using PM.Business.RawMaterial;
using PM.Common;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.Production.ViewModel;
using PM.DataEntity.System.ViewModel;
using PM.Domain.WebBase;
using PM.Web.Models.ExcelModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.Production.Controllers
{
    [HandlerLogin]
    public class WorkOrderController : BaseController
    {
        //
        //加工订单
        private readonly TbWorkOrderLogic _workOrderLogic = new TbWorkOrderLogic();

        #region 列表
        public ActionResult Index()
        {
            //ViewBag.orgType = PM.Common.OperatorProvider.Provider.CurrentUser.OrgType;
            //ViewBag.CompanyId = PM.Common.OperatorProvider.Provider.CurrentUser.CompanyId;
            ViewBag.ProcessFactoryCode = PM.Common.OperatorProvider.Provider.CurrentUser.ProcessFactoryCode;
            ViewBag.LoginUserCode = PM.Common.OperatorProvider.Provider.CurrentUser.UserCode;
            return View();
        }

        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetGridJson(WorkOrderRequest request)
        {
            if (request.SiteCode == null)
            {
                if (OperatorProvider.Provider.CurrentUser.OrgType != "1")
                    request.SiteCode = OperatorProvider.Provider.CurrentUser.CompanyId;
            }
            var data = _workOrderLogic.GetDataListForPage(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 加载报表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetWorkOrderReportForm(WorkOrderRequest request)
        {
            var data = _workOrderLogic.GetWorkOrderReportForm(request);
            return Content(data.ToJson());
        }

        #endregion

        #region 新增、修改、查看
        public ActionResult Form(string type)
        {
            if (type == "add")
            {
                ViewBag.OrderCode = CreateCode.GetTableMaxCode("JGDD", "OrderCode", "TbWorkOrder");
                ViewBag.UserName = base.UserName;
                string orgType = PM.Common.OperatorProvider.Provider.CurrentUser.OrgType;
                ViewBag.OrgType = orgType;
                //判断当前登录人是否是站点人员
                if (orgType == "5")
                {
                    string CompanyId = PM.Common.OperatorProvider.Provider.CurrentUser.CompanyId;
                    DataTable dt = _workOrderLogic.GetCompany(CompanyId);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        ViewBag.SiteCode = CompanyId;
                        ViewBag.SiteName = dt.Rows[0]["CompanyFullName"].ToString();
                        ViewBag.DistributionAdd = dt.Rows[0]["Address"].ToString();
                    }

                }
                //if (orgType == "1")
                //{
                //    ViewBag.ProcessFactoryCode = PM.Common.OperatorProvider.Provider.CurrentUser.CompanyId;
                //    ViewBag.ProcessFactoryName = PM.Common.OperatorProvider.Provider.CurrentUser.ComPanyName;
                //}
                ViewBag.ProcessFactoryCode = PM.Common.OperatorProvider.Provider.CurrentUser.ProcessFactoryCode;
                ViewBag.ProcessFactoryName = PM.Common.OperatorProvider.Provider.CurrentUser.ProcessFactoryName;
                ViewBag.ProjectId = PM.Common.OperatorProvider.Provider.CurrentUser.ProjectId;
            }
            var processingTechnology = new TbProcessingTechnologyLogic().GetChildList()
             .Select(p => string.Join(",", string.Format("{0}:{1}", p.ID, p.ProcessingTechnologyName)))
             .ToJson().ToString().Replace("[", "").Replace("]", "").Replace("\"", "").Replace(",", ";");

            ViewBag.ProcessingTechnology = processingTechnology;
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
            var data = _workOrderLogic.FindEntity(keyValue);
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
                var workOrderModel = JsonEx.JsonToObj<TbWorkOrder>(model);
                var workOrderItem = JsonEx.JsonToObj<List<TbWorkOrderDetail>>(itemModel);

                if (type == "add")
                {
                    var data = _workOrderLogic.Insert(workOrderModel, workOrderItem);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _workOrderLogic.Update(workOrderModel, workOrderItem);
                    return Content(data.ToJson());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region 修改打包数量

        public ActionResult UpdatePackNum()
        {
            return View();
        }

        [HandlerLogin(Ignore = false)]
        public ActionResult GetFormJson1(int keyValue)
        {
            var data = _workOrderLogic.FindEntity1(keyValue);
            return Content(data.ToJson());
        }

        [HandlerLogin(Ignore = false)]
        public ActionResult SavePackNum(int keyValue, string itemModel)
        {
            var item = JsonEx.JsonToObj<List<TbWorkOrderDetail>>(itemModel);
            var data = _workOrderLogic.SavePackNum(keyValue, item);
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
            var data = _workOrderLogic.Delete(keyValue);
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
            var data = _workOrderLogic.AnyInfo(keyValue);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 判断信息是否线上订单
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult IsOffline(int keyValue)
        {
            var data = _workOrderLogic.IsOffline(keyValue);
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
                //HttpPostedFileBase fostFile = Request.Files["Filedata"];
                //Stream streamfile = fostFile.InputStream;
                //string exName = Path.GetExtension(fostFile.FileName); //得到扩展名
                var fostFile = Request.Files;
                Stream streamfile = fostFile[0].InputStream;
                string exName = Path.GetExtension(fostFile[0].FileName); //得到扩展名
                Dictionary<string, string> cellheader = new Dictionary<string, string> {
                    { "ComponentName","构件名称"},
                    { "LargePattern","大样图"},
                    { "MaterialCode","原材料编号"},
                    { "MaterialName","原材料名称"},
                    { "SpecificationModel","规格"},
                    { "MeasurementUnitText","计量单位"},
                    { "MeasurementUnitZl","单位重量"},
                    { "ItemUseNum","单件用量"},
                    { "Number","件数"},
                    { "WeightSmallPlan","重量小计"},
                    { "SkillRequirement","技术要求"},
                    { "ProcessingTechnologyName","加工工艺"},
                    { "Remark","备注"},
                };
                var dataList = ExcelHelper.ExcelToEntityList<TbWorkOrderDetail>(cellheader, streamfile, exName, out errorMsg);
                List<TbWorkOrderDetail> list1 = new List<TbWorkOrderDetail>();
                if (dataList.Count > 0)
                {
                    list1.AddRange(dataList);
                    for (int i = 0; i < dataList.Count; i++)
                    {
                        //去掉Excel中的空白行
                        if (dataList[i].ComponentName == null && dataList[i].LargePattern == null && dataList[i].SpecificationModel == null && dataList[i].ProcessingTechnologyName == null && dataList[i].Number == 0 && dataList[i].ItemUseNum == 0 && dataList[i].MeasurementUnitZl == 0)
                        {
                            list1.Remove(dataList[i]);
                        }
                    }
                }
                var retList = new List<TbWorkOrderDetail>();
                TbRawMaterialMonthDemandPlanLogic _rawMonthDemPlanLogic = new TbRawMaterialMonthDemandPlanLogic();
                //所有原材料档案
                var rawAllMaterials = Repository<TbRawMaterialArchives>.GetAll().ToList();
                //单位
                var unit = Repository<TbSysDictionaryData>.Query(p => p.FDictionaryCode == "Unit");
                var index = 1;
                foreach (var item in list1)
                {
                    index++;

                    if (string.IsNullOrWhiteSpace(item.ComponentName))
                    {
                        errorMsg.AppendFormat("第{0}行构件名称不能为空！</br>", index);
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(item.LargePattern))
                    {
                        errorMsg.AppendFormat("第{0}行大样图不能为空！</br>", index);
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(item.SpecificationModel))
                    {
                        errorMsg.AppendFormat("第{0}行规格不能为空！</br>", index);
                        continue;
                    }
                    else
                    {
                        //判断原材料是否存在
                        var rawMaterial = rawAllMaterials.Where(p => p.SpecificationModel == item.SpecificationModel).FirstOrDefault();
                        if (rawMaterial != null)
                        {
                            item.MaterialName = rawMaterial.MaterialName;
                            item.MaterialCode = rawMaterial.MaterialCode;
                            item.SpecificationModel = rawMaterial.SpecificationModel;
                            item.MeasurementUnit = rawMaterial.MeasurementUnit;
                            //if (item.MeasurementUnitZl != rawMaterial.MeasurementUnitZl)
                            //{
                            //    errorMsg.AppendFormat("第{0}行规格的原材料的单位重量不正确！</br>", index);
                            //    continue;
                            //}
                            //原材料单位
                            var rawUnit = unit.FirstOrDefault(p => p.DictionaryCode == rawMaterial.MeasurementUnit);
                            item.MeasurementUnitText = rawUnit.DictionaryText;
                        }
                        else
                        {
                            errorMsg.AppendFormat("第{0}行规格的原材料不存在！</br>", index);
                            continue;
                        }
                    }
                    if (string.IsNullOrWhiteSpace(item.MeasurementUnitZl.ToString()))
                    {
                        errorMsg.AppendFormat("第{0}行规格的原材料的单位重量不能为空！</br>", index);
                        continue;
                    }
                    else
                    {
                        if (item.MeasurementUnitZl <= 0)
                        {
                            errorMsg.AppendFormat("第{0}行规格的原材料的单位重量不正确！</br>", index);
                            continue;
                        }
                    }
                    //单件用量
                    if (string.IsNullOrWhiteSpace(item.ItemUseNum.ToString()))
                    {
                        errorMsg.AppendFormat("第{0}行单件用量不能为空！</br>", index);
                        continue;
                    }
                    else
                    {
                        if (item.ItemUseNum <= 0)
                        {
                            errorMsg.AppendFormat("第{0}行单件用量必须大于0！</br>", index);
                            continue;
                        }
                    }
                    //件数
                    if (string.IsNullOrWhiteSpace(item.Number.ToString()))
                    {
                        errorMsg.AppendFormat("第{0}行件数不能为空！</br>", index);
                        continue;
                    }
                    else
                    {
                        if (item.Number <= 0)
                        {
                            errorMsg.AppendFormat("第{0}行件数必须大于0！</br>", index);
                            continue;
                        }
                    }
                    //重量小计
                    if (string.IsNullOrWhiteSpace(item.WeightSmallPlan.ToString()))
                    {
                        errorMsg.AppendFormat("第{0}行重量小计不能为空！</br>", index);
                        continue;
                    }
                    else
                    {
                        //判断重量小计是否正确
                        if (item.WeightSmallPlan <= 0)
                        {
                            errorMsg.AppendFormat("第{0}行重量小计不正确！</br>", index);
                            continue;
                        }
                        else
                        {
                            if (item.WeightSmallPlan != (item.MeasurementUnitZl * item.Number * item.ItemUseNum))
                            {
                                errorMsg.AppendFormat("第{0}行重量小计不正确！</br>", index);
                                continue;
                            }
                        }
                    }

                    #region 加工工艺

                    if (string.IsNullOrWhiteSpace(item.ProcessingTechnologyName))
                    {
                        errorMsg.AppendFormat("第{0}行加工工艺不能为空！</br>", index);
                        continue;
                    }
                    else
                    {
                        var pt = Repository<TbProcessingTechnology>.First(p => p.ProcessingTechnologyName == item.ProcessingTechnologyName && p.PID != 0);
                        if (pt != null)
                        {
                            item.ProcessingTechnologyName = pt.ProcessingTechnologyName;
                            item.ProcessingTechnology = pt.ID;
                        }
                        else
                        {
                            errorMsg.AppendFormat("第{0}行加工工艺在系统中不存在！</br>", index);
                            continue;
                        }
                    }

                    #endregion

                    item.ComponentName = item.ComponentName.Replace(" ", "");
                    item.LargePattern = item.LargePattern.Replace(" ", "");
                    item.MaterialCode = item.MaterialCode.Replace(" ", "");
                    item.MaterialName = item.MaterialName.Replace(" ", "");
                    item.MeasurementUnitZl = Convert.ToDecimal(item.MeasurementUnitZl.ToString().Replace(" ", ""));
                    item.ItemUseNum = Convert.ToDecimal(item.ItemUseNum.ToString().Replace(" ", ""));
                    item.Number = Convert.ToInt32(item.Number.ToString().Replace(" ", ""));
                    item.WeightSmallPlan = Convert.ToDecimal(item.WeightSmallPlan.ToString("f5").Replace(" ", ""));
                    item.DaetailWorkStrat = "未加工";
                    item.RevokeStart = "正常";
                    item.PackNumber = 0;
                    item.PackageNumber = 0;
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

        #region 获取当前登录人下的所有站点
        //public ActionResult GetCompanyCompanyAllSiteList()
        //{
        //    var data = _workOrderLogic.GetCompanyCompanyAllSiteList();
        //    return Content(data.ToJson());
        //}
        public ActionResult GetCompanyCompanyAllSiteList(TbCompanyRequest request)
        {
            request.rows = 50;
            var data = _workOrderLogic.GetCompanyCompanyAllSiteList(request);
            return Content(data.ToJson());
        }
        #endregion

        #region 订单打包
        /// <summary>
        /// 打包二维码列表页面
        /// </summary>
        /// <returns></returns>
        public ActionResult OrderPackIndex()
        {
            return View();
        }
        /// <summary>
        /// 获取当前登录人该站点下的订单明细
        /// </summary>
        /// <returns></returns>
        public ActionResult GetNowSiteWorkOrderDetailList(WorkOrderPackResponse request)
        {
            request.rows = 15;
            var data = _workOrderLogic.GetNowSiteWorkOrderDetailList(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取该组织机构下所有的加工订单
        /// </summary>
        /// <returns></returns>
        public ActionResult GetWorkOrderList(WorkOrderRequest request)
        {
            request.rows = 50;
            var data = _workOrderLogic.GetWorkOrderList(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 打印二维码页面
        /// </summary>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult PrintPackQRCodeInfo(string actionCode, string tablestr)
        {
            if (Session["tablestr"] != null)
            {
                ViewBag.PrintData = Session["tablestr"].ToString();
                Session["tablestr"] = null;
            }
            if (actionCode == "SaveHtmlSession")
            {
                Session["tablestr"] = tablestr;
            }
            return View();
        }

        #endregion

        #region 订单进度图
        public ActionResult GetOrderProgess(string OrderCode)
        {
            var data = _workOrderLogic.GetOrderProgess(OrderCode);
            return Content(data.ToJson());
        }

        #endregion

        #region 产能填报
        public ActionResult WorkCapacityFilling()
        {
            ViewBag.ProcessFactoryCode = PM.Common.OperatorProvider.Provider.CurrentUser.ProcessFactoryCode;
            ViewBag.ProcessFactoryName = PM.Common.OperatorProvider.Provider.CurrentUser.ProcessFactoryName;
            ViewBag.OrgType = PM.Common.OperatorProvider.Provider.CurrentUser.OrgType;
            return View();
        }
        /// <summary>
        /// 获取产能填报信息
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        [HandlerLogin(Ignore = false)]
        public ActionResult GetWorkCapacityFormJson(string keyValue, string month)
        {
            var data = _workOrderLogic.GetWorkCapacityFormJson(keyValue, month);
            return Content(data.ToJson());
        }

        public ActionResult GetWorkCapacityIsDefault(string keyValue)
        {
            var data = _workOrderLogic.GetWorkCapacityIsDefault(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 保存产能填报信息
        /// </summary>
        /// <param name="model"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HandlerLogin(Ignore = false)]
        public ActionResult CapacitySubmitForm(string model, string type)
        {
            try
            {
                var capacityModel = JsonEx.JsonToObj<TbCapacityFilling>(model);
                if (type == "add")
                {
                    var data = _workOrderLogic.InsertCapacity(capacityModel);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _workOrderLogic.UpdateCapacity(capacityModel);
                    return Content(data.ToJson());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取当月加工完成量
        /// </summary>
        /// <param name="model"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HandlerLogin(Ignore = false)]
        public ActionResult GetWorkJgwcl(string keyValue, string month)
        {
            var data = _workOrderLogic.GetWorkJgwcl(keyValue, month);
            return Content(data.ToJson());
        }
        #endregion

        #region 订单的节点图形
        /// <summary>
        /// 获取领料信息
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        public ActionResult GetLlInfo(string OrderCode)
        {
            var data = _workOrderLogic.GetLlInfo(OrderCode);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取加工进度信息
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        public ActionResult GetJgJdInfo(string OrderCode)
        {
            var data = _workOrderLogic.GetJgJdInfo(OrderCode);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取运输卸货统计信息
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        public ActionResult GetYsXhTjInfo(string OrderCode)
        {
            var data = _workOrderLogic.GetYsXhTjInfo(OrderCode);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取签收完成信息
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <returns></returns>
        public ActionResult GetQsWcInfo(string OrderCode)
        {
            var data = _workOrderLogic.GetQsWcInfo(OrderCode);
            return Content(data.ToJson());
        }
        #endregion

        #region 配送确认

        public ActionResult DeliveryConfirmView(int keyValue)
        {
            ViewBag.keyValue = keyValue;
            return View();
        }

        /// <summary>
        /// 配送确认
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult DeliveryConfirm(int keyValue, DateTime deliveryCompleteTime, string Enclosure, string Remark)
        {
            var data = _workOrderLogic.DeliveryConfirm(keyValue, deliveryCompleteTime, Enclosure, Remark);
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
            var request = JsonEx.JsonToObj<WorkOrderRequest>(jsonData);
            request.IsOutPut = true;
            var ret = _workOrderLogic.GetDataListForPage(request);
            decimal hj = 0;
            var data = (List<WorkOrderListModel>)ret.rows;
            if (data.Count > 0)
            {
                hj = data.Sum(p => p.WeightTotal);
            }
            string hzzfc = "合计(KG):" + hj;
            var dataList = MapperHelper.Map<WorkOrderListModel, WorkOrderExcel>(data);
            var fileStream = ExcelHelper.EntityListToExcelStream<WorkOrderExcel>(dataList, "加工订单", hzzfc);
            return File(fileStream, "application/vnd.ms-excel", "加工订单.xls");
        }

        #endregion

        #region 统计报表

        public ActionResult Img2(WorkOrderRequest request)
        {
            var data = _workOrderLogic.Img2(request);
            return Content(data.ToJson());
        }
        public ActionResult Img3(WorkOrderRequest request)
        {
            var data = _workOrderLogic.Img3(request);
            return Content(data.ToJson());
        }
        public ActionResult Img4(WorkOrderRequest request)
        {
            var data = _workOrderLogic.Img4(request);
            return Content(data.ToJson());
        }
        #endregion

        #region 左侧组织机构
        public ActionResult GetLoginUserAllCompanyNoSiteNew(string IsQB = "否", string HistoryMonth = "", string MonthType = "加工月份")
        {
            if (IsQB == "否")
            {
                if (string.IsNullOrWhiteSpace(HistoryMonth))
                {
                    HistoryMonth = DateTime.Now.ToString("yyyy-MM");
                }
            }
            var data = _workOrderLogic.GetLoginUserAllCompanyNew(true, "WorkOrder", HistoryMonth, MonthType);
            var treeList = new List<TreeGridModel>();
            foreach (TbCompanyNew1 item in data)
            {
                TreeGridModel treeModel = new TreeGridModel();
                bool hasChildren = data.Count(t => t.ParentCompanyCode == item.CompanyCode) == 0 ? false : true;
                treeModel.id = item.CompanyCode;
                treeModel.text = item.CompanyFullName;
                if (data.Count(t => t.CompanyCode == item.ParentCompanyCode) == 0)
                {
                    item.ParentCompanyCode = "0";
                }
                treeModel.isLeaf = hasChildren;
                treeModel.parentId = item.ParentCompanyCode;
                treeModel.expanded = hasChildren;
                treeModel.entityJson = item.ToJson();
                treeList.Add(treeModel);
            }
            return Content(treeList.TreeGridJson());
        }

        #endregion

    }
}
