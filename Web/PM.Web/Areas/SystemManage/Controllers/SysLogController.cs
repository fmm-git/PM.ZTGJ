using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PM.Business;
using PM.DataEntity;

namespace PM.Web.Areas.SystemManage.Controllers
{
    /// <summary>
    /// 系统日志
    /// </summary>
    public class SysLogController : BaseController
    {
        public readonly SysLogLogic si = new SysLogLogic();
        //
        // GET: /SystemManage/SysLog/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetAllLog(PageSearchRequest pr, string queryJson) 
        {
            var queryParam = queryJson.ToJObject();
            var data = new
            {
                rows = si.GetAllLog(pr, queryParam),
                total = pr.total,
                page = pr.page,
                records = pr.records
            };
            return Content(data.ToJson());
        }
    }
}
