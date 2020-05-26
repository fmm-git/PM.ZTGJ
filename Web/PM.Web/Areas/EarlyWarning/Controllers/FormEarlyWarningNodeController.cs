using PM.Business.EarlyWarning;
using PM.Common.Extension;
using PM.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.EarlyWarning.Controllers
{
    public class FormEarlyWarningNodeController : Controller
    {
        //
        // 预警节点设置
        private readonly TbFormEarlyWarningNodeLogic _fewNodeLogic = new TbFormEarlyWarningNodeLogic();

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
        public ActionResult GetGridJson(PageSearchRequest request, string EarlyWarningCode)
        {
            var data = _fewNodeLogic.GetDataListForPage(request, EarlyWarningCode);
            return Content(data.ToJson());
        }

        #endregion

        #region 新增、修改、查看
        public ActionResult Form(string MenuCode)
        {
            return View();
        }
        /// <summary>
        /// 选择预警人员
        /// </summary>
        /// <returns></returns>
        public ActionResult ChooseEarlyWarningUser()
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
            var data = _fewNodeLogic.FindEntity(keyValue);
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
                var ewNode = JsonEx.JsonToObj<TbFormEarlyWarningNode>(model);
                var ewNodeItem = JsonEx.JsonToObj<List<TbFormEarlyWarningNodePersonnel>>(itemModel);

                if (type == "add")
                {
                    var data = _fewNodeLogic.Insert(ewNode, ewNodeItem);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _fewNodeLogic.Update(ewNode, ewNodeItem);
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
            var data = _fewNodeLogic.Delete(keyValue);
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
            var data = _fewNodeLogic.AnyInfo(keyValue);
            return Content(data.ToJson());
        }

        #endregion

        #region 获取菜单
        public ActionResult GetMenu(PageSearchRequest request)
        {
            var data = _fewNodeLogic.GetMenu(request);
            return Content(data.ToJson());
        }

        #endregion
    }
}
