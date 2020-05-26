using PM.Business;
using PM.Business.RawMaterial;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.RawMaterial.Controllers
{
    /// <summary>
    /// 原材料初期库存
    /// </summary>
    [HandlerLogin]
    public class RawMaterialStockController : BaseController
    {
        private readonly RawMaterialStockLogic _rawMaterialStock = new RawMaterialStockLogic();

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
        public ActionResult GetGridJson(RawMaterialStockRequest request)
        {
            var data = _rawMaterialStock.GetDataListForPage(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取分页列表数据(明细)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetDetialGridJson(RawMaterialStockRequest request)
        {
            var data = _rawMaterialStock.GetDataDetialForPage(request);
            return Content(data.ToJson());
        }

        #endregion

        #region 编辑

        /// <summary>
        /// 新增、修改页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Form()
        {
            ViewBag.UserName = base.UserName;
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

        /// <summary>
        /// 编辑/查看页获取数据
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>

        [HandlerLogin(Ignore = false)]
        public ActionResult GetFormJson(int keyValue)
        {
            var data = _rawMaterialStock.FindEntity(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitForm(string itemModel, string type)
        {
            try
            {
                var stockRecord = JsonEx.JsonToObj<List<TbRawMaterialStockRecord>>(itemModel);
                foreach (var item in stockRecord)
                {
                    if (string.IsNullOrWhiteSpace(item.ProjectId))
                        return Error("项目信息错误");
                    if (string.IsNullOrEmpty(item.SiteCode))
                        item.SiteCode = "";
                    if (!item.UseCountS.HasValue)
                        item.UseCount = item.Count;
                    else
                        item.UseCount = item.UseCountS.Value;
                    item.LockCount = 0;
                    item.HistoryCount = item.Count;
                    item.ChackState = 1;
                    if (type == "add")
                        item.InsertUserCode = base.UserCode;
                }
                if (type == "add")
                {
                    var data = _rawMaterialStock.Insert(stockRecord);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _rawMaterialStock.Update(stockRecord);
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
            var data = _rawMaterialStock.Delete(keyValue);
            return Content(data.ToJson());
        }

        #endregion

        #region 导入

        /// <summary>
        /// 导入数据提交
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitInput(string keyValue)
        {
            StringBuilder errorMsg = new StringBuilder(); // 错误信息
            try
            {
                var fostFile = Request.Files;
                Stream streamfile = fostFile[0].InputStream;
                string exName = Path.GetExtension(fostFile[0].FileName); //得到扩展名
                Dictionary<string, string> cellheader = new Dictionary<string, string> {
                    { "BranchCode", "分部" },
                    { "WorkAreaCode", "工区" },
                    { "SpecificationModel", "规格" },
                    { "Count", "重量" },
                    { "UseCount", "动态库存重量" },
                    { "StorageCode", "仓库" },
                    { "Factory", "厂家" },
                    { "BatchNumber", "炉批号" },
                    { "TestReportNo", "质检报告编号" },
                };
                var dataList = ExcelHelper.ExcelToEntityList<TbRawMaterialStockRecord>(cellheader, streamfile, exName, out errorMsg);
                var retList = new List<TbRawMaterialStockRecord>();
                var index = 1;
                foreach (var item in dataList)
                {
                    index++;
                    //if (string.IsNullOrWhiteSpace(item.MaterialCode))
                    //{
                    //    errorMsg.AppendFormat("第{0}行原材料编号不能为空！", index);
                    //    continue;
                    //}
                    //if (string.IsNullOrWhiteSpace(item.MaterialName))
                    //{
                    //    errorMsg.AppendFormat("第{0}行原材料名称不能为空！", index);
                    //    continue;
                    //}
                    if (string.IsNullOrWhiteSpace(item.BranchCode))
                    {
                        errorMsg.AppendFormat("第{0}行分部不能为空！", index);
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(item.WorkAreaCode))
                    {
                        errorMsg.AppendFormat("第{0}行工区不能为空！", index);
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(item.SiteCode))
                    {
                        item.SiteCode = "";
                    }
                    if (string.IsNullOrWhiteSpace(item.SpecificationModel))
                    {
                        errorMsg.AppendFormat("第{0}行规格不能为空！", index);
                        continue;
                    }
                    //if (item.Count == 0)
                    //{
                    //    errorMsg.AppendFormat("第{0}行数量不能为空！", index);
                    //    continue;
                    //}
                    if (item.UseCount == 0)
                        item.UseCount = item.Count;
                    if (string.IsNullOrWhiteSpace(item.StorageCode))
                    {
                        errorMsg.AppendFormat("第{0}行仓库不能为空！", index);
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(item.Factory))
                    {
                        errorMsg.AppendFormat("第{0}行厂家不能为空！", index);
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(item.BatchNumber))
                    {
                        errorMsg.AppendFormat("第{0}行炉批号不能为空！", index);
                        continue;
                    }
                    //item.MaterialCode.Trim();
                    //item.MaterialName.Trim();
                    item.SpecificationModel.Trim();
                    item.StorageCode.Trim();
                    item.BranchCode.Trim();
                    item.WorkAreaCode.Trim();
                    //item.SiteCode.Trim();
                    //item.Factory.Trim();
                    //item.BatchNumber.Trim();
                    //var iindex = 1;
                    //var flag = false;
                    //foreach (var tep in dataList)
                    //{
                    //    flag = false;
                    //    iindex++;
                    //    if (item.SpecificationModel == tep.SpecificationModel
                    //        && item.StorageCode == tep.StorageCode
                    //        && item.BranchCode == tep.BranchCode
                    //        && item.WorkAreaCode == tep.WorkAreaCode
                    //        && item.SiteCode == tep.SiteCode
                    //        && item.Factory == tep.Factory
                    //        && item.BatchNumber == tep.BatchNumber
                    //        && index != iindex 
                    //        && index < iindex)
                    //    {
                    //        errorMsg.AppendFormat("第{0}行与第{1}行信息重复！", index, iindex);
                    //        break;
                    //    }
                    //    flag = true;
                    //}
                    //if (!flag)
                    //{
                    //    continue;
                    //}
                    item.IndexNum = index;
                    item.ProjectId = keyValue;
                    item.LockCount = 0;
                    item.HistoryCount = item.Count;
                    item.ChackState = 1;
                    retList.Add(item);
                }
                if (retList.Count > 0)
                    return Content(_rawMaterialStock.Input(retList, errorMsg).ToJson());
                else
                    return Error(errorMsg.ToString());
            }
            catch (Exception ex)
            {
                return Error(ex.ToString());
            }
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
            var data = _rawMaterialStock.AnyInfo(keyValue);
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
            //材料名称、规格型号、钢筋类型、计量单位、库存重量（kg）、动态库存重量合计（kg）、分部名称、工区名称、仓库名称
            //MaterialName,SpecificationModel,RebarTypeText,MeasurementUnitText,SurplusNumber,PassCount,BranchName,WorkAreaName,StorageName
            //导出数据列
            Dictionary<string, string> cellheader = new Dictionary<string, string> { 
                    { "MaterialName", "材料名称" }, 
                    { "SpecificationModel", "规格型号" }, 
                    { "RebarTypeText", "钢筋类型" }, 
                    { "MeasurementUnitText", "计量单位" }, 
                    { "SurplusNumber", "库存重量(kg)" }, 
                    { "PassCount", "动态库存重量合计(kg)" }, 
                    { "BranchName", "分部名称" }, 
                    { "WorkAreaName", "工区名称" }, 
                    { "StorageName", "仓库名称" }, 
                };
            var request = JsonEx.JsonToObj<RawMaterialStockRequest>(jsonData);
            var data = _rawMaterialStock.GetExportList(request);
            string hzzfc = "";
            var fileStream = ExcelHelper.ExportToMemoryStream(cellheader, data, "", "", "原材料库存", hzzfc);
            return File(fileStream, "application/vnd.ms-excel", "原材料库存.xls");
        }

        #endregion


        /// <summary>
        /// 获取数据列表(原材料)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetMaterialGridJson(RawMaterialStockRequest request)
        {
            var data = _rawMaterialStock.GetMaterialDataList(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取数据列表(仓库)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetStorageGridJson(RawMaterialStockRequest request)
        {
            var data = _rawMaterialStock.GetStorageDataList(request);
            return Content(data.ToJson());
        }
    }
}
