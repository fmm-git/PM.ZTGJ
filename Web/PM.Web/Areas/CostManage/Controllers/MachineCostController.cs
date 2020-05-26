using PM.Business.CostManage;
using PM.Common.Extension;
using PM.DataEntity;
using PM.DataEntity.CostManage.ViewModel;
using PM.Domain.WebBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.CostManage.Controllers
{
    /// <summary>
    /// 机械费用核算
    /// </summary>
    [HandlerLogin]
    public class MachineCostController : BaseController
    {
        private readonly MachineCostLogic _machineCost = new MachineCostLogic();

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
        public ActionResult GetGridJson(MachineCostRequest request)
        {
            var data = _machineCost.GetDataListForPage(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取分页列表数据（明细）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetItemGridJson(MachineCostRequest request)
        {
            var data = _machineCost.GetItemGridJson(request);
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
                ViewBag.CheckCode = CreateCode.GetTableMaxCode("JXFY", "CheckCode", "TbMachineCost");
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
            var data = _machineCost.FindEntity(keyValue);
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
                var inOrderModel = JsonEx.JsonToObj<TbMachineCost>(model);
                var inOrderItem = JsonEx.JsonToObj<List<TbMachineCostItem>>(itemModel);
                if (type == "add")
                {
                    var data = _machineCost.Insert(inOrderModel, inOrderItem);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _machineCost.Update(inOrderModel, inOrderItem);
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
            var data = _machineCost.Delete(keyValue);
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
            var data = _machineCost.AnyInfo(keyValue);
            return Content(data.ToJson());
        }

        #endregion


        /// <summary>
        /// 获取数据列表(设备信息)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetEquipmentRegisterList(MachineCostRequest request)
        {
            var data = _machineCost.GetEquipmentRegisterList(request);
            return Content(data.ToJson());
        }
    }
}
