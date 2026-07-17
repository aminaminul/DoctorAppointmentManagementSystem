using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoctorAppointmentManagementSystem.Models;
using DoctorAppointmentManagementSystem.Data;

namespace DoctorAppointmentManagementSystem.Controllers
{
    public class QueueController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QueueController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult CallNext(int doctorId)
        {
            var today = DateTime.Today;

            QueueManager.EnsureQueueGenerated(_context, doctorId, today);

            var active = _context.QueueEntries
                .Include(q => q.Appointment)
                .FirstOrDefault(q => q.Appointment.DoctorId == doctorId 
                                  && q.Appointment.AppointmentDate.Date == today 
                                  && (q.Status == "Calling" || q.Status == "InConsultation"));

            if (active != null)
            {
                TempData["Error"] = "Finish the current patient's consultation before calling the next one!";
                return RedirectToAction("Dashboard", "Doctor", new { section = "queue" });
            }

            var nextEntry = _context.QueueEntries
                .Include(q => q.Appointment)
                .Where(q => q.Appointment.DoctorId == doctorId 
                         && q.Appointment.AppointmentDate.Date == today 
                         && q.Status == "Waiting")
                .OrderBy(q => q.SequenceNumber)
                .FirstOrDefault();

            if (nextEntry != null)
            {
                nextEntry.Status = "Calling";
                nextEntry.CallTime = DateTime.Now;
                _context.SaveChanges();
                TempData["Success"] = $"Calling Token #{nextEntry.TokenNumber} next!";
            }
            else
            {
                TempData["Error"] = "No more waiting patients in the queue.";
            }

            return RedirectToAction("Dashboard", "Doctor", new { section = "queue" });
        }

        [HttpPost]
        public IActionResult StartConsultation(int id)
        {
            var entry = _context.QueueEntries
                .Include(q => q.Appointment)
                .FirstOrDefault(q => q.Id == id);

            if (entry != null && entry.Status == "Calling")
            {
                entry.Status = "InConsultation";
                _context.SaveChanges();
                TempData["Success"] = $"Consultation started for Token #{entry.TokenNumber}!";
            }

            return RedirectToAction("Dashboard", "Doctor", new { section = "queue" });
        }

        [HttpPost]
        public IActionResult CompleteConsultation(int id)
        {
            var entry = _context.QueueEntries
                .Include(q => q.Appointment)
                .FirstOrDefault(q => q.Id == id);

            if (entry != null && entry.Status == "InConsultation")
            {
                entry.Status = "Completed";
                entry.CompletionTime = DateTime.Now;

                entry.Appointment.AppointmentStatus = "Completed";
                
                _context.SaveChanges();
                TempData["Success"] = $"Consultation completed for Token #{entry.TokenNumber}!";

                return RedirectToAction("AddPrescription", "Doctor", new { appointmentId = entry.AppointmentId });
            }

            return RedirectToAction("Dashboard", "Doctor", new { section = "queue" });
        }

        [HttpPost]
        public IActionResult SkipPatient(int id)
        {
            var entry = _context.QueueEntries
                .Include(q => q.Appointment)
                .FirstOrDefault(q => q.Id == id);

            if (entry != null && (entry.Status == "Calling" || entry.Status == "InConsultation" || entry.Status == "Waiting"))
            {
                entry.Status = "Skipped";
                _context.SaveChanges();
                TempData["Success"] = $"Token #{entry.TokenNumber} marked as Skipped!";
            }

            return RedirectToAction("Dashboard", "Doctor", new { section = "queue" });
        }

        [HttpPost]
        public IActionResult BumpEmergency(int id, string redirectSection = "queue")
        {
            var entry = _context.QueueEntries
                .Include(q => q.Appointment)
                .FirstOrDefault(q => q.Id == id);

            if (entry != null)
            {
                entry.Appointment.IsEmergency = true;
                _context.SaveChanges();

                QueueManager.ReSequenceQueue(_context, entry.Appointment.DoctorId, entry.Appointment.AppointmentDate);
                TempData["Success"] = $"Token #{entry.TokenNumber} bumped to Emergency Priority!";
            }

            if (redirectSection == "admin")
            {
                return RedirectToAction("Dashboard", "Admin", new { section = "queue" });
            }
            return RedirectToAction("Dashboard", "Doctor", new { section = "queue" });
        }

        [HttpGet]
        public JsonResult GetLiveQueueJson(int doctorId)
        {
            var today = DateTime.Today;

            var queue = _context.QueueEntries
                .Include(q => q.Appointment)
                .ThenInclude(a => a.Patient)
                .ThenInclude(p => p.User)
                .Where(q => q.Appointment.DoctorId == doctorId 
                         && q.Appointment.AppointmentDate.Date == today)
                .OrderBy(q => q.SequenceNumber)
                .Select(q => new
                {
                    id = q.Id,
                    tokenNumber = q.TokenNumber,
                    sequenceNumber = q.SequenceNumber,
                    patientName = q.Appointment.Patient.User.Username,
                    timeSlot = q.Appointment.AppointmentTime,
                    status = q.Status,
                    isEmergency = q.Appointment.IsEmergency
                })
                .ToList();

            var currentServing = queue.FirstOrDefault(q => q.status == "InConsultation" || q.status == "Calling");
            var doctorStatus = QueueManager.GetDoctorStatus(_context, doctorId, today);

            return Json(new
            {
                doctorStatus = doctorStatus,
                currentServingToken = currentServing?.tokenNumber ?? 0,
                currentServingName = currentServing?.patientName ?? "None",
                queueList = queue
            });
        }
    }
}
