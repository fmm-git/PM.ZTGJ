using PM.Business.OA;
using PM.Common.Extension;
using PM.DataEntity;
using PM.Domain.WebBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.OA.Controllers
{
    public class EarlyWarningNewsController : Controller
    {
        //
        // 预警类消息设置
        private readonly EarlyWarningNews _earlyWarningNews = new EarlyWarningNews();
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 新增、修改
        /// </summary>
        /// <returns></returns>
        public ActionResult Form(string type) 
        {
            if (type == "add")
            {
                ViewBag.EarlyWarningNewsCode = CreateCode.GetTableMaxCode("YJXX", "EarlyWarningNewsCode", "TbEarlyWarningSetUp");
            }
            return View();
        }

        #region 新增、修改

        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitForm(string model, string type)
        {
            try
            {
                var inOrderModel = JsonEx.JsonToObj<TbEarlyWarningSetUp>(model);
                if (type == "add")
                {
                    var data = _earlyWarningNews.Insert(inOrderModel);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _earlyWarningNews.Update(inOrderModel);
                    return Content(data.ToJson());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region 列表信息

        public ActionResult GetEarlyWarningNewsList(string MenuCode="") 
        {
            var Data = _earlyWarningNews.GetEarlyWarningNewsList(MenuCode);
            return Content(Data.ToJson());
        }
        public ActionResult GetEarlyWarningNewsList2(string OrgType = "", string DeptId = "", string RoleId = "", string PersonnelSource = "", string PersonnelCode = "", string ProjectId = "", int NewsType=2) 
        {
            var Data = _earlyWarningNews.GetEarlyWarningNewsList2(OrgType, DeptId, RoleId, PersonnelSource, PersonnelCode, ProjectId,NewsType);
            return Content(Data.ToJson());
        }
        public ActionResult GetFormJson(int keyValue) 
        {
            var Data = _earlyWarningNews.FindEntity(keyValue);
            return Content(Data.ToJson());
        }
        #endregion
    }
}
