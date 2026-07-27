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
// Index Action
        public IActionResult Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var doctors = _db.Doctors
                .Include(d => d.User)
                .Where(d => d.ActiveStatus)
                .Take(4)
                .ToList();

            ViewBag.Doctors = doctors;

            return View();
        }
// Privacy Action
        public IActionResult Privacy()
        {
            var policy = _db.PrivacyPolicies.OrderByDescending(p => p.UpdatedAt).FirstOrDefault();
            ViewBag.PrivacyContent = policy?.Content ?? string.Empty;
            return View();
        }

        [HttpGet]
// PrintPrivacy Action
        public IActionResult PrintPrivacy()
        {
            var policy = _db.PrivacyPolicies.OrderByDescending(p => p.UpdatedAt).FirstOrDefault();
            ViewBag.PrivacyContent = policy?.Content ?? string.Empty;
            return View("PrivacyPrint");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
// Error Action
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

