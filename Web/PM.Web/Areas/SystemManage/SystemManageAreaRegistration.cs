using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.SystemManage
{
    public class SystemManageAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "SystemManage";
            }
        }
        public override void RegisterArea(AreaRegistrationContext context)
        {
            //context.MapRoute(
            //    "SystemManage_default",
            //    "SystemManage/{controller}/{action}/{id}",
            //    new { action = "Default", id = UrlParameter.Optional }
            //);
            context.MapRoute(
             this.AreaName + "_Default",
             this.AreaName + "/{controller}/{action}/{id}",
             new { area = this.AreaName, controller = "Home", action = "Index", id = UrlParameter.Optional },
             new string[] { "PM.Web.Areas." + this.AreaName + ".Controllers" }
           );
        }
    }
}