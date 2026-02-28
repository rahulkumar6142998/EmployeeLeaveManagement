using EmployeeLeaveManagement.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EmployeeLeaveManagement.Controllers
{
    public class BaseController : Controller
    {
        protected int? CurrentUserId => SessionHelper.GetUserId(HttpContext.Session);
        protected string? CurrentUserName => SessionHelper.GetUserName(HttpContext.Session);
        protected string? CurrentUserEmail => SessionHelper.GetUserEmail(HttpContext.Session);
        protected string? CurrentUserRole => SessionHelper.GetUserRole(HttpContext.Session);
        protected bool IsAdmin => SessionHelper.IsAdmin(HttpContext.Session);
        protected bool IsEmployee => SessionHelper.IsEmployee(HttpContext.Session);

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
            base.OnActionExecuting(context);
        }
    }
}