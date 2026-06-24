using DoctorAppointmentManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointmentManagementSystem.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= STEP 1: BOOK PAGE =================

        public IActionResult Book()
        {
            ViewBag.Specializations = _context.Doctors
                .Select(d => d.Specialization)
                .Distinct()
                .ToList();

            return View();
        }

        // ================= AJAX: LOAD DOCTOR BY SPECIALIZATION =================

        public JsonResult GetDoctorsBySpecialization(string specialization)
        {
            var doctors = _context.Doctors
                .Include(d => d.User)
                .Where(d => d.Specialization == specialization)
                .Select(d => new
                {
                    id = d.Id,
                    name = d.User.Name
                })
                .ToList();

            return Json(doctors);
        }

        // ================= STEP 2: SELECT TIME =================

        public IActionResult SelectTime(int doctorId, DateTime date)
        {
            // 🔥 Past date validation
            if (date < DateTime.Today)
            {
                TempData["Error"] = "You cannot select past date!";
                return RedirectToAction("Book");
            }

            var doctor = _context.Doctors
                .Include(d => d.User)
                .FirstOrDefault(d => d.Id == doctorId);

            if (doctor == null)
                return RedirectToAction("Book");

            // 🔥 Availability logic
            var slots = doctor.Availability
                .Split(',')
                .Select(t => t.Trim())
                .ToList();

            // 🔥 Remove booked slots
            var booked = _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.Date == date)
                .Select(a => a.TimeSlot)
                .ToList();

            slots = slots.Except(booked).ToList();

            ViewBag.DoctorId = doctorId;
            ViewBag.Date = date;
            ViewBag.Slots = slots;

            return View();
        }

        // ================= PAYMENT STEP =================

        [HttpPost]
        public IActionResult Confirm(Appointment model, string PaymentMethod, string CardType)
        {
            // 🔥 Save temporary data
            TempData["DoctorId"] = model.DoctorId;
            TempData["Date"] = model.Date.ToString();
            TempData["TimeSlot"] = model.TimeSlot;

            TempData["PaymentMethod"] = PaymentMethod;
            TempData["CardType"] = CardType;

            // 🔥 bKash
            if (PaymentMethod == "Bkash")
            {
                return RedirectToAction("BkashPayment");
            }

            // 🔥 Card
            if (PaymentMethod == "Card")
            {
                return RedirectToAction("CardPayment");
            }

            return RedirectToAction("Book");
        }

        // ================= BKASH PAYMENT PAGE =================

        public IActionResult BkashPayment()
        {
            return View();
        }

        // ================= CARD PAYMENT PAGE =================

        public IActionResult CardPayment()
        {
            return View();
        }

        // ================= FINAL CONFIRM =================

        [HttpPost]
        public IActionResult FinalConfirm(string PaymentNumber)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var patient = _context.Patients
                .FirstOrDefault(p => p.UserId == userId);

            if (patient == null)
                return RedirectToAction("Login", "Account");

            Appointment model = new Appointment();

            model.PatientId = patient.Id;

            model.DoctorId = Convert.ToInt32(TempData["DoctorId"]);

            model.Date = Convert.ToDateTime(TempData["Date"]);

            model.TimeSlot = TempData["TimeSlot"].ToString();

            // 🔥 Past date validation
            if (model.Date < DateTime.Today)
            {
                TempData["Error"] = "You cannot book appointment in past dates!";
                return RedirectToAction("Book");
            }

            // 🔥 Double booking check
            var exists = _context.Appointments.Any(a =>
                a.DoctorId == model.DoctorId &&
                a.Date == model.Date &&
                a.TimeSlot == model.TimeSlot);

            if (exists)
            {
                TempData["Error"] = "This slot already booked!";
                return RedirectToAction("Book");
            }

            model.Status = "Pending";

            _context.Appointments.Add(model);

            _context.SaveChanges();

            return RedirectToAction("Dashboard", "Patient");
        }
    }
}