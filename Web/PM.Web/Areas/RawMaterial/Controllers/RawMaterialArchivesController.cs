using PM.Business.RawMaterial;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataEntity;
using PM.DataEntity.Production.ViewModel;
using PM.Domain.WebBase;
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
    //原材料档案
    [HandlerLogin]
    public class RawMaterialArchivesController : BaseController
    {
        //
        // 原材料档案
        private readonly TbRawMaterialArchivesLogic _rawArchivesImp = new TbRawMaterialArchivesLogic();

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
        public ActionResult GetGridJson(RawMarchivesRequest request)
        {
            var data = _rawArchivesImp.GetDataListForPage(request);
            return Content(data.ToJson());
        }
        #endregion

        #region 新增、修改、查看
        public ActionResult Form(string type)
        {
            if (type == "add")
            {
                ViewBag.MaterialCode = CreateCode.GetTableMaxCode("MI", "MaterialCode", "TbRawMaterialArchives");
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
            var data = _rawArchivesImp.FindEntity(keyValue);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitForm(TbRawMaterialArchives model,string type)
        {
            try
            {
                if (type == "add")
                {
                    var data = _rawArchivesImp.Insert(model);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _rawArchivesImp.Update(model);
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
            var data = _rawArchivesImp.Delete(keyValue);
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
            TbRawMaterialMonthDemandPlanLogic _rawMonthDemPlanLogic = new TbRawMaterialMonthDemandPlanLogic();
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
                    { "MaterialCode", "原材料编号" }, 
                    { "MaterialName", "原材料名称" }, 
                    { "SpecificationModel", "规格型号" },  
                    { "RebarType", "钢筋类型" }, 
                    { "MeasurementUnit", "单位" }, 
                    { "MeasurementUnitZl", "单位重量" },
                    { "Remarks", "备注" }, 
                };
                var dataList = ExcelHelper.ExcelToEntityList<TbRawMaterialArchives>(cellheader, streamfile, exName, out errorMsg);
                var retList = new List<TbRawMaterialArchives>();
                var index = 1;
                var projectId = base.CurrentUser.ProjectId;
                foreach (var item in dataList)
                {
                    index++;
                    if (string.IsNullOrWhiteSpace(item.MaterialCode))
                    {
                        errorMsg.AppendFormat("第{0}行原材料编号不能为空！", index);
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(item.MaterialName))
                    {
                        errorMsg.AppendFormat("第{0}行原材料名称不能为空！", index);
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(item.SpecificationModel))
                    {
                        errorMsg.AppendFormat("第{0}行材料规格型号不能为空！", index);
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(item.RebarType))
                    {
                        errorMsg.AppendFormat("第{0}行钢筋类型不能为空！", index);
                        continue;
                    }
                    //查询单位编号
                    DataTable data1 = _rawMonthDemPlanLogic.GetUnitCode("RebarType", item.RebarType);
                    if (data1 != null && data1.Rows.Count > 0)
                    {
                        item.RebarType = data1.Rows[0][0].ToString();
                    }
                    else
                    {
                        errorMsg.AppendFormat("第{0}行钢筋类型错误，钢筋类型只能为型钢或者是建筑钢筋！", index, item.RebarType);
                        continue;
                    }
                    if (item.MeasurementUnitZl <= 0)
                    {
                        errorMsg.AppendFormat("第{0}行单位重量不能小于0！", index);
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(item.MeasurementUnit))
                    {
                        errorMsg.AppendFormat("第{0}行单位不能为空！", index);
                        continue;
                    }
                    //查询单位编号
                    DataTable data2 = _rawMonthDemPlanLogic.GetUnitCode("Unit",item.MeasurementUnit);
                    if (data2 != null && data2.Rows.Count > 0)
                    {
                        item.MeasurementUnit = data2.Rows[0][0].ToString();
                    }
                    else
                    {
                        errorMsg.AppendFormat("第{0}行单位,在系统中不存在，请先完善系统中数据字典信息！", index, item.MeasurementUnit);
                        continue;
                    }
                    var iindex = 1;
                    var flag = false;
                    foreach (var tep in dataList)
                    {
                        flag = false;
                        iindex++;
                        if (item.MaterialCode == tep.MaterialCode
                            && index != iindex
                            && index < iindex)
                        {
                            errorMsg.AppendFormat("第{0}行与第{1}行信息重复！", index, iindex);
                            break;
                        }
                        flag = true;
                    }
                    if (!flag)
                    {
                        continue;
                    }
                    Convert.ToString(item.MaterialCode).Trim();
                    Convert.ToString(item.MaterialName).Trim();
                    Convert.ToString(item.SpecificationModel);
                    Convert.ToString(item.MeasurementUnit).Trim();
                    Convert.ToDecimal(item.MeasurementUnitZl);
                    Convert.ToString(item.RebarType);
                    Convert.ToString(item.Remarks);
                    item.IndexNum = index;
                    retList.Add(item);
                }
                if (retList.Count > 0)
                    return Content(_rawArchivesImp.Input(retList, errorMsg).ToJson());
                else
                    return Error(errorMsg.ToString());
            }
            catch (Exception ex)
            {
                return Error(ex.ToString());
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

            //原材料编号、原材料名称、规格型号、钢筋类型、单位、单位重量、备注
            //MaterialCode,MaterialName,SpecificationModel,RebarTypeNew,MeasurementUnitNew,MeasurementUnitZl,Remarks
            Dictionary<string, string> cellheader = new Dictionary<string, string> { 
                    { "MaterialCode", "原材料编号" }, 
                    { "MaterialName", "原材料名称" }, 
                    { "SpecificationModel", "规格型号" }, 
                    { "RebarTypeNew", "钢筋类型" }, 
                    { "MeasurementUnitNew", "单位" }, 
                    { "MeasurementUnitZl", "单位重量" }, 
                    { "Remarks", "备注" }, 
                };
            var request = JsonEx.JsonToObj<RawMarchivesRequest>(jsonData);
            var data = _rawArchivesImp.GetExportList(request);
            string hzzfc = "";
            var fileStream = ExcelHelper.ExportToMemoryStream(cellheader, data, "", "", "原材料档案", hzzfc);
            return File(fileStream, "application/vnd.ms-excel", "原材料档案.xls");
        }

        #endregion

    }
}
