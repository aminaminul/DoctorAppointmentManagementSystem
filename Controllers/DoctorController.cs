using DoctorAppointmentManagementSystem.Models;
using DoctorAppointmentManagementSystem.ViewModels;
using DoctorAppointmentManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoctorAppointmentManagementSystem.Data;

namespace DoctorAppointmentManagementSystem.Controllers
{
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public DoctorController(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        private Doctor? GetCurrentDoctor()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return null;
            return _context.Doctors
                           .Include(d => d.User)
                           .FirstOrDefault(d => d.UserId == userId);
        }
// Dashboard Action
        public IActionResult Dashboard(string? section)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            int userId = doctor.UserId;
            ViewBag.UnreadNotificationCount = _context.Notifications
                .Count(n => n.UserId == userId && n.NotificationStatus == "Unread");

            if (section == "notifications")
            {
                ViewBag.Notifications = _context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.SentDateTime)
                    .ToList();
            }

            var appointments = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Where(a => a.DoctorId == doctor.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();

            ViewBag.TotalAppointments  = appointments.Count;
            ViewBag.PendingCount       = appointments.Count(a => a.AppointmentStatus == "Pending");
            ViewBag.ConfirmedCount     = appointments.Count(a => a.AppointmentStatus == "Confirmed"
                                                              || a.AppointmentStatus == "Approved");
            ViewBag.CompletedCount     = appointments.Count(a => a.AppointmentStatus == "Completed");

            var today = DateTime.Today;
            ViewBag.TodaySlots         = _context.DoctorSchedules
                                            .Count(ds => ds.DoctorId == doctor.Id
                                                      && ds.AvailableDate.Date == today
                                                      && !ds.IsVacation);
            ViewBag.UpcomingVacations  = _context.DoctorSchedules
                                            .Count(ds => ds.DoctorId == doctor.Id
                                                      && ds.IsVacation
                                                      && ds.AvailableDate.Date >= today);

            ViewBag.DoctorId       = doctor.Id;
            ViewBag.DoctorName     = doctor.User.Username;
            ViewBag.Email          = doctor.User.Email;
            ViewBag.Specialization = doctor.Specialization;
            ViewBag.Qualification  = doctor.Qualification;
            ViewBag.Experience     = doctor.Experience;
            ViewBag.Fee            = doctor.ConsultationFee;
            ViewBag.AvailableDays  = doctor.AvailableDays;

            ViewBag.Section = section ?? "overview";

            if (ViewBag.Section == "queue")
            {
                QueueManager.EnsureQueueGenerated(_context, doctor.Id, today);
                var queueEntries = _context.QueueEntries
                    .Include(q => q.Appointment)
                    .ThenInclude(a => a.Patient)
                    .ThenInclude(p => p.User)
                    .Where(q => q.Appointment.DoctorId == doctor.Id 
                             && q.Appointment.AppointmentDate.Date == today)
                    .OrderBy(q => q.SequenceNumber)
                    .ToList();

                ViewBag.QueueEntries = queueEntries;
                ViewBag.DoctorStatus = QueueManager.GetDoctorStatus(_context, doctor.Id, today);
            }

            return View(appointments);
        }
// Approve Async Action
        public async Task<IActionResult> Approve(int id)
        {
            var appt = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appt != null)
            {
                appt.AppointmentStatus = "Confirmed";
                _context.SaveChanges();
                try { await _notificationService.SendAppointmentApprovedAsync(appt); } catch { }
            }
            return RedirectToAction("Dashboard", new { section = "appointments" });
        }
// Reject Async Action
        public async Task<IActionResult> Reject(int id)
        {
            var appt = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appt != null)
            {
                appt.AppointmentStatus = "Cancelled";
                _context.SaveChanges();
                try { await _notificationService.SendAppointmentCancelledAsync(appt); } catch { }
            }
            return RedirectToAction("Dashboard", new { section = "appointments" });
        }
// Complete Action
        public IActionResult Complete(int id)
        {
            var appt = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appt != null) { appt.AppointmentStatus = "Completed"; _context.SaveChanges(); }
            return RedirectToAction("Dashboard", new { section = "appointments" });
        }
// Delay Async Action
        public async Task<IActionResult> Delay(int id)
        {
            var appt = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appt != null)
            {
                appt.AppointmentStatus = "Delayed";
                _context.SaveChanges();
                try { await _notificationService.SendAppointmentDelayedAsync(appt); } catch { }
            }
            return RedirectToAction("Dashboard", new { section = "appointments" });
        }
// AddPrescription Action
        public IActionResult AddPrescription(int appointmentId)
        {
            ViewBag.AppointmentId = appointmentId;
            return View();
        }

        [HttpPost]
// AddPrescription Async Action
        public async Task<IActionResult> AddPrescription(Prescription model)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == model.AppointmentId);
            if (appointment != null)
            {
                model.DoctorId = appointment.DoctorId;
                model.PatientId = appointment.PatientId;
            }
            model.Status = "Active";
            model.PrescriptionDateTime = DateTime.Now;
            if (string.IsNullOrEmpty(model.Diagnosis))
            {
                model.Diagnosis = "General Consultation";
            }

            _context.Prescriptions.Add(model);
            _context.SaveChanges();

            try { await _notificationService.SendPrescriptionReadyAsync(model); } catch { }

            return RedirectToAction("Dashboard", new { section = "appointments" });
        }
// Schedule Action
        public IActionResult Schedule(int? month, int? year)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            int m = month ?? DateTime.Today.Month;
            int y = year  ?? DateTime.Today.Year;

            var schedules = _context.DoctorSchedules
                .Where(ds => ds.DoctorId == doctor.Id
                          && ds.AvailableDate.Month == m
                          && ds.AvailableDate.Year  == y)
                .OrderBy(ds => ds.AvailableDate)
                .ThenBy(ds => ds.StartTime)
                .ToList();

            ViewBag.DoctorId   = doctor.Id;
            ViewBag.DoctorName = doctor.User.Username;
            ViewBag.Month      = m;
            ViewBag.Year       = y;
            ViewBag.MonthName  = new DateTime(y, m, 1).ToString("MMMM yyyy");
            ViewBag.DaysInMonth = DateTime.DaysInMonth(y, m);
            ViewBag.FirstDayOfWeek = (int)new DateTime(y, m, 1).DayOfWeek;

            var bookedDates = _context.Appointments
                .Where(a => a.DoctorId == doctor.Id
                         && a.AppointmentDate.Month == m
                         && a.AppointmentDate.Year  == y
                         && a.AppointmentStatus != "Cancelled")
                .GroupBy(a => a.AppointmentDate.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.BookedCounts = bookedDates;

            return View(schedules);
        }
// AddSchedule Action
        public IActionResult AddSchedule()
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");
            return View(new DoctorScheduleViewModel());
        }

        [HttpPost]
// AddSchedule Action
        public IActionResult AddSchedule(DoctorScheduleViewModel vm)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(vm);

            if (!vm.IsVacation)
            {
                bool overlap = _context.DoctorSchedules.Any(ds =>
                    ds.DoctorId == doctor.Id &&
                    ds.AvailableDate.Date == vm.AvailableDate.Date &&
                    !ds.IsVacation);

                if (overlap)
                {
                    ModelState.AddModelError("",
                        "A schedule already exists for this date. Edit the existing entry instead.");
                    return View(vm);
                }
            }

            var schedule = new DoctorSchedule
            {
                DoctorId       = doctor.Id,
                AvailableDate  = vm.AvailableDate,
                StartTime      = vm.IsVacation ? "â€”" : vm.StartTime,
                EndTime        = vm.IsVacation ? "â€”" : vm.EndTime,
                BreakStartTime = vm.BreakStartTime,
                BreakEndTime   = vm.BreakEndTime,
                SlotStatus     = vm.IsVacation ? "Blocked" : "Available",
                IsVacation     = vm.IsVacation,
                Notes          = vm.Notes
            };

            _context.DoctorSchedules.Add(schedule);
            _context.SaveChanges();

            TempData["Success"] = vm.IsVacation
                ? $"Vacation day marked for {vm.AvailableDate:MMMM dd, yyyy}."
                : $"Schedule added for {vm.AvailableDate:MMMM dd, yyyy} ({vm.StartTime} â€“ {vm.EndTime}).";

            return RedirectToAction("Schedule");
        }
// EditSchedule Action
        public IActionResult EditSchedule(int id)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            var entry = _context.DoctorSchedules
                .FirstOrDefault(ds => ds.Id == id && ds.DoctorId == doctor.Id);
            if (entry == null) return NotFound();

            var vm = new DoctorScheduleViewModel
            {
                Id             = entry.Id,
                AvailableDate  = entry.AvailableDate,
                StartTime      = entry.StartTime,
                EndTime        = entry.EndTime,
                BreakStartTime = entry.BreakStartTime,
                BreakEndTime   = entry.BreakEndTime,
                IsVacation     = entry.IsVacation,
                Notes          = entry.Notes
            };
            return View(vm);
        }

        [HttpPost]
// EditSchedule Action
        public IActionResult EditSchedule(DoctorScheduleViewModel vm)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(vm);

            var entry = _context.DoctorSchedules
                .FirstOrDefault(ds => ds.Id == vm.Id && ds.DoctorId == doctor.Id);
            if (entry == null) return NotFound();

            entry.AvailableDate  = vm.AvailableDate;
            entry.StartTime      = vm.IsVacation ? "â€”" : vm.StartTime;
            entry.EndTime        = vm.IsVacation ? "â€”" : vm.EndTime;
            entry.BreakStartTime = vm.BreakStartTime;
            entry.BreakEndTime   = vm.BreakEndTime;
            entry.SlotStatus     = vm.IsVacation ? "Blocked" : "Available";
            entry.IsVacation     = vm.IsVacation;
            entry.Notes          = vm.Notes;

            _context.SaveChanges();

            TempData["Success"] = "Schedule updated successfully.";
            return RedirectToAction("Schedule");
        }

        [HttpPost]
// DeleteSchedule Action
        public IActionResult DeleteSchedule(int id)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            var entry = _context.DoctorSchedules
                .FirstOrDefault(ds => ds.Id == id && ds.DoctorId == doctor.Id);
            if (entry != null)
            {
                _context.DoctorSchedules.Remove(entry);
                _context.SaveChanges();
                TempData["Success"] = "Schedule entry removed.";
            }
            return RedirectToAction("Schedule");
        }

        [HttpPost]
// SetVacationRange Action
        public IActionResult SetVacationRange(DateTime from, DateTime to, string? notes)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            if (from > to)
            {
                TempData["Error"] = "Start date must be before end date.";
                return RedirectToAction("Schedule");
            }

            int added = 0;
            for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
            {
                bool exists = _context.DoctorSchedules.Any(ds =>
                    ds.DoctorId == doctor.Id && ds.AvailableDate.Date == d);
                if (!exists)
                {
                    _context.DoctorSchedules.Add(new DoctorSchedule
                    {
                        DoctorId      = doctor.Id,
                        AvailableDate = d,
                        StartTime     = "â€”",
                        EndTime       = "â€”",
                        SlotStatus    = "Blocked",
                        IsVacation    = true,
                        Notes         = notes ?? "Vacation"
                    });
                    added++;
                }
            }
            _context.SaveChanges();

            TempData["Success"] = $"Vacation set for {added} day(s) from {from:MMM dd} to {to:MMM dd, yyyy}.";
            return RedirectToAction("Schedule");
        }

        public JsonResult GetScheduleJson(int month, int year)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return Json(new List<object>());

            var schedules = _context.DoctorSchedules
                .Where(ds => ds.DoctorId == doctor.Id
                          && ds.AvailableDate.Month == month
                          && ds.AvailableDate.Year  == year)
                .Select(ds => new
                {
                    id            = ds.Id,
                    date          = ds.AvailableDate.ToString("yyyy-MM-dd"),
                    startTime     = ds.StartTime,
                    endTime       = ds.EndTime,
                    breakStart    = ds.BreakStartTime,
                    breakEnd      = ds.BreakEndTime,
                    slotStatus    = ds.SlotStatus,
                    isVacation    = ds.IsVacation,
                    notes         = ds.Notes
                })
                .ToList();

            return Json(schedules);
        }

        [HttpPost]
// MarkNotificationRead Action
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
// MarkAllNotificationsRead Action
        public IActionResult MarkAllNotificationsRead()
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            int userId = doctor.UserId;
            var unread = _context.Notifications
                .Where(n => n.UserId == userId && n.NotificationStatus == "Unread")
                .ToList();
            foreach (var n in unread) n.NotificationStatus = "Read";
            _context.SaveChanges();
            return RedirectToAction("Dashboard", new { section = "notifications" });
        }
    }
}

