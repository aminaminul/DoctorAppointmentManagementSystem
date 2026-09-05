using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace DoctorAppointmentManagementSystem.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _allowedRoles;

        public AuthorizeRoleAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var effectiveAttr = context.ActionDescriptor.EndpointMetadata.OfType<AuthorizeRoleAttribute>().LastOrDefault();
            if (effectiveAttr != null && effectiveAttr != this)
            {
                base.OnActionExecuting(context);
                return;
            }

            var session = context.HttpContext.Session;
            var userId = session.GetInt32("UserId");
            var userRole = session.GetString("UserRole");

            if (userId == null || string.IsNullOrEmpty(userRole))
            {
                var controller = context.Controller as Controller;
                if (controller != null)
                {
                    controller.TempData["Error"] = "Please log in to access this area.";
                }

                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (_allowedRoles != null && _allowedRoles.Length > 0)
            {
                bool isAuthorized = _allowedRoles.Any(r => string.Equals(r, userRole, StringComparison.OrdinalIgnoreCase));
                if (!isAuthorized)
                {
                    var controller = context.Controller as Controller;
                    if (controller != null)
                    {
                        controller.TempData["Error"] = "Access restricted. You have been redirected to your authorized portal.";
                    }

                    // Safely redirect to user's own portal
                    if (string.Equals(userRole, "Doctor", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Result = new RedirectToActionResult("Dashboard", "Doctor", null);
                    }
                    else if (string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Result = new RedirectToActionResult("Dashboard", "Admin", null);
                    }
                    else
                    {
                        context.Result = new RedirectToActionResult("Dashboard", "Patient", null);
                    }
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
