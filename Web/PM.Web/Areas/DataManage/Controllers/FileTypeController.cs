using PM.Business.DataManage;
using PM.Common;
using PM.Common.Extension;
using PM.DataEntity;
using PM.DataEntity.DataManage.ViewModel;
using PM.Domain.WebBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.DataManage.Controllers
{
    [HandlerLogin]
    public class FileTypeController : BaseController
    {
        private readonly FileTypeBuessic _File = new FileTypeBuessic();
        //
        // GET: /DataManage/FileType/

        //资料分类归档数据列表页

        #region 首页
        public ActionResult Index()
        {
            ViewBag.InsertUserCode = base.UserCode;
            ViewBag.InsertUserName = base.UserName;
            return View();
        }

        /// <summary>
        /// 列表数据查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetGridJson(TbDataManageRequest request)
        {
            var data = _File.GetGridJson(request);
            return Content(data.ToJson());
        }
        #endregion

        #region 添加/修改
        public ActionResult Form(string type) 
        {
            if (type == "add") {
                ViewBag.DataCode = CreateCode.GetTableMaxCode("ZLFL", "DataCode", "TbDataManage");
                ViewBag.InsertUserCode = base.UserCode;
                ViewBag.InsertUserName = base.UserName;
                var OrgType = OperatorProvider.Provider.CurrentUser.OrgType;
                if (OrgType == "1")
                {
                    ViewBag.ProcessFactoryCode = OperatorProvider.Provider.CurrentUser.CompanyId;
                    ViewBag.ProcessFactoryName = OperatorProvider.Provider.CurrentUser.ComPanyName;
                }
                ViewBag.OrgType = OrgType;
                
            }
            return View();
        }

        /// <summary>
        /// 查询部门信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ActionResult GetDepartmentCodeJson(string keyword) 
        {
            var data = _File.GetDepartmentCodeJson(keyword);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 查询当前登录人员下的加工厂信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ActionResult ProcessFactoryNameSelect(string UserCode) 
        {
            var data = _File.ProcessFactoryNameSelect(UserCode);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 添加/修改操作
        /// </summary>
        /// <param name="UserCode"></param>
        /// <returns></returns>
        public ActionResult SubmitForm(TbDataManage data,string type) 
        {
            try
            {
                if (type == "add")
                {
                    var data1 = _File.Insert(data);
                    return Content(data1.ToJson());
                }
                else
                {
                    var data1 = _File.Update(data);
                    return Content(data1.ToJson());
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
            var data = _File.DeleteForm(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 判断信息是否可操作
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult AnyInfo(int keyValue)
        {
            var data = _File.AnyInfo(keyValue);
            return Content(data.ToJson());
        }
        #endregion

        #region 查看页面
        [HandlerLogin(Ignore = false)]
        public ActionResult Details(int keyValue)
        {
            ViewBag.keyValue = keyValue;
            return View();
        }

        //资料信息查看

        [HandlerLogin(Ignore = false)]
        public ActionResult GetFormJson(int keyValue)
        {
            var data = _File.GetFormJson(keyValue);
            return Content(data.ToJson());
        }
        #endregion
    }
}
