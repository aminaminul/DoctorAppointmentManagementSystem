using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoctorAppointmentManagementSystem.Data;
using DoctorAppointmentManagementSystem.Models;

namespace DoctorAppointmentManagementSystem.Controllers
{
    public class EMRController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EMRController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= PATIENT SEARCH / LIST =================
        public IActionResult Index(string? searchQuery)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId == null || (roleId != 1 && roleId != 2))
            {
                TempData["Error"] = "Unauthorized access.";
                return RedirectToAction("Login", "Account");
            }

            var query = _context.Patients
                .Include(p => p.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(p => p.User.FullName.Contains(searchQuery) 
                                      || p.User.Email.Contains(searchQuery)
                                      || p.User.PhoneNumber.Contains(searchQuery));
            }

            var patients = query.ToList();
            ViewBag.SearchQuery = searchQuery;
            ViewBag.RoleId = roleId;

            return View(patients);
        }

        // ================= PATIENT EMR TIMELINE =================
        public IActionResult PatientTimeline(int patientId)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Authorization check
            var patient = _context.Patients
                .Include(p => p.User)
                .FirstOrDefault(p => p.Id == patientId);

            if (patient == null)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction("Index");
            }

            // Patients can only see their own EMR
            if (roleId == 3)
            {
                var currentPatient = _context.Patients.FirstOrDefault(p => p.UserId == userId);
                if (currentPatient == null || currentPatient.Id != patientId)
                {
                    TempData["Error"] = "Access denied.";
                    return RedirectToAction("Dashboard", "Patient");
                }
            }

            // Fetch Medical Records
            var medicalRecords = _context.MedicalRecords
                .Include(mr => mr.Doctor).ThenInclude(d => d.User)
                .Where(mr => mr.PatientId == patientId)
                .OrderByDescending(mr => mr.RecordDate)
                .ToList();

            // Fetch Prescriptions
            var prescriptions = _context.Prescriptions
                .Include(p => p.Doctor).ThenInclude(d => d.User)
                .Where(p => p.PatientId == patientId)
                .OrderByDescending(p => p.PrescriptionDateTime)
                .ToList();

            ViewBag.Patient = patient;
            ViewBag.MedicalRecords = medicalRecords;
            ViewBag.Prescriptions = prescriptions;
            ViewBag.RoleId = roleId;

            // Resolve logged-in doctor if role is Doctor
            if (roleId == 2)
            {
                var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId);
                ViewBag.DoctorId = doctor?.Id;
            }

            return View();
        }

        // ================= CREATE MEDICAL RECORD =================
        public IActionResult Create(int patientId, int? appointmentId)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId == null || roleId != 2)
            {
                TempData["Error"] = "Only doctors can add medical records.";
                return RedirectToAction("Login", "Account");
            }

            var patient = _context.Patients.Include(p => p.User).FirstOrDefault(p => p.Id == patientId);
            if (patient == null)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction("Index");
            }

            ViewBag.Patient = patient;
            ViewBag.AppointmentId = appointmentId;

            return View();
        }

        [HttpPost]
        public IActionResult Create(MedicalRecord model)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId);
            if (doctor == null)
            {
                TempData["Error"] = "Doctor profile not found.";
                return RedirectToAction("Login", "Account");
            }

            model.DoctorId = doctor.Id;
            model.RecordDate = DateTime.Now;

            // If linked to an appointment, auto-complete it
            if (model.AppointmentId.HasValue)
            {
                var appt = _context.Appointments.FirstOrDefault(a => a.Id == model.AppointmentId.Value);
                if (appt != null && appt.AppointmentStatus != "Completed")
                {
                    appt.AppointmentStatus = "Completed";
                }
            }

            _context.MedicalRecords.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "Medical record added successfully!";
            return RedirectToAction("PatientTimeline", new { patientId = model.PatientId });
        }

        // ================= EDIT MEDICAL RECORD =================
        public IActionResult Edit(int id)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (roleId == null || roleId != 2)
            {
                TempData["Error"] = "Only doctors can edit medical records.";
                return RedirectToAction("Login", "Account");
            }

            var record = _context.MedicalRecords
                .Include(mr => mr.Patient).ThenInclude(p => p.User)
                .FirstOrDefault(mr => mr.Id == id);

            if (record == null)
            {
                TempData["Error"] = "Medical record not found.";
                return RedirectToAction("Index");
            }

            var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId);
            if (doctor == null || record.DoctorId != doctor.Id)
            {
                TempData["Error"] = "You can only edit medical records you created.";
                return RedirectToAction("PatientTimeline", new { patientId = record.PatientId });
            }

            ViewBag.Patient = record.Patient;
            return View(record);
        }

        [HttpPost]
        public IActionResult Edit(MedicalRecord model)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            var record = _context.MedicalRecords.FirstOrDefault(mr => mr.Id == model.Id);
            if (record == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("Index");
            }

            var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId);
            if (doctor == null || record.DoctorId != doctor.Id)
            {
                TempData["Error"] = "Unauthorized edit.";
                return RedirectToAction("PatientTimeline", new { patientId = record.PatientId });
            }

            record.Diagnosis = model.Diagnosis;
            record.TreatmentDetails = model.TreatmentDetails;
            record.TestReports = model.TestReports;
            record.Notes = model.Notes;

            _context.SaveChanges();

            TempData["Success"] = "Medical record updated successfully!";
            return RedirectToAction("PatientTimeline", new { patientId = record.PatientId });
        }

        // ================= DELETE MEDICAL RECORD =================
        [HttpPost]
        public IActionResult Delete(int id)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (roleId == null || (roleId != 1 && roleId != 2))
            {
                TempData["Error"] = "Unauthorized access.";
                return RedirectToAction("Login", "Account");
            }

            var record = _context.MedicalRecords.FirstOrDefault(mr => mr.Id == id);
            if (record == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("Index");
            }

            if (roleId == 2) // Doctor check
            {
                var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId);
                if (doctor == null || record.DoctorId != doctor.Id)
                {
                    TempData["Error"] = "You can only delete records you created.";
                    return RedirectToAction("PatientTimeline", new { patientId = record.PatientId });
                }
            }

            int patientId = record.PatientId;
            _context.MedicalRecords.Remove(record);
            _context.SaveChanges();

            TempData["Success"] = "Medical record deleted successfully!";
            return RedirectToAction("PatientTimeline", new { patientId = patientId });
        }
    }
}
