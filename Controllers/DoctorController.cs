using DoctorAppointmentManagementSystem.Models;
using DoctorAppointmentManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoctorAppointmentManagementSystem.Data;

namespace DoctorAppointmentManagementSystem.Controllers
{
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Helper — resolve logged-in doctor (returns null if not authenticated)
        // ─────────────────────────────────────────────────────────────────────────
        private Doctor? GetCurrentDoctor()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return null;
            return _context.Doctors
                           .Include(d => d.User)
                           .FirstOrDefault(d => d.UserId == userId);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  DASHBOARD
        // ─────────────────────────────────────────────────────────────────────────
        public IActionResult Dashboard(string? section)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            var appointments = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Where(a => a.DoctorId == doctor.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();

            // Stats for dashboard cards
            ViewBag.TotalAppointments  = appointments.Count;
            ViewBag.PendingCount       = appointments.Count(a => a.AppointmentStatus == "Pending");
            ViewBag.ConfirmedCount     = appointments.Count(a => a.AppointmentStatus == "Confirmed"
                                                              || a.AppointmentStatus == "Approved");
            ViewBag.CompletedCount     = appointments.Count(a => a.AppointmentStatus == "Completed");

            // Schedule summary
            var today = DateTime.Today;
            ViewBag.TodaySlots         = _context.DoctorSchedules
                                            .Count(ds => ds.DoctorId == doctor.Id
                                                      && ds.AvailableDate.Date == today
                                                      && !ds.IsVacation);
            ViewBag.UpcomingVacations  = _context.DoctorSchedules
                                            .Count(ds => ds.DoctorId == doctor.Id
                                                      && ds.IsVacation
                                                      && ds.AvailableDate.Date >= today);

            // Doctor profile
            ViewBag.DoctorId       = doctor.Id;
            ViewBag.DoctorName     = doctor.User.FullName;
            ViewBag.Email          = doctor.User.Email;
            ViewBag.Specialization = doctor.Specialization;
            ViewBag.Qualification  = doctor.Qualification;
            ViewBag.Experience     = doctor.Experience;
            ViewBag.Fee            = doctor.ConsultationFee;
            ViewBag.AvailableDays  = doctor.AvailableDays;
            ViewBag.ProfileImage   = doctor.ProfileImage ?? "doctor_default.png";

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

        // ─────────────────────────────────────────────────────────────────────────
        //  APPROVE / REJECT APPOINTMENT
        // ─────────────────────────────────────────────────────────────────────────
        public IActionResult Approve(int id)
        {
            var appt = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appt != null) { appt.AppointmentStatus = "Confirmed"; _context.SaveChanges(); }
            return RedirectToAction("Dashboard", new { section = "appointments" });
        }

        public IActionResult Reject(int id)
        {
            var appt = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appt != null) { appt.AppointmentStatus = "Cancelled"; _context.SaveChanges(); }
            return RedirectToAction("Dashboard", new { section = "appointments" });
        }

        public IActionResult Complete(int id)
        {
            var appt = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appt != null) { appt.AppointmentStatus = "Completed"; _context.SaveChanges(); }
            return RedirectToAction("Dashboard", new { section = "appointments" });
        }

        public IActionResult Delay(int id)
        {
            var appt = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (appt != null) { appt.AppointmentStatus = "Delayed"; _context.SaveChanges(); }
            return RedirectToAction("Dashboard", new { section = "appointments" });
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  PRESCRIPTIONS
        // ─────────────────────────────────────────────────────────────────────────
        public IActionResult AddPrescription(int appointmentId)
        {
            ViewBag.AppointmentId = appointmentId;
            return View();
        }

        [HttpPost]
        public IActionResult AddPrescription(Prescription model)
        {
            model.PrescriptionDateTime = DateTime.Now;
            _context.Prescriptions.Add(model);
            _context.SaveChanges();
            return RedirectToAction("Dashboard", new { section = "appointments" });
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  SCHEDULE — LIST
        // ─────────────────────────────────────────────────────────────────────────
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
            ViewBag.DoctorName = doctor.User.FullName;
            ViewBag.Month      = m;
            ViewBag.Year       = y;
            ViewBag.MonthName  = new DateTime(y, m, 1).ToString("MMMM yyyy");
            ViewBag.DaysInMonth = DateTime.DaysInMonth(y, m);
            ViewBag.FirstDayOfWeek = (int)new DateTime(y, m, 1).DayOfWeek;

            // Booked appointment counts per date for conflict indicator
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

        // ─────────────────────────────────────────────────────────────────────────
        //  SCHEDULE — ADD
        // ─────────────────────────────────────────────────────────────────────────
        public IActionResult AddSchedule()
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");
            return View(new DoctorScheduleViewModel());
        }

        [HttpPost]
        public IActionResult AddSchedule(DoctorScheduleViewModel vm)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(vm);

            // Overlap check — skip for vacation days
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
                StartTime      = vm.IsVacation ? "—" : vm.StartTime,
                EndTime        = vm.IsVacation ? "—" : vm.EndTime,
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
                : $"Schedule added for {vm.AvailableDate:MMMM dd, yyyy} ({vm.StartTime} – {vm.EndTime}).";

            return RedirectToAction("Schedule");
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  SCHEDULE — EDIT
        // ─────────────────────────────────────────────────────────────────────────
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
        public IActionResult EditSchedule(DoctorScheduleViewModel vm)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(vm);

            var entry = _context.DoctorSchedules
                .FirstOrDefault(ds => ds.Id == vm.Id && ds.DoctorId == doctor.Id);
            if (entry == null) return NotFound();

            entry.AvailableDate  = vm.AvailableDate;
            entry.StartTime      = vm.IsVacation ? "—" : vm.StartTime;
            entry.EndTime        = vm.IsVacation ? "—" : vm.EndTime;
            entry.BreakStartTime = vm.BreakStartTime;
            entry.BreakEndTime   = vm.BreakEndTime;
            entry.SlotStatus     = vm.IsVacation ? "Blocked" : "Available";
            entry.IsVacation     = vm.IsVacation;
            entry.Notes          = vm.Notes;

            _context.SaveChanges();

            TempData["Success"] = "Schedule updated successfully.";
            return RedirectToAction("Schedule");
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  SCHEDULE — DELETE
        // ─────────────────────────────────────────────────────────────────────────
        [HttpPost]
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

        // ─────────────────────────────────────────────────────────────────────────
        //  SCHEDULE — BULK VACATION RANGE
        // ─────────────────────────────────────────────────────────────────────────
        [HttpPost]
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
                // Skip if already exists
                bool exists = _context.DoctorSchedules.Any(ds =>
                    ds.DoctorId == doctor.Id && ds.AvailableDate.Date == d);
                if (!exists)
                {
                    _context.DoctorSchedules.Add(new DoctorSchedule
                    {
                        DoctorId      = doctor.Id,
                        AvailableDate = d,
                        StartTime     = "—",
                        EndTime       = "—",
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

        // ─────────────────────────────────────────────────────────────────────────
        //  SCHEDULE — JSON for calendar rendering
        // ─────────────────────────────────────────────────────────────────────────
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
    }
}
