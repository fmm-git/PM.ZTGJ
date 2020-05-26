using PM.Business.RawMaterial;
using PM.Common;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataEntity;
using PM.DataEntity.RawMaterial.ViewModel;
using PM.Domain.WebBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.RawMaterial.Controllers
{
    /// <summary>
    /// 库存盘点
    /// </summary> 
    [HandlerLogin]
    public class StockTakingController : BaseController
    {
        private readonly StockTakingLogic _StockTaking = new StockTakingLogic();

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
        public ActionResult GetGridJson(StockTakingRequest request)
        {
            var data = _StockTaking.GetDataListForPage(request);
            return Content(data.ToJson());
        }

        #endregion

        #region 编辑

        /// <summary>
        /// 新增、修改页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Form(string type)
        {
            if (type == "add")
            {
                ViewBag.UserName = base.UserName;
                ViewBag.TakNum = CreateCode.GetTableMaxCode("PD", "TakNum", "TbStockTaking");
                ViewBag.ProcessFactoryCode = OperatorProvider.Provider.CurrentUser.ProcessFactoryCode;
            }
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
            var data = _StockTaking.FindEntity(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitForm(string model, string itemModel, string type)
        {
            try
            {
                var StockTakingModel = JsonEx.JsonToObj<TbStockTaking>(model);
                var StockTakingItem = JsonEx.JsonToObj<List<TbStockTakingItem>>(itemModel);
                if (type == "add")
                {
                    var data = _StockTaking.Insert(StockTakingModel, StockTakingItem);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _StockTaking.Update(StockTakingModel, StockTakingItem);
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
            var data = _StockTaking.Delete(keyValue);
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
            var data = _StockTaking.AnyInfo(keyValue);
            return Content(data.ToJson());
        }

        #endregion


        /// <summary>
        /// 获取明细表数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetItemData(string warehouseType, string factoryCode)
        {
            var data = _StockTaking.GetItemData(warehouseType, factoryCode);
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
            //盘点单编号、加工厂名称、盘点日期、仓库类型、账存总量（吨）、盘盈/盘亏（吨）,备注
            //TakNum,FactoryName,TakDay,WarehouseTypeName,TotalInventory,TotalEarnOrLos,Remarks
            //导出数据列
            Dictionary<string, string> cellheader = new Dictionary<string, string> { 
                    { "TakNum", "盘点单编号" }, 
                    { "FactoryName", "加工厂名称" }, 
                    { "TakDay", "盘点日期" }, 
                    { "WarehouseTypeName", "仓库类型" }, 
                    { "TotalInventory", "账存总量（吨）" }, 
                    { "TotalEarnOrLos", "盘盈/盘亏（吨）" },
                    { "Remarks", "备注" },
                };
            var request = JsonEx.JsonToObj<StockTakingRequest>(jsonData);
            var data = _StockTaking.GetExportList(request);
            var fileStream = ExcelHelper.ExportToMemoryStream(cellheader, data, "", "", "材料库存盘点", "");
            return File(fileStream, "application/vnd.ms-excel", "材料库存盘点.xls");
        }

        #endregion
    }
}
