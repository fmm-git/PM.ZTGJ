using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PM.Web.Areas.Flow
{
    public class FlowAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Flow";
            }
        }
        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Flow_default",
                "Flow/{controller}/{action}/{id}",
                new { area = this.AreaName, controller = "Home", action = "Default", id = UrlParameter.Optional },
                new string[] { "PM.Web.Areas." + this.AreaName + ".Controllers" }
            );
        }
    }
}