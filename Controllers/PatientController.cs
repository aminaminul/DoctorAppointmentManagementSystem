using DoctorAppointmentManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoctorAppointmentManagementSystem.Data;

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
            ViewBag.Patient = patient;
            ViewBag.PatientName = patient.User.Name;
            ViewBag.Email = patient.User.Email;
            ViewBag.Age = patient.Age;
            ViewBag.Gender = patient.Gender;

            ViewBag.Section = section;

            // 🔔 Notification count for sidebar bell badge
            ViewBag.UnreadNotificationCount = _context.Notifications
                .Count(n => n.UserId == userId && n.NotificationStatus == "Unread");

            if (section == "notifications")
            {
                ViewBag.Notifications = _context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.SentDateTime)
                    .ToList();
            }

            if (section == "queue")
            {
                var today = DateTime.Today;
                var activeQueueEntry = _context.QueueEntries
                    .Include(q => q.Appointment)
                    .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
                    .FirstOrDefault(q => q.Appointment.PatientId == patient.Id 
                                      && q.Appointment.AppointmentDate.Date == today
                                      && q.Appointment.AppointmentStatus == "Confirmed"
                                      && (q.Status == "Waiting" || q.Status == "Calling" || q.Status == "InConsultation"));

                if (activeQueueEntry != null)
                {
                    ViewBag.ActiveQueueEntry = activeQueueEntry;
                    
                    // Make sure queue for this doctor is updated/sequenced
                    QueueManager.EnsureQueueGenerated(_context, activeQueueEntry.Appointment.DoctorId, today);
                    
                    // Fetch queue stats
                    var doctorQueue = _context.QueueEntries
                        .Where(q => q.Appointment.DoctorId == activeQueueEntry.Appointment.DoctorId 
                                 && q.Appointment.AppointmentDate.Date == today)
                        .OrderBy(q => q.SequenceNumber)
                        .ToList();

                    // Currently serving
                    var servingEntry = doctorQueue.FirstOrDefault(q => q.Status == "InConsultation" || q.Status == "Calling");
                    ViewBag.ServingToken = servingEntry?.TokenNumber ?? 0;

                    // Position ahead of current patient
                    int patientsAhead = doctorQueue
                        .Count(q => q.SequenceNumber < activeQueueEntry.SequenceNumber 
                                 && q.Status == "Waiting");
                    ViewBag.PatientsAhead = patientsAhead;

                    // Doctor schedule & status
                    var schedule = _context.DoctorSchedules
                        .FirstOrDefault(ds => ds.DoctorId == activeQueueEntry.Appointment.DoctorId 
                                           && ds.AvailableDate.Date == today);
                    ViewBag.DoctorSchedule = schedule;
                    ViewBag.DoctorStatus = QueueManager.GetDoctorStatus(_context, activeQueueEntry.Appointment.DoctorId, today);
                }
            }

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
                patient.DateOfBirth = model.DateOfBirth;
                patient.Gender = model.Gender;
                patient.BloodGroup = model.BloodGroup;
                patient.Address = model.Address;
                patient.EmergencyContact = model.EmergencyContact;
                patient.MedicalHistory = model.MedicalHistory;
                patient.Allergies = model.Allergies;

                // 🔷 Update User Info
                patient.User.Name = model.User.Name;
                patient.User.Email = model.User.Email;
                patient.User.PhoneNumber = model.User.PhoneNumber;

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

        // ================= MARK NOTIFICATION READ =================

        [HttpPost]
        public IActionResult MarkNotificationRead(int id)
        {
            var notification = _context.Notifications.FirstOrDefault(n => n.Id == id);
            if (notification != null)
            {
                notification.NotificationStatus = "Read";
                _context.SaveChanges();
            }
            return RedirectToAction("Dashboard", new { section = "notifications" });
        }

        [HttpPost]
        public IActionResult MarkAllNotificationsRead()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            var unread = _context.Notifications
                .Where(n => n.UserId == userId && n.NotificationStatus == "Unread")
                .ToList();
            foreach (var n in unread) n.NotificationStatus = "Read";
            _context.SaveChanges();
            return RedirectToAction("Dashboard", new { section = "notifications" });
        }
    }
}