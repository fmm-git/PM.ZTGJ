using System.Web.Mvc;

namespace PM.Web.Areas.OA
{
    public class OAAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "OA";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "OA_default",
                "OA/{controller}/{action}/{id}",
                new { area = this.AreaName, controller = "Home", action = "Index", id = UrlParameter.Optional },
                new string[] { "PM.Web.Areas." + this.AreaName + ".Controllers" }
            );
        }
    }
}
