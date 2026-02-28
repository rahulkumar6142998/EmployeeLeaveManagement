using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EmployeeLeaveManagement.Helpers
{
    public class EmployeeOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;

            if (!SessionHelper.IsLoggedIn(session))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
            else if (!SessionHelper.IsEmployee(session))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }

            base.OnActionExecuting(context);
        }
    }
}