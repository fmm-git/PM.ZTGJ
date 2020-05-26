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
    /// 配送费用申报
    /// </summary>
    [HandlerLogin]
    public class DistributionDeclareController : BaseController
    {

        private readonly DistributionDeclareLogic _distributionDeclare = new DistributionDeclareLogic();

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
        public ActionResult GetGridJson(DistributionDeclareRequest request)
        {
            var data = _distributionDeclare.GetDataListForPage(request);
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
                ViewBag.UserCode = base.UserCode;
                ViewBag.DistributionDeclareCode = CreateCode.GetTableMaxCode("PSFY", "DistributionDeclareCode", "TbDistributionDeclare");
                string factoryCode = "", factoryName="";
                if (base.CurrentUser.OrgType == "1")
                {
                    factoryCode = base.CurrentUser.CompanyId;
                    factoryName = base.CurrentUser.ComPanyName;
                }
                ViewBag.FactoryCode = factoryCode;
                ViewBag.FactoryName = factoryName;
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
            var data = _distributionDeclare.FindEntity(keyValue);
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
                var inOrderModel = JsonEx.JsonToObj<TbDistributionDeclare>(model);
                var inOrderItem = JsonEx.JsonToObj<List<TbDistributionDeclareItem>>(itemModel);
                if (type == "add")
                {
                    var data = _distributionDeclare.Insert(inOrderModel, inOrderItem);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _distributionDeclare.Update(inOrderModel, inOrderItem);
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
            var data = _distributionDeclare.Delete(keyValue);
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
            var data = _distributionDeclare.AnyInfo(keyValue);
            return Content(data.ToJson());
        }

        #endregion
    }
}
