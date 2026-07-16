using System.Diagnostics;
using DoctorAppointmentManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DoctorAppointmentManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointmentManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (!string.IsNullOrEmpty(userRole))
            {
                if (userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                    return RedirectToAction("Dashboard", "Admin");
                if (userRole.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
                    return RedirectToAction("Dashboard", "Doctor");
                if (userRole.Equals("Patient", StringComparison.OrdinalIgnoreCase))
                    return RedirectToAction("Dashboard", "Patient");
            }

            var doctors = _db.Doctors
                .Include(d => d.User)
                .Where(d => d.ActiveStatus)
                .Take(4)
                .ToList();

            ViewBag.Doctors = doctors;

            return View();
        }

        public IActionResult Privacy()
        {
            var policy = _db.PrivacyPolicies.OrderByDescending(p => p.UpdatedAt).FirstOrDefault();
            ViewBag.PrivacyContent = policy?.Content ?? string.Empty;
            return View();
        }

        [HttpGet]
        public IActionResult PrintPrivacy()
        {
            var policy = _db.PrivacyPolicies.OrderByDescending(p => p.UpdatedAt).FirstOrDefault();
            ViewBag.PrivacyContent = policy?.Content ?? string.Empty;
            return View("PrivacyPrint");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
