using DoctorAppointmentManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointmentManagementSystem.Controllers
{
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= DASHBOARD =================

        public IActionResult Dashboard(string section)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            var patient = _context.Patients
                .Include(p => p.User)
                .FirstOrDefault(p => p.UserId == userId);

            var appointments = _context.Appointments
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .Where(a => a.PatientId == patient.Id)
                .ToList();

            // 🔷 Profile Data
            ViewBag.PatientName = patient.User.Name;
            ViewBag.Email = patient.User.Email;
            ViewBag.Age = patient.Age;
            ViewBag.Gender = patient.Gender;

            ViewBag.Section = section;

            return View(appointments);
        }

        // ================= EDIT PROFILE PAGE =================

        public IActionResult EditProfile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            var patient = _context.Patients
                .Include(p => p.User)
                .FirstOrDefault(p => p.UserId == userId);

            return View(patient);
        }

        // ================= UPDATE PROFILE =================

        [HttpPost]
        public IActionResult EditProfile(Patient model)
        {
            var patient = _context.Patients
                .Include(p => p.User)
                .FirstOrDefault(p => p.Id == model.Id);

            if (patient != null)
            {
                // 🔷 Update Patient Info
                patient.Age = model.Age;
                patient.Gender = model.Gender;

                // 🔷 Update User Info
                patient.User.Name = model.User.Name;
                patient.User.Email = model.User.Email;

                _context.SaveChanges();
            }

            return RedirectToAction("Dashboard", new { section = "profile" });
        }

        // ================= PRESCRIPTIONS =================

        public IActionResult Prescriptions()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            var patient = _context.Patients
                .FirstOrDefault(p => p.UserId == userId);

            var prescriptions = _context.Prescriptions
                .Include(p => p.Appointment)
                .Where(p => p.Appointment.PatientId == patient.Id)
                .ToList();

            return View(prescriptions);
        }
    }
}