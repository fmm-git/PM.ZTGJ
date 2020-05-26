using PM.Business.Distribution;
using PM.Common.Extension;
using PM.DataEntity;
using PM.DataEntity.Distribution.ViewModel;
using PM.Domain.WebBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.Distribution.Controllers
{
    [HandlerLogin]
    public class SiteDischargeCargoController : BaseController
    {
        //
        // 站点卸货

        private readonly SiteDischargeCargoLogic _siteDiscCargoLogic = new SiteDischargeCargoLogic();
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
        public ActionResult GetGridJson(SiteDischargeCargoRequest request)
        {
            var data = _siteDiscCargoLogic.GetDataListForPage(request);
            return Content(data.ToJson());
        }

        #endregion

        #region 新增、修改、查看
        public ActionResult Form(string type)
        {
            if (type == "add")
            {
                ViewBag.DischargeCargoCode = CreateCode.GetTableMaxCode("ZDXH", "DischargeCargoCode", "TbSiteDischargeCargo");
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
        public ActionResult GetFormJson(int keyValue)
        {
            var data = _siteDiscCargoLogic.FindEntity(keyValue);
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
                var siteDiscCargoModel = JsonEx.JsonToObj<TbSiteDischargeCargo>(model);
                var siteDiscCargoItem = JsonEx.JsonToObj<List<TbSiteDischargeCargoDetail>>(itemModel);

                if (type == "add")
                {
                    var data = _siteDiscCargoLogic.Insert(siteDiscCargoModel, siteDiscCargoItem);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _siteDiscCargoLogic.Update(siteDiscCargoModel, siteDiscCargoItem);
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
            var data = _siteDiscCargoLogic.Delete(keyValue);
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
            var data = _siteDiscCargoLogic.AnyInfo(keyValue);
            return Content(data.ToJson());
        }

        #endregion

        #region 获取配送装车列表数据
        /// <summary>
        /// 获取配送装车列表数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDistributionEntList(TbDistributionEntRequest request)
        {
            request.rows = 50;
            var data = _siteDiscCargoLogic.GetDistributionEntList(request);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取该配送单号下的明细信息
        /// </summary>
        /// <param name="DistributionCode">配送单号</param>
        /// <returns></returns>
        public ActionResult GetDistributionEntItemList(string DistributionCode) 
        {
            var data = _siteDiscCargoLogic.GetDistributionEntItemList(DistributionCode);
            return Content(data.ToJson());
        }
        #endregion

        #region 确认卸货

        /// <summary>
        /// 确认卸货
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public ActionResult DischargeCargoConfirm(int keyValue)
        {
            var data = _siteDiscCargoLogic.DischargeCargoConfirm(keyValue);
            return Content(data.ToJson());
        }

        #endregion

    }
}
