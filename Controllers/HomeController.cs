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
            var doctors = _db.Doctors
                .Include(d => d.User)
                .Where(d => d.ActiveStatus)
                .Take(4)
                .ToList();

            ViewBag.Doctors = doctors;

            return View();
        }
        // Department Details Action
        public IActionResult Department(string? name)
        {
            string deptName = string.IsNullOrWhiteSpace(name) ? "Cardiology" : name.Trim();

            // Mapping department names to keywords that match doctor Specialization in database
            string searchKey = deptName.ToLower() switch
            {
                "cardiology" => "cardio",
                "neurology" => "neuro",
                "pediatrics" => "pediatric",
                "orthopedics" => "ortho",
                "dermatology" => "derm",
                "ophthalmology" => "ophthal",
                "gynaecologist" or "gynecology" => "gynaec",
                "medicine specialist" or "medicine" => "medicine",
                _ => deptName.ToLower()
            };

            // Fetch doctors whose specialization strictly belongs to this department
            var allActiveDoctors = _db.Doctors
                .Include(d => d.User)
                .Where(d => d.ActiveStatus)
                .ToList();

            var doctors = allActiveDoctors
                .Where(d => !string.IsNullOrEmpty(d.Specialization) && 
                            d.Specialization.ToLower().Contains(searchKey))
                .ToList();

            ViewBag.DepartmentName = deptName;
            ViewBag.Doctors = doctors;
            ViewBag.AllDepartments = new List<string> { 
                "Cardiology", "Neurology", "Pediatrics", 
                "Orthopedics", "Dermatology", "Ophthalmology",
                "Gynaecologist", "Medicine Specialist"
            };

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

