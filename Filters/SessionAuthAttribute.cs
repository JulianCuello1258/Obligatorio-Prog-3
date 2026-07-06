using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BeeKeeperApp.Filters
{
    public class SessionAuthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session.GetString("IsAuthenticated");
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();

            if (string.IsNullOrEmpty(session) && controller != "Account")
            {
                // For AJAX / API requests return 401 so the client can handle it gracefully
                // instead of following a redirect to the login HTML page.
                var acceptsJson = context.HttpContext.Request.Headers["Accept"]
                    .ToString().Contains("application/json", System.StringComparison.OrdinalIgnoreCase);
                var isAjax = context.HttpContext.Request.Headers["X-Requested-With"]
                    .ToString().Equals("XMLHttpRequest", System.StringComparison.OrdinalIgnoreCase);

                if (acceptsJson || isAjax)
                {
                    context.Result = new JsonResult(new { message = "Sesión expirada" })
                    {
                        StatusCode = 401
                    };
                }
                else
                {
                    context.Result = new RedirectToActionResult("Login", "Account", null);
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
