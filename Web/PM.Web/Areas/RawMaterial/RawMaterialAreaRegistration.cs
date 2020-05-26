using System.Web.Mvc;

namespace PM.Web.Areas.RawMaterial
{
    public class RawMaterialAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "RawMaterial";
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
