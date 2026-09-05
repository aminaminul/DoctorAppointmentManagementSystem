using DoctorAppointmentManagementSystem.Models;
using DoctorAppointmentManagementSystem.ViewModels;
using DoctorAppointmentManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoctorAppointmentManagementSystem.Data;

namespace DoctorAppointmentManagementSystem.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public AppointmentController(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }
// Book Action
        public IActionResult Book()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Please log in to book an appointment.";
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Departments = _context.Doctors
                .Where(d => d.ActiveStatus)
                .Select(d => d.Specialization)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            return View();
        }

        public JsonResult GetDoctorsByDepartment(string department)
        {
            var doctors = _context.Doctors
                .Include(d => d.User)
                .Where(d => d.Specialization == department && d.ActiveStatus)
                .Select(d => new
                {
                    id           = d.Id,
                    name         = d.User.Username,
                    specialization = d.Specialization,
                    experience   = d.Experience,
                    fee          = d.ConsultationFee,
                    availableDays = d.AvailableDays ?? "",
                    rating       = _context.Feedbacks.Where(f => f.DoctorId == d.Id && f.Status == "Active").Select(f => (double?)f.Rating).Average() ?? 5.0,
                    reviewsCount = _context.Feedbacks.Count(f => f.DoctorId == d.Id && f.Status == "Active")
                })
                .ToList();

            return Json(doctors);
        }

        public JsonResult GetAvailableSlots(int doctorId, string date)
        {
            if (!DateTime.TryParse(date, out DateTime selectedDate))
                return Json(new List<string>());

            var scheduleSlots = _context.DoctorSchedules
                .Where(ds => ds.DoctorId == doctorId
                          && ds.AvailableDate.Date == selectedDate.Date
                          && ds.SlotStatus == "Available")
                .Select(ds => ds.StartTime)
                .ToList();

            List<string> slots;

            if (scheduleSlots.Any())
            {
                slots = scheduleSlots;
            }
            else
            {
                var doctor = _context.Doctors.FirstOrDefault(d => d.Id == doctorId);
                if (doctor == null || string.IsNullOrWhiteSpace(doctor.AvailableTime))
                    return Json(new List<string>());

                slots = doctor.AvailableTime
                    .Split(',')
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();
            }

            var booked = _context.Appointments
                .Where(a => a.DoctorId == doctorId
                         && a.AppointmentDate.Date == selectedDate.Date
                         && a.AppointmentStatus != "Cancelled")
                .Select(a => a.AppointmentTime)
                .ToList();

            slots = slots.Except(booked).ToList();

            return Json(slots);
        }
// SelectTime Action
        public IActionResult SelectTime(int doctorId, DateTime date)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (date < DateTime.Today)
            {
                TempData["Error"] = "You cannot select a past date!";
                return RedirectToAction("Book");
            }

            var doctor = _context.Doctors
                .Include(d => d.User)
                .FirstOrDefault(d => d.Id == doctorId);

            if (doctor == null)
                return RedirectToAction("Book");

            var scheduleSlots = _context.DoctorSchedules
                .Where(ds => ds.DoctorId == doctorId
                          && ds.AvailableDate.Date == date.Date
                          && ds.SlotStatus == "Available")
                .Select(ds => ds.StartTime)
                .ToList();

            List<string> slots;
            if (scheduleSlots.Any())
            {
                slots = scheduleSlots;
            }
            else
            {
                slots = (doctor.AvailableTime ?? "")
                    .Split(',')
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();
            }

            var booked = _context.Appointments
                .Where(a => a.DoctorId == doctorId
                         && a.AppointmentDate.Date == date.Date
                         && a.AppointmentStatus != "Cancelled")
                .Select(a => a.AppointmentTime)
                .ToList();

            slots = slots.Except(booked).ToList();

            ViewBag.DoctorId     = doctorId;
            ViewBag.Date         = date.ToString("yyyy-MM-dd");
            ViewBag.DateDisplay  = date.ToString("dddd, MMMM dd, yyyy");
            ViewBag.Slots        = slots;
            ViewBag.BookedSlots  = booked;
            ViewBag.DoctorName   = doctor.User.Username;
            ViewBag.Specialization = doctor.Specialization;
            ViewBag.Fee          = doctor.ConsultationFee;
            ViewBag.Experience   = doctor.Experience;

            var avgRating = _context.Feedbacks
                .Where(f => f.DoctorId == doctorId && f.Status == "Active")
                .Select(f => (double?)f.Rating)
                .Average() ?? 5.0;
            var reviewsCount = _context.Feedbacks.Count(f => f.DoctorId == doctorId && f.Status == "Active");
            ViewBag.DoctorAvgRating = avgRating;
            ViewBag.DoctorReviewsCount = reviewsCount;

            return View();
        }

        [HttpPost]
// Confirm Action
        public IActionResult Confirm(int DoctorId, string Date, string TimeSlot,
                                     string ReasonForVisit, bool IsEmergency,
                                     string PaymentMethod, string CardType)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(TimeSlot))
            {
                TempData["Error"] = "Please select a time slot.";
                return Redirect($"/Appointment/SelectTime?doctorId={DoctorId}&date={Date}");
            }

            TempData["DoctorId"]       = DoctorId;
            TempData["Date"]           = Date;
            TempData["TimeSlot"]       = TimeSlot;
            TempData["ReasonForVisit"] = ReasonForVisit;
            TempData["IsEmergency"]    = IsEmergency;
            TempData["PaymentMethod"]  = PaymentMethod;
            TempData["CardType"]       = CardType;

            TempData.Keep();

            if (PaymentMethod == "Bkash")
                return RedirectToAction("BkashPayment");

            if (PaymentMethod == "Nagad")
                return RedirectToAction("NagadPayment");

            if (PaymentMethod == "Card")
                return RedirectToAction("CardPayment");

            return RedirectToAction("FinalConfirm");
        }
// BkashPayment Action
        public IActionResult BkashPayment()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");
            TempData.Keep();
            return View();
        }
// NagadPayment Action
        public IActionResult NagadPayment()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");
            TempData.Keep();
            return View();
        }
// CardPayment Action
        public IActionResult CardPayment()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");
            TempData.Keep();
            return View();
        }

        [HttpPost]
// FinalConfirm Async Action
        public async Task<IActionResult> FinalConfirm(string? PaymentNumber)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var patient = _context.Patients.FirstOrDefault(p => p.UserId == userId);
            if (patient == null)
                return RedirectToAction("Login", "Account");

            int    doctorId      = Convert.ToInt32(TempData["DoctorId"]);
            string dateStr       = TempData["Date"]?.ToString() ?? "";
            string timeSlot      = TempData["TimeSlot"]?.ToString() ?? "";
            string reason        = TempData["ReasonForVisit"]?.ToString() ?? "";
            bool   isEmergency   = Convert.ToBoolean(TempData["IsEmergency"]);
            string paymentMethod = TempData["PaymentMethod"]?.ToString() ?? "Cash";
            string cardType      = TempData["CardType"]?.ToString() ?? "";

            if (!DateTime.TryParse(dateStr, out DateTime bookingDate))
            {
                TempData["Error"] = "Invalid date. Please try again.";
                return RedirectToAction("Book");
            }

            if (bookingDate.Date < DateTime.Today)
            {
                TempData["Error"] = "You cannot book an appointment for a past date!";
                return RedirectToAction("Book");
            }

            bool exists = _context.Appointments.Any(a =>
                a.DoctorId == doctorId &&
                a.AppointmentDate.Date == bookingDate.Date &&
                a.AppointmentTime == timeSlot &&
                a.AppointmentStatus != "Cancelled");

            if (exists)
            {
                TempData["Error"] = "This time slot has just been booked. Please choose another.";
                return RedirectToAction("Book");
            }

            var doctor = _context.Doctors.Include(d => d.User).FirstOrDefault(d => d.Id == doctorId);
            decimal doctorFee = doctor?.ConsultationFee ?? 0;
            decimal ticketFee = 50;
            decimal totalAmount = doctorFee + ticketFee;

            var appointment = new Appointment
            {
                PatientId          = patient.Id,
                DoctorId           = doctorId,
                AppointmentDate    = bookingDate,
                AppointmentTime    = timeSlot,
                ReasonForVisit     = reason,
                IsEmergency        = isEmergency,
                AppointmentStatus  = "Confirmed",
                BookingDateTime    = DateTime.Now
            };

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            string? transactionId = null;
            if (!string.IsNullOrEmpty(PaymentNumber))
            {
                string suffix = DateTime.Now.Ticks.ToString();
                suffix = suffix.Substring(suffix.Length - 6);
                transactionId = (paymentMethod.ToUpper() == "BKASH" ? "BK" : (paymentMethod.ToUpper() == "NAGAD" ? "NG" : "CRD")) + suffix;
            }

            var payment = new Payment
            {
                AppointmentId = appointment.Id,
                PatientId     = patient.Id,
                Amount        = totalAmount,
                PaymentMethod = paymentMethod,
                TransactionId = transactionId,
                PaymentDateTime = DateTime.Now,
                PaymentStatus = "Paid"
            };
            _context.Payments.Add(payment);

            var invoice = new Invoice
            {
                PatientId     = patient.Id,
                AppointmentId = appointment.Id,
                TotalAmount   = totalAmount,
                IssueDate     = DateTime.Now,
                Status        = "Paid",
                Particulars   = $"Doctor Consultation Fee (৳{doctorFee}) + Hospital Ticket / Booking Fee (৳{ticketFee}) for Dr. {doctor?.User?.Username ?? ""} ({doctor?.Specialization ?? ""})"
            };
            _context.Invoices.Add(invoice);

            var scheduleSlot = _context.DoctorSchedules
                .FirstOrDefault(ds => ds.DoctorId == doctorId
                                   && ds.AvailableDate.Date == bookingDate.Date
                                   && ds.StartTime == timeSlot
                                   && ds.SlotStatus == "Available");
            if (scheduleSlot != null)
            {
                scheduleSlot.SlotStatus = "Booked";
            }
            _context.SaveChanges();

            try { await _notificationService.SendPaymentAndBookingNotificationAsync(appointment, payment); }
            catch { }

            TempData["BookingSuccess"]      = "true";
            TempData["BookedDoctorId"]      = doctorId;
            TempData["BookedDate"]          = bookingDate.ToString("dddd, MMMM dd, yyyy");
            TempData["BookedTime"]          = timeSlot;
            TempData["BookedReason"]        = reason;
            TempData["BookedAppointmentId"] = appointment.Id;
            TempData["BookedPaymentMethod"] = paymentMethod;

            return RedirectToAction("Confirmation");
        }
// Confirmation Action
        public IActionResult Confirmation()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (TempData["BookingSuccess"]?.ToString() != "true")
                return RedirectToAction("Book");

            int doctorId = Convert.ToInt32(TempData["BookedDoctorId"]);
            var doctor = _context.Doctors.Include(d => d.User)
                                         .FirstOrDefault(d => d.Id == doctorId);

            decimal docFee = doctor?.ConsultationFee ?? 0;
            decimal ticketFee = 50;

            ViewBag.DoctorName     = doctor?.User?.Username ?? "Doctor";
            ViewBag.Specialization = doctor?.Specialization ?? "";
            ViewBag.Fee            = docFee;
            ViewBag.DoctorFee      = docFee;
            ViewBag.TicketFee      = ticketFee;
            ViewBag.TotalAmount    = docFee + ticketFee;
            ViewBag.PaymentMethod  = TempData["BookedPaymentMethod"]?.ToString() ?? "Online Payment";
            ViewBag.Date           = TempData["BookedDate"];
            ViewBag.TimeSlot       = TempData["BookedTime"];
            ViewBag.Reason         = TempData["BookedReason"];
            ViewBag.AppointmentId   = TempData["BookedAppointmentId"];

            return View();
        }
    }
}

