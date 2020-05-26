using System.Web.Mvc;

namespace PM.Web.Areas.Safe
{
    public class SafeAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Safe";
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
