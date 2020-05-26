using PM.Business.RawMaterial;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataAccess.DbContext;
using PM.DataEntity;
using PM.DataEntity.Production.ViewModel;
using PM.Domain.WebBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace PM.Web.Areas.RawMaterial.Controllers
{
    [HandlerLogin]
    public class RawMonthDemandSupplyPlanController : BaseController
    {
        //
        //原材料需求补充计划
        private readonly TbRawMaterialMonthDemandSupplyPlanLogic _rawMonthDemSupplyPlanLogic = new TbRawMaterialMonthDemandSupplyPlanLogic();

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
        public ActionResult GetGridJson(RawMonthDemSupplyPlanRequest request)
        {
            var data = _rawMonthDemSupplyPlanLogic.GetDataListForPage(request);
            return Content(data.ToJson());
        }
        #endregion

        #region 新增、修改、查看
        public ActionResult Form(string type)
        {
            if (type == "add")
            {
                ViewBag.SupplyPlanCode = CreateCode.GetTableMaxCode("BCJH", "SupplyPlanCode", "TbRawMaterialMonthDemandSupplyPlan");
                ViewBag.UserName = base.UserName;
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
        public ActionResult GetFormJson(string keyValue = "")
        {
            var str = string.Empty;
            int i = 0;
            int.TryParse(keyValue, out i);

            if (i > 0)
            {//ID
                str = "";
            }
            else
            {//月度需求编号
                str = keyValue;
            }

            var data = _rawMonthDemSupplyPlanLogic.FindEntity(i,str);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 通过补充计划编号获取数据
        /// </summary>
        /// <param name="SupplyPlanCode"></param>
        /// <returns></returns>
        public ActionResult GetDemSupplyPlanData(string SupplyPlanCode) 
        {
            var data = _rawMonthDemSupplyPlanLogic.GetDemSupplyPlanData(SupplyPlanCode);
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
                var monthDemandSupplyPlanModel = JsonEx.JsonToObj<TbRawMaterialMonthDemandSupplyPlan>(model);
                var monthDemandSupplyPlanItem = JsonEx.JsonToObj<List<TbRawMaterialMonthDemandSupplyPlanDetail>>(itemModel);
                if (type == "add")
                {
                    var data = _rawMonthDemSupplyPlanLogic.Insert(monthDemandSupplyPlanModel, monthDemandSupplyPlanItem);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _rawMonthDemSupplyPlanLogic.Update(monthDemandSupplyPlanModel, monthDemandSupplyPlanItem);
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
            var data = _rawMonthDemSupplyPlanLogic.Delete(keyValue);
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
            var data = _rawMonthDemSupplyPlanLogic.AnyInfo(keyValue);
            return Content(data.ToJson());
        }

        #endregion

        #region 获取可以选择的原需求计划
        public ActionResult GetRawMaterialMonthDemandPlanList(RawMonthDemPlanRequest request)
        {
            var data = _rawMonthDemSupplyPlanLogic.GetRawMaterialMonthDemandPlanList(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 通过原材料需求计划获取明细信息
        /// </summary>
        /// <param name="DemandPlanCode"></param>
        /// <returns></returns>
        public ActionResult GetRawMonthDemandPlanDetail(string DemandPlanCode)
        {
            var data = _rawMonthDemSupplyPlanLogic.GetRawMonthDemandPlanDetail(DemandPlanCode);
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
                    { "SupplyNum", "补充数量" },
                    { "DemandNum", "原计划数量" },
                    { "SkillRequire", "技术要求" },
                    { "Remark", "备注" },
                };
                var dataList = ExcelHelper.ExcelToEntityList<RawMaterialMonthDemandSupplyPlanDetail>(cellheader, streamfile, exName, out errorMsg);
                List<RawMaterialMonthDemandSupplyPlanDetail> list1 = new List<RawMaterialMonthDemandSupplyPlanDetail>();
                if (dataList.Count > 0)
                {
                    list1.AddRange(dataList);
                    for (int i = 0; i < dataList.Count; i++)
                    {
                        //去掉Excel中的空白行
                        if (dataList[i].SpecificationModel == null && dataList[i].DemandNum == 0 && dataList[i].SupplyNum == 0)
                        {
                            list1.Remove(dataList[i]);
                        }
                    }
                }
                var retList = new List<RawMaterialMonthDemandSupplyPlanDetail>();
                //所有原材料档案
                var rawAllMaterials = Repository<TbRawMaterialArchives>.GetAll().ToList();
                //单位
                var unit = Repository<TbSysDictionaryData>.Query(p => p.FDictionaryCode == "Unit");
                var index = 1;
                foreach (var item in dataList)
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
                    if (string.IsNullOrWhiteSpace(item.SupplyNum.ToString()) || item.SupplyNum <= 0)
                    {
                        errorMsg.AppendFormat("第{0}行补充数量{1}不正确！", index, item.SupplyNum);
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(item.DemandNum.ToString()))
                    {
                        errorMsg.AppendFormat("第{0}行需求补充计划明细名称{1}不能为空！", index, item.DemandNum);
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

        #region 导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="ProjectCode"></param>
        /// <returns></returns>
        public ActionResult OutputExcel(string jsonData)
        {

            //导出数据列
            Dictionary<string, string> cellheader = new Dictionary<string, string> {
                    { "SupplyPlanCode", "补充计划编号" },
                    { "DemandPlanCode", "需求计划编号" },
                    { "RebarTypeNew", "钢筋类型" },
                    { "BranchName", "分部名称" },
                    { "WorkAreaName", "工区名称" },
                    { "ProcessFactoryName", "加工厂名称" },
                    { "DeliveryAdd", "交货地点" },
                    { "SupplyPlanNum", "补充计划总量(kg)" },
                    { "GrandTotalPlanNum", "累计计划重量(kg)" },
                };
            var request = JsonEx.JsonToObj<RawMonthDemSupplyPlanRequest>(jsonData);
            var data = _rawMonthDemSupplyPlanLogic.GetExportList(request);
            decimal xg = 0;
            decimal jzgj = 0;
            if (data.Rows.Count > 0)
            {
                xg = Convert.ToDecimal(data.Compute("sum(GrandTotalPlanNum)", "RebarType='SectionSteel'"));
                jzgj = Convert.ToDecimal(data.Compute("sum(GrandTotalPlanNum)", "RebarType='BuildingSteel'"));
            }
            decimal zzl = xg + jzgj;
            string hzzfc = "建筑钢筋总量(KG):" + jzgj + ",型钢总量(KG):" + xg + ",合计总量(KG):" + zzl;
            var fileStream = ExcelHelper.ExportToMemoryStream(cellheader, data, "", "", "原材料月度需求补充计划", hzzfc);
            return File(fileStream, "application/vnd.ms-excel", "原材料月度需求补充计划.xls");
        }
        #endregion
    }
}
