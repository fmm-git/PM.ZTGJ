using PM.Business.RawMaterial;
using PM.Common.Extension;
using PM.Common.Helper;
using PM.DataEntity;
using PM.Domain.WebBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.RawMaterial.Controllers
{
    /// <summary>
    /// 车辆管理
    /// </summary>
    [HandlerLogin]
    public class CarInfoController : BaseController
    {
        private readonly CarInfoLogic _carInfo = new CarInfoLogic();

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
        public ActionResult GetGridJson(CarInfoRequest request)
        {
            var data = _carInfo.GetDataListForPage(request);
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
                ViewBag.CarCode = CreateCode.GetTableMaxCode("CAR", "CarCode", "TbCarInfo");
            }
            return View();
        }

        /// <summary>
        /// 查看页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Details()
        {
            return View();
        }

        /// <summary>
        /// 编辑/查看页获取数据
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult GetFormJson(int keyValue)
        {
            var data = _carInfo.FindEntity(keyValue);
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
                var carInfoModel = JsonEx.JsonToObj<TbCarInfo>(model);
                var carInfoItem = JsonEx.JsonToObj<List<TbCarInfoDetail>>(itemModel);
                if (type == "add")
                {
                    var data = _carInfo.Insert(carInfoModel, carInfoItem);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _carInfo.Update(carInfoModel, carInfoItem);
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
            var data = _carInfo.Delete(keyValue);
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
            var data = _carInfo.AnyInfo(keyValue);
            return Content(data.ToJson());
        }

        #endregion

        /// <summary>
        /// 获取司机信息（只能选择加工厂通用人员）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetJgcTyUser(TbUserRequest request, string ProcessFactoryCode)
        {
            request.rows = 50;
            var data = _carInfo.GetJgcTyUser(request,ProcessFactoryCode);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取驾驶员信息(弹框)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetItemGridJson(CarInfoRequest request)
        {
            var data = _carInfo.GetDataItemListForPage(request);
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
            //车辆编号、车牌号、行驶证号、车辆型号、车辆使用状态、所属单位、所属加工厂
            //CarCode,CarCph,DrivingLicenseNum,CarXh,CarStartName,SupplierName,ProcessFactoryName
            //导出数据列
            Dictionary<string, string> cellheader = new Dictionary<string, string> { 
                    { "CarCode", "车辆编号" }, 
                    { "CarCph", "车牌号" }, 
                    { "DrivingLicenseNum", "行驶证号" }, 
                    { "CarXh", "车辆型号" }, 
                    { "CarStartName", "车辆使用状态" }, 
                    { "SupplierName", "所属单位" },
                    { "ProcessFactoryName", "所属加工厂" },
                };
            var request = JsonEx.JsonToObj<CarInfoRequest>(jsonData);
            var data = _carInfo.GetExportList(request);
            var fileStream = ExcelHelper.ExportToMemoryStream(cellheader, data, "", "", "车辆信息管理", "");
            return File(fileStream, "application/vnd.ms-excel", "车辆信息管理.xls");
        }
        #endregion
    }
}
