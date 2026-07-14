using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DoctorAppointmentManagementSystem.Data;
using System.Linq;

namespace DoctorAppointmentManagementSystem.ViewComponents
{
    public class NavbarViewComponent : ViewComponent
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _db;

        public NavbarViewComponent(IHttpContextAccessor httpContextAccessor, ApplicationDbContext db)
        {
            _httpContextAccessor = httpContextAccessor;
            _db = db;
        }

        public IViewComponentResult Invoke()
        {
            var rd = _httpContextAccessor.HttpContext?.Request?.RouteValues;
            string controller = rd?["controller"]?.ToString() ?? string.Empty;
            string action = rd?["action"]?.ToString() ?? string.Empty;
            ViewData["controller"] = controller;
            ViewData["action"] = action;

            // Prefer session values (set at login / register)
            var http = _httpContextAccessor.HttpContext;
            var session = http?.Session;
            var userName = session?.GetString("UserName");
            var userRole = session?.GetString("UserRole");

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userRole))
            {
                // fallback to first user (legacy) — keep behavior but safer
                var user = _db.Users.FirstOrDefault();
                if (user != null)
                {
                    userName = user.Username ?? user.Email;
                    var role = _db.Roles.FirstOrDefault(r => r.Id == user.RoleId);
                    userRole = role?.RoleName ?? "";
                }
            }

            ViewData["UserRole"] = (userRole ?? "").ToLowerInvariant();
            ViewData["UserName"] = userName ?? "";

            return View("Default");
        }
    }
}
