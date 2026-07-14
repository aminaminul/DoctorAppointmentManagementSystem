using System.Diagnostics;
using DoctorAppointmentManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DoctorAppointmentManagementSystem.Data;

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
            return View();
        }

        public IActionResult Privacy()
        {
            var policy = _db.PrivacyPolicies.OrderByDescending(p => p.UpdatedAt).FirstOrDefault();
            ViewBag.PrivacyContent = policy?.Content ?? string.Empty;
            return View();
        }

        // Print-friendly view: opens in new tab and triggers browser print
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
