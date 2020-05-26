using PM.Business.Safe;
using PM.Common.Extension;
using PM.DataEntity;
using PM.DataEntity.Safe.ViewModel;
using PM.Domain.WebBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.Safe.Controllers
{
    [HandlerLogin]
    public class HoistingSafetyController : BaseController
    {
        //
        // 吊装安全

        private readonly HoistingSafetyLogic _hoistSafetyLogic = new HoistingSafetyLogic();
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
        public ActionResult GetGridJson(HoistingSafetyRequest request)
        {
            var data = _hoistSafetyLogic.GetDataListForPage(request);
            return Content(data.ToJson());
        }
        #endregion

        #region 新增、修改、查看
        public ActionResult Form(string type)
        {
            if (type == "add")
            {
                ViewBag.HoistingCode = CreateCode.GetTableMaxCode("HS", "HoistingCode", "TbHoistingSafety");
                string factoryCode = "", factoryName = "";
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
        public ActionResult GetFormJson(int keyValue)
        {
            var data = _hoistSafetyLogic.FindEntity(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitForm(TbHoistingSafety model, string type)
        {
            try
            {
                if (type == "add")
                {
                    var data = _hoistSafetyLogic.Insert(model);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _hoistSafetyLogic.Update(model);
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
            var data = _hoistSafetyLogic.Delete(keyValue);
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
            var data = _hoistSafetyLogic.AnyInfo(keyValue);
            return Content(data.ToJson());
        }

        #endregion

    }
}
