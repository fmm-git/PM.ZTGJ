using System.Web.Mvc;

namespace PM.Web.Areas.WorkFlow
{
    public class WorkFlowAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "WorkFlow";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "WorkFlow_default",
                "WorkFlow/{controller}/{action}/{id}",
                new { area = this.AreaName, controller = "Home", action = "Default", id = UrlParameter.Optional },
                new string[] { "PM.Web.Areas." + this.AreaName + ".Controllers" }
            );
        }
    }
}
