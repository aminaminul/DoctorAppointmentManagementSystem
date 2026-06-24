using DoctorAppointmentManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointmentManagementSystem.Controllers
{
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard(string section)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            var doctor = _context.Doctors
                .Include(d => d.User)
                .FirstOrDefault(d => d.UserId == userId);

            var appointments = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Where(a => a.DoctorId == doctor.Id)
                .ToList();

            // 🔥 Profile data
            ViewBag.DoctorName = doctor.User.Name;
            ViewBag.Email = doctor.User.Email;
            ViewBag.Specialization = doctor.Specialization;
            ViewBag.Availability = doctor.Availability;

            ViewBag.Section = section;

            return View(appointments);
        }

        // ✅ Approve Appointment
        public IActionResult Approve(int id)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);

            if (appointment != null)
            {
                appointment.Status = "Approved";
                _context.SaveChanges();
            }

            return RedirectToAction("Dashboard");
        }

        // ❌ Reject Appointment
        public IActionResult Reject(int id)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);

            if (appointment != null)
            {
                appointment.Status = "Rejected";
                _context.SaveChanges();
            }

            return RedirectToAction("Dashboard");
        }

        public IActionResult AddPrescription(int appointmentId)
        {
            ViewBag.AppointmentId = appointmentId;
            return View();
        }

        [HttpPost]
        public IActionResult AddPrescription(Prescription model)
        {
            model.CreatedDate = DateTime.Now;

            _context.Prescriptions.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }
    }
}
