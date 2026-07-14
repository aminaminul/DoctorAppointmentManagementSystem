using Microsoft.AspNetCore.Mvc;
using DoctorAppointmentManagementSystem.Data;
using System.Linq;

namespace DoctorAppointmentManagementSystem.ViewComponents
{
    public class SidebarViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;
        public SidebarViewComponent(ApplicationDbContext db)
        {
            _db = db;
        }

        public IViewComponentResult Invoke()
        {
            var http = HttpContext;
            var session = http?.Session;
            var username = session?.GetString("UserName");
            var userRole = session?.GetString("UserRole");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userRole))
            {
                var user = _db.Users.FirstOrDefault();
                username = username ?? user?.Username ?? user?.Email ?? "";
                var role = user != null ? _db.Roles.FirstOrDefault(r => r.Id == user.RoleId) : null;
                userRole = userRole ?? role?.RoleName ?? "";
            }

            ViewData["UserRole"] = (userRole ?? "").ToLowerInvariant();
            ViewData["Username"] = username ?? "";
            return View("Default");
        }
    }
}
