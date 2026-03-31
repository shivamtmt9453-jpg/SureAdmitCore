using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace SureAdmitCore.Areas.Admin.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Get user role from claims instead of session
            string userRole = User.FindFirstValue(ClaimTypes.Role);

            // Make it available in views
            ViewBag.UserRole = userRole;

            base.OnActionExecuting(filterContext);
        }
    }
}
