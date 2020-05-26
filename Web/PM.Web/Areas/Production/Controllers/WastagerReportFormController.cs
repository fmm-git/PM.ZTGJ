using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PM.Business.Production;
using PM.Common.Extension;
using PM.DataEntity;
using PM.DataEntity.Production.ViewModel;

namespace PM.Web.Areas.Production.Controllers
{
    /// <summary>
    /// 耗材管理
    /// </summary>
    [HandlerLogin]
    public class WastagerReportFormController : BaseController
    {
        // GET: /Production/WastagerReportForm/
        private readonly WastagerReportFormLogic _wasRFLogic = new WastagerReportFormLogic();
        #region 视图
        
        
        public ActionResult Index()
        {
            return View();
        }

        [HandlerLogin(Ignore = false)]
        public ActionResult Details()
        {
            return View();
        }

        public ActionResult Form(string type)
        {
            if (type == "add")
            {
                ViewBag.UserName = base.UserName;
                ViewBag.UserCode = base.UserCode;
            }
            return View();
        }
        #endregion

        #region

        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetGridJson(WastagerReportFormRequest request)
        {
            var data = _wasRFLogic.GetDataListForPage(request);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 编辑/查看页获取数据
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult GetFormJson(int keyValue)
        {
            var data = _wasRFLogic.FindEntity(keyValue);
            return Content(data.ToJson());
        }


         /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="model">主表信息</param>
        /// <param name="itemModel">明细信息</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public ActionResult SubmitForm(TbWastagerReportForm model, string type)
        {
            try
            {
                //var workOrderModel = JsonEx.JsonToObj<TbWastagerReportForm>(model);
                if (type == "add")
                {
                    var data = _wasRFLogic.Insert(model);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _wasRFLogic.Update(model);
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
        
        public ActionResult DeleteForm(int keyValue)
        {
            var data = _wasRFLogic.Delete(keyValue);
            return Content(data.ToJson());
        }

        #endregion

    }
}
