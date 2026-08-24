using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DoctorAppointmentManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
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

            // Auto-login from Remember Me cookie if Session expired
            if (!userId.HasValue && http != null && http.Request.Cookies.TryGetValue("dams_remember_user", out string? savedUserIdStr))
            {
                if (int.TryParse(savedUserIdStr, out int savedUserId))
                {
                    var user = _db.Users.Include(u => u.Role).FirstOrDefault(u => u.Id == savedUserId && u.ActiveStatus);
                    if (user != null)
                    {
                        var role = user.Role ?? _db.Roles.FirstOrDefault(r => r.Id == user.RoleId);
                        string roleName = role?.Name ?? "Patient";
                        int virtualRoleId = roleName.ToLower() switch
                        {
                            "admin" => 1,
                            "doctor" => 2,
                            "patient" => 3,
                            _ => 3
                        };

                        session?.SetInt32("UserId", user.Id);
                        session?.SetInt32("RoleId", virtualRoleId);
                        session?.SetString("UserRole", roleName);
                        session?.SetString("UserName", user.Username ?? user.Email);

                        userId = user.Id;
                        userName = user.Username ?? user.Email;
                        userRole = roleName;
                    }
                }
            }

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
