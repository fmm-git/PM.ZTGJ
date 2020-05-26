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

namespace PM.Web.Areas.RawMaterial.Controllers
{
    [HandlerLogin]
    public class RawMonthDemandPlanController : BaseController
    {
        //
        // 原材料月度需求计划
        private readonly TbRawMaterialMonthDemandPlanLogic _rawMonthDemPlanLogic = new TbRawMaterialMonthDemandPlanLogic();

        #region 列表
        public ActionResult Index()
        {
            ViewBag.LoginUserCode = OperatorProvider.Provider.CurrentUser.UserCode;
            return View();
        }

        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetGridJson(RawMonthDemPlanRequest request)
        {
            var data = _rawMonthDemPlanLogic.GetDataListForPage(request);
            return Content(data.ToJson());
        }

        #endregion

        #region 新增、修改、查看
        public ActionResult Form(string type)
        {
            if (type == "add")
            {
                ViewBag.DemandPlanCode = CreateCode.GetTableMaxCode("XQJH", "DemandPlanCode", "TbRawMaterialMonthDemandPlan");
                ViewBag.UserName = base.UserName;
                ViewBag.OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
                ViewBag.CompanyId = OperatorProvider.Provider.CurrentUser.CompanyId;
                //获取项目ID
                ViewBag.ProcessFactoryCode = OperatorProvider.Provider.CurrentUser.ProcessFactoryCode;
                ViewBag.ProcessFactoryName = OperatorProvider.Provider.CurrentUser.ProcessFactoryName;
                //获取加工厂地址
                ViewBag.DeliveryAdd = _rawMonthDemPlanLogic.GetCompanyAdd(OperatorProvider.Provider.CurrentUser.ProcessFactoryCode);
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
            var data = _rawMonthDemPlanLogic.FindEntity(keyValue);
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
                var monthDemandPlanModel = JsonEx.JsonToObj<TbRawMaterialMonthDemandPlan>(model);
                var monthDemandPlanItem = JsonEx.JsonToObj<List<TbRawMaterialMonthDemandPlanDetail>>(itemModel);
                if (type == "add")
                {
                    var data = _rawMonthDemPlanLogic.Insert(monthDemandPlanModel, monthDemandPlanItem);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _rawMonthDemPlanLogic.Update(monthDemandPlanModel, monthDemandPlanItem);
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
            var data = _rawMonthDemPlanLogic.Delete(keyValue);
            return Content(data.ToJson());
        }

        #endregion

        #region 获取组织机构列表
        /// <summary>
        /// 获取组织机构列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetCompanyList(int type, TbCompanyRequest request)
        {
            var data = _rawMonthDemPlanLogic.GetCompanyList(type, request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 通过分部编号获取工区或者站点
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetCompanyWorkAreaOrSiteList(TbCompanyRequest request, string parentCode, int type)
        {
            var data = _rawMonthDemPlanLogic.GetCompanyWorkAreaOrSiteList(request, parentCode, type);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 查询组织机构（分部、工区、站点）
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAllCompany()
        {
            var data = _rawMonthDemPlanLogic.GetCompany();
            var treeList = new List<TreeGridModel>();
            foreach (TbCompany item in data)
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
        /// <summary>
        /// 获取该组织机构的所有父级
        /// </summary>
        /// <param name="CompanyCode"></param>
        /// <returns></returns>
        public ActionResult GetParentCompany(string CompanyCode)
        {
            var data = _rawMonthDemPlanLogic.GetParentCompany(CompanyCode);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取数据列表(原材料)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetMaterialGridJson(RawMaterialStockRequest request, string RebarType)
        {
            var data = _rawMonthDemPlanLogic.GetMaterialDataList(request, RebarType);
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
            var data = _rawMonthDemPlanLogic.AnyInfo(keyValue);
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
                var fostFile = Request.Files;
                Stream streamfile = fostFile[0].InputStream;
                string exName = Path.GetExtension(fostFile[0].FileName); //得到扩展名
                Dictionary<string, string> cellheader = new Dictionary<string, string> { 
                    { "MaterialCode", "原材料编号" }, 
                    { "MaterialName", "原材料名称" }, 
                    { "SpecificationModel", "规格" }, 
                    { "MeasurementUnitText", "计量单位" }, 
                    { "DemandNum", "需求数量" }, 
                    { "SkillRequire", "技术要求" }, 
                    { "Remark", "备注" }, 
                };
                var dataList = ExcelHelper.ExcelToEntityList<RawMaterialMonthDemandPlanDetail>(cellheader, streamfile, exName, out errorMsg);
                List<RawMaterialMonthDemandPlanDetail> list1 = new List<RawMaterialMonthDemandPlanDetail>();
                if (dataList.Count > 0)
                {
                    list1.AddRange(dataList);
                    for (int i = 0; i < dataList.Count; i++)
                    {
                        //去掉Excel中的空白行
                        if (dataList[i].SpecificationModel == null && dataList[i].DemandNum == 0)
                        {
                            list1.Remove(dataList[i]);
                        }
                    }
                }
                var retList = new List<RawMaterialMonthDemandPlanDetail>();
                //所有原材料档案
                var rawAllMaterials = Repository<TbRawMaterialArchives>.GetAll().ToList();
                //单位
                var unit = Repository<TbSysDictionaryData>.Query(p => p.FDictionaryCode == "Unit");
                var index = 1;
                foreach (var item in list1)
                {
                    index++;
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
                    if (string.IsNullOrWhiteSpace(item.DemandNum.ToString()) || item.DemandNum <= 0)
                    {
                        errorMsg.AppendFormat("第{0}行月度需求计划数量{1}不正确！", index, item.DemandNum);
                        continue;
                    }
                    item.MaterialCode = item.MaterialCode.Replace(" ", "");
                    item.MaterialName = item.MaterialName.Replace(" ", "");
                    item.DemandNum = Convert.ToDecimal(item.DemandNum.ToString().Replace(" ", ""));

                    var iindex = 1;
                    var flag = false;
                    foreach (var tep in dataList)
                    {
                        flag = false;
                        iindex++;
                        if (item.SpecificationModel == tep.SpecificationModel && index != iindex && index < iindex)
                        {
                            errorMsg.AppendFormat("第{0}行与第{1}行原材料的{2}重复！", index, iindex, item.SpecificationModel);
                            break;
                        }
                        flag = true;
                    }
                    if (!flag)
                    {
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

        #region 获取当前登录人所属的组织机构跟下级组织机构

        public ActionResult GetLoginUserAllCompany()
        {
            var data = _rawMonthDemPlanLogic.GetLoginUserAllCompany();
            var treeList = new List<TreeGridModel>();
            foreach (TbCompany item in data)
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

        public ActionResult GetLoginUserAllCompanyNoSite()
        {
            var data = _rawMonthDemPlanLogic.GetLoginUserAllCompany(false);
            var treeList = new List<TreeGridModel>();
            foreach (TbCompany item in data)
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

        public ActionResult GetLoginUserAllCompanyNoSiteNew(string RebarType = "", string HistoryMonth = "")
        {
            if (string.IsNullOrWhiteSpace(RebarType))
            {
                RebarType = "BuildingSteel";
            }
            if (string.IsNullOrWhiteSpace(HistoryMonth))
            {
                HistoryMonth = DateTime.Now.ToString("yyyy-MM");
            }
            var data = _rawMonthDemPlanLogic.GetLoginUserAllCompanyNew(false, "RawMonthDemandPlan", RebarType, HistoryMonth);
            var treeList = new List<TreeGridModel>();
            foreach (TbCompanyNew item in data)
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

        #region 导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="ProjectCode"></param>
        /// <returns></returns>
        public ActionResult OutputExcel(string jsonData)
        {
            var request = JsonEx.JsonToObj<RawMonthDemPlanRequest>(jsonData);
            request.IsOutput = true;
            var data = _rawMonthDemPlanLogic.GetDataListForPage(request);
            decimal xg = ((SumRowData)data.userdata).SectionSteel;
            decimal jzgj = ((SumRowData)data.userdata).BuildingSteel;
            decimal zzl = xg + jzgj;
            string hzzfc = "建筑钢筋总量(KG):" + jzgj + ",型钢总量(KG):" + xg + ",合计总量(KG):" + zzl;
            List<RawMonthDemandPlanExcel> dataList = ModelConvertHelper<RawMonthDemandPlanExcel>.ToList((DataTable)data.rows);
            var fileStream = ExcelHelper.EntityListToExcelStream<RawMonthDemandPlanExcel>(dataList, "原材料月度需求计划", hzzfc);
            return File(fileStream, "application/vnd.ms-excel", "原材料月度需求计划.xls");
        }
        #endregion

        #region 图形报表
        /// <summary>
        /// 图形1
        /// </summary>
        /// <param name="RebarType">钢筋类型</param>
        /// <param name="DemandMonth">需求月份</param>
        /// <returns></returns>
        public ActionResult Img1(RawMonthDemPlanRequest request)
        {

            var data = _rawMonthDemPlanLogic.Img1(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 图形2
        /// </summary>
        /// <param name="RebarType">钢筋类型</param>
        /// <param name="DemandMonth">需求月份</param>
        /// <returns></returns>
        public ActionResult Img2(RawMonthDemPlanRequest request)
        {
            var data = _rawMonthDemPlanLogic.Img2(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 图形3
        /// </summary>
        /// <param name="RebarType">钢筋类型</param>
        /// <param name="DemandMonth">需求月份</param>
        /// <returns></returns>
        public ActionResult Img3(RawMonthDemPlanRequest request)
        {
            var data = _rawMonthDemPlanLogic.Img3(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 图形4
        /// </summary>
        /// <param name="RebarType">钢筋类型</param>
        /// <param name="DemandMonth">需求月份</param>
        /// <returns></returns>
        public ActionResult Img4(RawMonthDemPlanRequest request)
        {
            var data = _rawMonthDemPlanLogic.Img4(request);
            return Content(data.ToJson());
        }
        #endregion
    }
}
