using PM.Business.Safe;
using PM.Common.Extension;
using PM.Common.Helper;
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
    /// <summary>
    /// 临时用电检查
    /// </summary>
    [HandlerLogin]
    public class InterimUseElectricCheckController : BaseController
    {
        private readonly TbInterimUseElectricCheckLogic _iueclogic = new TbInterimUseElectricCheckLogic();

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
        public ActionResult GetGridJson(InterimUseElectricCheckRequest request)
        {
            var data = _iueclogic.GetDataListForPage(request);
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
                ViewBag.CheckCode = CreateCode.GetTableMaxCode("IUEC", "CheckCode", "TbInterimUseElectricCheck");
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
        public ActionResult GetFormJson(int keyValue)
        {
            var data = _iueclogic.FindEntity(keyValue);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 新增、修改数据提交
        /// </summary>
        /// <param name="request"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitForm(TbInterimUseElectricCheck model, string type)
        {
            try
            {
                if (type == "add")
                {
                    model.InsertUserCode = base.UserCode;
                    var data = _iueclogic.Insert(model);
                    return Content(data.ToJson());
                }
                else
                {
                    var data = _iueclogic.Update(model);
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
            var data = _iueclogic.Delete(keyValue);
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
            var data = _iueclogic.AnyInfo(keyValue);
            return Content(data.ToJson());
        }

        #endregion

        #region 导出

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="ProjectCode"></param>
        /// <returns></returns>
        public ActionResult OutputExcel(string jsonData)
        {
            //检查编号、加工厂名称、外电防护与配电线路、接地与防雷、配电室与自备电源、配电箱与开关箱、检查人、检查时间、问题描述
            //CheckCode,ProcessFactoryName,epod,gorlp,drorps,dorsb,CheckUserNames,CheckDate,ProblemDescription
            //导出数据列
            Dictionary<string, string> cellheader = new Dictionary<string, string> { 
                    { "CheckCode", "检查编号" }, 
                    { "ProcessFactoryName", "加工厂名称" }, 
                    { "epod", "外电防护与配电线路" }, 
                    { "gorlp", "接地与防雷" }, 
                    { "drorps", "配电室与自备电源" }, 
                    { "dorsb", "配电箱与开关箱" },
                    { "CheckUserNames", "检查人" },
                    { "CheckDate", "检查时间" },
                    { "ProblemDescription", "问题描述" },
                };
            var request = JsonEx.JsonToObj<InterimUseElectricCheckRequest>(jsonData);
            var data = _iueclogic.GetExportList(request);
            var fileStream = ExcelHelper.ExportToMemoryStream(cellheader, data, "", "", "临时用电检查", "");
            return File(fileStream, "application/vnd.ms-excel", "临时用电检查.xls");
        }

        #endregion

    }
}
