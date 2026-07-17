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

            var http = _httpContextAccessor.HttpContext;
            var session = http?.Session;
            var userId = session?.GetInt32("UserId");
            var userName = session?.GetString("UserName");
            var userRole = session?.GetString("UserRole");

            bool isLoggedIn = userId.HasValue && !string.IsNullOrEmpty(userRole);

            if (!isLoggedIn)
            {
                userName = null;
                userRole = null;
            }

            ViewData["UserId"] = userId;
            ViewData["UserRole"] = isLoggedIn ? (userRole ?? "").ToLowerInvariant() : "";
            ViewData["UserName"] = userName ?? "";
            ViewData["IsLoggedIn"] = isLoggedIn;

            return View("Default");
        }
    }
}
