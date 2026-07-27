using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DoctorAppointmentManagementSystem.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            string? userRole = HttpContext.Session.GetString("UserRole");

            if (roleId == 1 || userRole == "Admin")
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else if (roleId == 2 || userRole == "Doctor")
            {
                return RedirectToAction("Dashboard", "Doctor");
            }
            else if (roleId == 3 || userRole == "Patient" || userRole == "Student")
            {
                return RedirectToAction("Dashboard", "Patient");
            }

            return RedirectToAction("Login", "Account");
        }
    }
}
