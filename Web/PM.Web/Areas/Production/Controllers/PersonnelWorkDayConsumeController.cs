using PM.Business.Production;
using PM.Common.Extension;
using PM.DataEntity;
using PM.DataEntity.Production.ViewModel;
using PM.Domain.WebBase;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.Production.Controllers
{
    // 人员工日消耗
    [HandlerLogin]
    public class PersonnelWorkDayConsumeController : BaseController
    {
        //人员工日消耗逻辑处理层类
        private readonly PersonnelWorkDayConsumeLogic _pwdcBus = new PersonnelWorkDayConsumeLogic();

        // GET: /Production/PersonnelWorkDayConsume/

        #region 视图

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
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
                ViewBag.ConsumeCode = CreateCode.GetTableMaxCode("GRXH", "ConsumeCode", "TbPersonnelWorkDayConsume");
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
        /// 首页 查询全部信息
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult GetAllOrBySearch(FPiCiXQPlan entity) 
        {
            var data = _pwdcBus.GetAllOrBySearch(entity);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 以ID查询人员工日消耗
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>

        [HandlerLogin(Ignore = false)]
        public ActionResult GetFormJson(int keyValue)
        {
            var data = _pwdcBus.GetFormJson(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 加工订单弹窗
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetJGDDGridJson(WorkOrderRequest request)
        {
            var data = _pwdcBus.GetJGDDGridJson(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 新增明细弹窗
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult GetOrderItemGridJson(WorkOrderRequest request, string keyValue, string OrderCode) 
        {
            var data = _pwdcBus.GetOrderItemGridJson(request, keyValue, OrderCode);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult AnyInfo(int keyValue)
        {
            var data = _pwdcBus.AnyInfo(keyValue);
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
                var PlanModel = JsonEx.JsonToObj<TbPersonnelWorkDayConsume>(model);
                var PlanItem = JsonEx.JsonToObj<List<TbPersonnelWorkDayConsumeItem>>(itemModel);
                if (type == "add")
                {
                    var data = _pwdcBus.Insert(PlanModel, PlanItem);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _pwdcBus.Update(PlanModel, PlanItem);
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
            var data = _pwdcBus.Delete(keyValue);
            return Content(data.ToJson());
        }

        #endregion
    }
}
