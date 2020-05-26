using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.DataManage
{
    public class DataManageAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "DataManage";
            }
        }
        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
             this.AreaName + "_Default",
             this.AreaName + "/{controller}/{action}/{id}",
             new { area = this.AreaName, controller = "Home", action = "Index", id = UrlParameter.Optional },
             new string[] { "PM.Web.Areas." + this.AreaName + ".Controllers" }
           );
        }
    }
}