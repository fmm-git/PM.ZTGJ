using PM.Business.EarlyWarning;
using PM.Common;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.EarlyWarning.Controllers
{
    public class FormEarlyWarningBegTimeController : Controller
    {
        //
        // 预警开始时间设置
        private readonly TbFormEarlyWarningBegTimeLogic _fewBegTimeLogic = new TbFormEarlyWarningBegTimeLogic();

        #region 预警开始时间列表
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 获取分页列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult GetGridJson(PageSearchRequest request, string MenuCode)
        {
            var data = _fewBegTimeLogic.GetDataListForPage(request,MenuCode);
            return Content(data.ToJson());
        }
        #endregion

        #region 预警开始时间新增、编辑
        public ActionResult Form(string MenuCode)
        {
            ViewBag.MenuCode = MenuCode;
            ViewBag.ProjectId = OperatorProvider.Provider.CurrentUser.ProjectId;
            return View();
        }
        /// <summary>
        /// 编辑/查看页获取数据
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult GetFormJson(int keyValue)
        {
            var data = _fewBegTimeLogic.FindEntity(keyValue);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitForm(TbFormEarlyWarningBegTime model, string type)
        {
            try
            {
                if (type == "add")
                {
                    var data = _fewBegTimeLogic.Insert(model);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _fewBegTimeLogic.Update(model);
                    return Content(data.ToJson());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion 

        #region 预警开始时间删除

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteForm(int keyValue)
        {
            var data = _fewBegTimeLogic.Delete(keyValue);
            return Content(data.ToJson());
        }

        #endregion

        #region 获取菜单
        public ActionResult GetMenu()
        {
            var data = _fewBegTimeLogic.GetMenu();
            return Content(data.ToJson());
        }
        #endregion
    }
}
