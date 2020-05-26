using PM.Business.RawMaterial;
using PM.Common;
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
    /// 余料库存
    /// </summary>
    [HandlerLogin]
    public class CloutStockController : BaseController
    {
        private readonly CloutStockLogic _cloutStock = new CloutStockLogic();

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
        public ActionResult GetGridJson(CloutStockRequest request)
        {
            var data = _cloutStock.GetDataListForPage(request);
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
            var data = _cloutStock.FindEntity(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitForm(TbCloutStock param, string type)
        {
            try
            {
                param.Weight = Convert.ToDecimal(Convert.ToDecimal(param.MeasurementUnitZl * param.Size * param.Number).ToString("f5"));
                if (type == "add")
                {
                    var data = _cloutStock.Insert(param);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _cloutStock.Update(param);
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
            var data = _cloutStock.Delete(keyValue);
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
                //string factoryCode = string.Empty;
                //if (base.CurrentUser.OrgType == "1")
                //{
                //    factoryCode = base.CurrentUser.CompanyId;
                //}
                //if (string.IsNullOrWhiteSpace(factoryCode))
                //{
                //    errorMsg.Append("加工厂信息错误");
                //    return Error(errorMsg.ToString());
                //}
                if (string.IsNullOrWhiteSpace(keyValue))
                {
                    errorMsg.Append("项目信息错误");
                    return Error(errorMsg.ToString());
                }
                var fostFile = Request.Files;
                Stream streamfile = fostFile[0].InputStream;
                string exName = Path.GetExtension(fostFile[0].FileName); //得到扩展名
                Dictionary<string, string> cellheader = new Dictionary<string, string> {
                    { "ProcessFactoryCode", "加工厂" },
                    { "SiteCode", "站点" },
                    { "SpecificationModel", "规格" },
                    { "Size", "尺寸" }, 
                    { "Number", "数量" }, 
                    { "Factory", "厂家" }, 
                    { "BatchNumber", "炉批号" }, 
                };
                var dataList = ExcelHelper.ExcelToEntityList<TbCloutStock>(cellheader, streamfile, exName, out errorMsg);
                var retList = new List<TbCloutStock>();
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
                    //string a = typeof(OperationEnum.MaterialType).GetEnumName(OperationEnum.MaterialType.圆钢);
                    //string b = typeof(OperationEnum.MaterialType).GetEnumName(OperationEnum.MaterialType.螺纹钢);
                    //if (item.MaterialName != a && item.MaterialName != b)
                    //{
                    //    errorMsg.AppendFormat("第{0}行原材料名称错误！", index);
                    //    continue;
                    //}
                    if (string.IsNullOrWhiteSpace(item.SpecificationModel))
                    {
                        errorMsg.AppendFormat("第{0}行规格不能为空！", index);
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(item.ProcessFactoryCode))
                    {
                        errorMsg.AppendFormat("第{0}行加工厂不能为空！", index);
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(item.SiteCode))
                    {
                        errorMsg.AppendFormat("第{0}行站点不能为空！", index);
                        continue;
                    }
                    if (item.Size <= 0)
                    {
                        errorMsg.AppendFormat("第{0}行尺寸不能为空！", index);
                        continue;
                    }
                    if (item.Number <= 0)
                    {
                        errorMsg.AppendFormat("第{0}行数量不能为空！", index);
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
                    item.SpecificationModel.Trim();
                    //item.SiteCode.Trim();
                    var iindex = 1;
                    var flag = false;
                    foreach (var tep in dataList)
                    {
                        flag = false;
                        iindex++;
                        if (item.SpecificationModel == tep.SpecificationModel
                            && item.SiteCode == tep.SiteCode
                            && item.Factory == tep.Factory
                            && item.BatchNumber == tep.BatchNumber
                            && item.Size == tep.Size
                            //&& item.Number == tep.Number
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
                    item.IndexNum = index;
                    item.ProjectId = keyValue;
                    //item.ProcessFactoryCode = factoryCode;
                    retList.Add(item);
                }
                if (retList.Count > 0)
                    return Content(_cloutStock.Input(retList, errorMsg,keyValue).ToJson());
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
            var data = _cloutStock.AnyInfo(keyValue);
            return Content(data.ToJson());
        }

        #endregion

        #region 余料流向

        public ActionResult CloutStockPlace()
        {
            return View();
        }

        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GeCloutStockPlace(CloutStockRequest request)
        {
            var data = _cloutStock.GetCloutStockPlaceForPage(request);
            return Content(data.ToJson());
        }

        #endregion

        /// <summary>
        /// 获取分页列表数据（余料）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetPlanYLGridJson(CloutStockRequest request)
        {
            var data = _cloutStock.GetDataListForPlanYL(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取分页列表数据（原材料）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>  
        [HttpPost]
        public ActionResult GetPlanYCLGridJson(CloutStockRequest request)
        {
            var data = _cloutStock.GetDataListForPlanYCL(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取分页列表数据（原材料-加工订单）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>  
        [HttpPost]
        public ActionResult GetOrderYCLGridJson(CloutStockRequest request)
        {
            var data = _cloutStock.GetDataListForOrderYCL(request);
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
            //所属项目、站点、加工厂名称、原材料编号、原材料名称、规格、尺寸（m、㎡）、单位重量（kg/m、㎡）、库存数量（根）、可用数量（根）、重量（kg）、厂家、炉批号
            //ProjectName,SiteName,ProcessFactoryName,MaterialCode,MaterialName,SpecificationModel,Size,MeasurementUnitZl,Number,UseNumber,Weight,Factory,BatchNumber
            //导出数据列
            Dictionary<string, string> cellheader = new Dictionary<string, string> { 
                    { "ProjectName", "所属项目" }, 
                    { "SiteName", "站点" }, 
                    { "ProcessFactoryName", "加工厂名称" }, 
                    { "MaterialCode", "原材料编号" }, 
                    { "MaterialName", "原材料名称" }, 
                    { "SpecificationModel", "规格" }, 
                    { "Size", "尺寸（m、㎡）" }, 
                    { "MeasurementUnitZl", "单位重量（kg/m、㎡）" }, 
                    { "Number", "库存数量（根）" }, 
                    { "UseNumber", "可用数量（根）" }, 
                    { "Weight", "重量（kg）" }, 
                    { "Factory", "厂家" }, 
                    { "BatchNumber", "炉批号" }, 
                };
            var request = JsonEx.JsonToObj<CloutStockRequest>(jsonData);
            var data = _cloutStock.GetExportList(request);
            decimal hj = 0;
            if (data.Rows.Count > 0)
            {
                hj = Convert.ToDecimal(data.Compute("sum(Weight)", "true"));
            }
            string hzzfc = "合计(KG):" + hj;
            var fileStream = ExcelHelper.ExportToMemoryStream(cellheader, data, "", "", "余料库存", hzzfc);
            return File(fileStream, "application/vnd.ms-excel", "余料库存.xls");
        }

        #endregion

        #region 报表
        /// <summary>
        /// 订单状态量展示
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetCloutStockTotalReport(CloutStockReportRequest request)
        {
            var data = _cloutStock.GetCloutStockTotalReport(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 余料产生及使用量统计
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetCloutStockUseReport(CloutStockReportRequest request)
        {
            var data = _cloutStock.GetCloutStockUseReport(request);
            return Content(data.ToJson());
        }

        #endregion
    }
}
