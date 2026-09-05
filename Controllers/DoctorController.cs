using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoctorAppointmentManagementSystem.Data;
using DoctorAppointmentManagementSystem.Models;
using DoctorAppointmentManagementSystem.ViewModels;
using DoctorAppointmentManagementSystem.Services;
using DoctorAppointmentManagementSystem.Filters;

namespace DoctorAppointmentManagementSystem.Controllers
{
    [AuthorizeRole("Doctor")]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IWebHostEnvironment _env;

        public DoctorController(ApplicationDbContext context, INotificationService notificationService, IWebHostEnvironment env)
        {
            _context = context;
            _notificationService = notificationService;
            _env = env;
        }

        private Doctor? GetCurrentDoctor()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return null;
            return _context.Doctors
                           .Include(d => d.User)
                           .FirstOrDefault(d => d.UserId == userId);
        }

        // =========================================================================
        // DASHBOARD MASTER ACTION
        // =========================================================================
        public IActionResult Dashboard(string? section, string? search, DateTime? dateFilter, string? statusFilter)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            int userId = doctor.UserId;
            ViewBag.Section = (section ?? "overview").ToLowerInvariant();
            ViewBag.Doctor = doctor;
            ViewBag.DoctorId = doctor.Id;
            ViewBag.DoctorName = doctor.User.Username;
            ViewBag.Email = doctor.User.Email;
            ViewBag.Phone = doctor.User.PhoneNumber;
            ViewBag.Specialization = doctor.Specialization;
            ViewBag.Qualification = doctor.Qualification;
            ViewBag.Experience = doctor.Experience;
            ViewBag.Fee = doctor.ConsultationFee;
            ViewBag.AvailableDays = doctor.AvailableDays;
            ViewBag.AvailableTime = doctor.AvailableTime;
            ViewBag.Biography = doctor.Biography;
            ViewBag.ProfilePicturePath = doctor.ProfilePicturePath ?? "/images/default-doctor.png";

            // Unread Notifications
            ViewBag.UnreadNotificationCount = _context.Notifications
                .Count(n => n.UserId == userId && n.NotificationStatus == "Unread");

            ViewBag.Notifications = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.SentDateTime)
                .Take(20)
                .ToList();

            // All Appointments for current doctor
            var apptQuery = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Where(a => a.DoctorId == doctor.Id)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.ToLower();
                apptQuery = apptQuery.Where(a => a.Patient.User.Username.ToLower().Contains(s) ||
                                                 a.Patient.User.Email.ToLower().Contains(s) ||
                                                 (a.ReasonForVisit != null && a.ReasonForVisit.ToLower().Contains(s)));
            }

            if (dateFilter.HasValue)
            {
                apptQuery = apptQuery.Where(a => a.AppointmentDate.Date == dateFilter.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
            {
                apptQuery = apptQuery.Where(a => a.AppointmentStatus == statusFilter);
            }

            var allAppointments = apptQuery.OrderByDescending(a => a.AppointmentDate).ToList();

            DateTime today = DateTime.Today;

            ViewBag.TotalAppointments = allAppointments.Count;
            ViewBag.TodayAppointmentsCount = allAppointments.Count(a => a.AppointmentDate.Date == today);
            ViewBag.UpcomingAppointmentsCount = allAppointments.Count(a => a.AppointmentDate.Date > today && a.AppointmentStatus != "Cancelled");
            ViewBag.PendingCount = allAppointments.Count(a => a.AppointmentStatus == "Pending");
            ViewBag.ConfirmedCount = allAppointments.Count(a => a.AppointmentStatus == "Confirmed" || a.AppointmentStatus == "Approved");
            ViewBag.CheckedInCount = allAppointments.Count(a => a.AppointmentStatus == "Checked In");
            ViewBag.InConsultationCount = allAppointments.Count(a => a.AppointmentStatus == "In Consultation");
            ViewBag.CompletedCount = allAppointments.Count(a => a.AppointmentStatus == "Completed");
            ViewBag.CancelledCount = allAppointments.Count(a => a.AppointmentStatus == "Cancelled");
            ViewBag.NoShowCount = allAppointments.Count(a => a.AppointmentStatus == "No Show");

            // Split Appointments Lists
            ViewBag.TodayAppointments = allAppointments.Where(a => a.AppointmentDate.Date == today).OrderBy(a => a.AppointmentTime).ToList();
            ViewBag.UpcomingAppointments = allAppointments.Where(a => a.AppointmentDate.Date > today && a.AppointmentStatus != "Cancelled").OrderBy(a => a.AppointmentDate).ToList();
            ViewBag.PendingAppointmentsList = allAppointments.Where(a => a.AppointmentStatus == "Pending").ToList();
            ViewBag.CompletedAppointmentsList = allAppointments.Where(a => a.AppointmentStatus == "Completed").ToList();
            ViewBag.CancelledAppointmentsList = allAppointments.Where(a => a.AppointmentStatus == "Cancelled").ToList();

            // Map payments by AppointmentId for live verified badges
            var apptIds = allAppointments.Select(a => a.Id).ToList();
            var paymentsMap = _context.Payments
                .Where(p => apptIds.Contains(p.AppointmentId))
                .GroupBy(p => p.AppointmentId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.PaymentDateTime).First());
            ViewBag.PaymentsMap = paymentsMap;

            // Revenue Calculations from Database Payments
            var docPayments = _context.Payments
                .Include(p => p.Appointment).ThenInclude(a => a.Patient).ThenInclude(pt => pt.User)
                .Where(p => p.Appointment.DoctorId == doctor.Id && (p.PaymentStatus == "Paid" || p.PaymentStatus == "Completed"))
                .OrderByDescending(p => p.PaymentDateTime)
                .ToList();

            decimal completedRevenue = docPayments.Sum(p => p.Amount);
            if (completedRevenue == 0)
            {
                completedRevenue = allAppointments.Count(a => a.AppointmentStatus == "Completed") * doctor.ConsultationFee;
            }

            decimal todayRevenue = docPayments.Where(p => p.PaymentDateTime.Date == today).Sum(p => p.Amount);
            DateTime startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            decimal weeklyRevenue = docPayments.Where(p => p.PaymentDateTime.Date >= startOfWeek).Sum(p => p.Amount);
            DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);
            decimal monthlyRevenue = docPayments.Where(p => p.PaymentDateTime.Date >= startOfMonth).Sum(p => p.Amount);

            ViewBag.TotalRevenue = completedRevenue;
            ViewBag.TodayRevenue = todayRevenue;
            ViewBag.WeeklyRevenue = weeklyRevenue;
            ViewBag.MonthlyRevenue = monthlyRevenue;
            ViewBag.DoctorPaymentsList = docPayments;

            // Doctor Schedules & Vacations
            ViewBag.Schedules = _context.DoctorSchedules
                .Where(ds => ds.DoctorId == doctor.Id)
                .OrderByDescending(ds => ds.AvailableDate)
                .ToList();

            ViewBag.TodaySlots = _context.DoctorSchedules
                .Count(ds => ds.DoctorId == doctor.Id && ds.AvailableDate.Date == today && !ds.IsVacation);

            ViewBag.UpcomingVacations = _context.DoctorSchedules
                .Count(ds => ds.DoctorId == doctor.Id && ds.IsVacation && ds.AvailableDate.Date >= today);

            // Leave Requests
            ViewBag.LeaveRequests = _context.LeaveRequests
                .Where(lr => lr.UserId == userId)
                .OrderByDescending(lr => lr.StartDate)
                .ToList();

            // System Holidays
            ViewBag.Holidays = _context.Holidays
                .OrderBy(h => h.Date)
                .ToList();

            // Patients List associated with Doctor
            var patientIds = allAppointments.Select(a => a.PatientId).Distinct().ToList();
            var patients = _context.Patients
                .Include(p => p.User)
                .Where(p => patientIds.Contains(p.Id))
                .ToList();
            ViewBag.Patients = patients;
            ViewBag.TotalPatientsCount = patients.Count;

            // EMR Medical Records
            ViewBag.MedicalRecords = _context.MedicalRecords
                .Include(mr => mr.Patient).ThenInclude(p => p.User)
                .Include(mr => mr.Appointment)
                .Where(mr => mr.DoctorId == doctor.Id)
                .OrderByDescending(mr => mr.RecordDate)
                .ToList();

            // Prescriptions
            ViewBag.Prescriptions = _context.Prescriptions
                .Include(p => p.Patient).ThenInclude(pt => pt.User)
                .Include(p => p.Appointment)
                .Where(p => p.DoctorId == doctor.Id)
                .OrderByDescending(p => p.PrescriptionDateTime)
                .ToList();

            // Medicines list for prescription form
            ViewBag.Medicines = _context.Medicines.OrderBy(m => m.Name).ToList();

            // Lab Tests & Reports
            ViewBag.LabTests = _context.LabTests.OrderBy(lt => lt.TestName).ToList();
            ViewBag.LabReports = _context.LabReports
                .Include(lr => lr.Patient).ThenInclude(p => p.User)
                .Include(lr => lr.LabTest)
                .Where(lr => patientIds.Contains(lr.PatientId))
                .OrderByDescending(lr => lr.ReportDate)
                .ToList();

            // Medical Documents
            ViewBag.MedicalDocuments = _context.MedicalDocuments
                .Include(md => md.Patient).ThenInclude(p => p.User)
                .Where(md => md.DoctorId == doctor.Id)
                .OrderByDescending(md => md.UploadDate)
                .ToList();

            // Follow-up Appointments
            ViewBag.FollowUps = _context.FollowUps
                .Include(f => f.Patient).ThenInclude(p => p.User)
                .Include(f => f.OriginalAppointment)
                .Where(f => f.DoctorId == doctor.Id)
                .OrderByDescending(f => f.FollowUpDate)
                .ToList();

            // Medical Certificates
            ViewBag.Certificates = _context.MedicalCertificates
                .Include(mc => mc.Patient).ThenInclude(p => p.User)
                .Where(mc => mc.DoctorId == doctor.Id)
                .OrderByDescending(mc => mc.IssueDate)
                .ToList();

            // Doctor Reviews & Ratings
            var reviews = _context.Feedbacks
                .Include(f => f.Patient).ThenInclude(p => p.User)
                .Where(f => f.DoctorId == doctor.Id)
                .OrderByDescending(f => f.FeedbackDateTime)
                .ToList();
            ViewBag.Reviews = reviews;
            ViewBag.AverageRating = reviews.Any() ? Math.Round(reviews.Average(r => r.Rating), 1) : 5.0;
            ViewBag.TotalReviewsCount = reviews.Count;

            // Chat Messages
            int selectedPatientUserId = 0;
            string patUserQuery = Request.Query["patientUserId"].ToString();
            if (int.TryParse(patUserQuery, out int pUserId))
            {
                selectedPatientUserId = pUserId;
            }
            else if (patients.Any())
            {
                selectedPatientUserId = patients.First().UserId;
            }

            ViewBag.SelectedPatientUserId = selectedPatientUserId;
            if (selectedPatientUserId > 0)
            {
                ViewBag.ChatMessages = _context.ChatMessages
                    .Include(cm => cm.SenderUser)
                    .Include(cm => cm.ReceiverUser)
                    .Where(cm => (cm.SenderUserId == userId && cm.ReceiverUserId == selectedPatientUserId) ||
                                (cm.SenderUserId == selectedPatientUserId && cm.ReceiverUserId == userId))
                    .OrderBy(cm => cm.SentAt)
                    .ToList();
            }
            else
            {
                ViewBag.ChatMessages = new List<ChatMessage>();
            }

            // Queue entries if queue section
            if (ViewBag.Section == "queue")
            {
                QueueManager.EnsureQueueGenerated(_context, doctor.Id, today);
                ViewBag.QueueEntries = _context.QueueEntries
                    .Include(q => q.Appointment).ThenInclude(a => a.Patient).ThenInclude(p => p.User)
                    .Where(q => q.Appointment.DoctorId == doctor.Id && q.Appointment.AppointmentDate.Date == today)
                    .OrderBy(q => q.SequenceNumber)
                    .ToList();
                ViewBag.DoctorStatus = QueueManager.GetDoctorStatus(_context, doctor.Id, today);
            }

            return View(allAppointments);
        }

        // =========================================================================
        // PROFILE MANAGEMENT
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(string specialization, string qualification, int experience, decimal consultationFee, string availableDays, string availableTime, string biography, string email, string phoneNumber)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            doctor.Specialization = specialization;
            doctor.Qualification = qualification;
            doctor.Experience = experience;
            doctor.ConsultationFee = consultationFee;
            doctor.AvailableDays = availableDays;
            doctor.AvailableTime = availableTime;
            doctor.Biography = biography;

            if (doctor.User != null)
            {
                doctor.User.Email = email;
                doctor.User.PhoneNumber = phoneNumber;
            }

            _context.SaveChanges();
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Dashboard", new { section = "profile" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfilePicture(IFormFile profilePicture)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            if (profilePicture != null && profilePicture.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "profiles");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = $"doc_{doctor.Id}_{Guid.NewGuid()}{Path.GetExtension(profilePicture.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(fileStream);
                }

                doctor.ProfilePicturePath = $"/uploads/profiles/{uniqueFileName}";
                _context.SaveChanges();
                TempData["Success"] = "Profile picture updated successfully!";
            }
            else
            {
                TempData["Error"] = "Please select a valid image file.";
            }

            return RedirectToAction("Dashboard", new { section = "profile" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            if (doctor.User.Password != currentPassword)
            {
                TempData["Error"] = "Current password is incorrect.";
                return RedirectToAction("Dashboard", new { section = "profile" });
            }

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "New passwords do not match.";
                return RedirectToAction("Dashboard", new { section = "profile" });
            }

            doctor.User.Password = newPassword;
            _context.SaveChanges();
            TempData["Success"] = "Password changed successfully!";
            return RedirectToAction("Dashboard", new { section = "profile" });
        }

        // =========================================================================
        // APPOINTMENT WORKFLOW & MANAGEMENT
        // =========================================================================
        public async Task<IActionResult> Approve(int id)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            var appt = _context.Appointments.FirstOrDefault(a => a.Id == id && a.DoctorId == doctor.Id);
            if (appt != null)
            {
                appt.AppointmentStatus = "Confirmed";
                _context.SaveChanges();
                try { await _notificationService.SendAppointmentApprovedAsync(appt); } catch { }
                TempData["Success"] = "Appointment confirmed successfully.";
            }
            else
            {
                TempData["Error"] = "Appointment not found.";
            }
            return RedirectToAction("Dashboard", new { section = "appointments" });
        }

        public async Task<IActionResult> Reject(int id)
        {
            return await CancelAppointment(id, "Rejected by doctor");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id, string? reason)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            var appt = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .FirstOrDefault(a => a.Id == id && a.DoctorId == doctor.Id);

            if (appt == null)
            {
                TempData["Error"] = "Appointment not found.";
                return RedirectToAction("Dashboard", new { section = "appointments" });
            }

            // ANTI-FRAUD RULE 1: Cannot cancel completed appointments
            if (appt.AppointmentStatus == "Completed")
            {
                TempData["Error"] = "Action Denied: This appointment has already been completed and finalized.";
                return RedirectToAction("Dashboard", new { section = "appointments" });
            }

            if (appt.AppointmentStatus == "Cancelled")
            {
                TempData["Error"] = "Appointment is already cancelled.";
                return RedirectToAction("Dashboard", new { section = "appointments" });
            }

            appt.AppointmentStatus = "Cancelled";
            string cancellationReason = string.IsNullOrWhiteSpace(reason) ? "Doctor schedule conflict / cancelled by doctor" : reason;

            // Free the doctor's schedule slot
            var slot = _context.DoctorSchedules.FirstOrDefault(ds =>
                ds.DoctorId == appt.DoctorId &&
                ds.AvailableDate.Date == appt.AppointmentDate.Date &&
                ds.StartTime == appt.AppointmentTime);
            if (slot != null)
            {
                slot.SlotStatus = "Available";
            }

            // Cancel any active queue entry
            var queueEntry = _context.QueueEntries.FirstOrDefault(q => q.AppointmentId == appt.Id);
            if (queueEntry != null)
            {
                queueEntry.Status = "Cancelled";
            }

            // ANTI-FRAUD RULE 2: If appointment was PAID, initiate full refund and alert patient
            var payment = _context.Payments.FirstOrDefault(p => p.AppointmentId == appt.Id);
            if (payment != null && (payment.PaymentStatus == "Paid" || payment.PaymentStatus == "Completed"))
            {
                payment.PaymentStatus = "Refunded";

                var invoice = _context.Invoices.FirstOrDefault(i => i.AppointmentId == appt.Id);
                if (invoice != null)
                {
                    invoice.Status = "Cancelled / Refunded";
                }

                try
                {
                    await _notificationService.SendAppointmentRefundNotificationAsync(appt, payment, cancellationReason);
                }
                catch { }

                var adminUser = _context.Users.FirstOrDefault(u => u.RoleId == 1);
                _context.AdminLogs.Add(new AdminLog
                {
                    AdminId = adminUser?.Id ?? doctor.UserId,
                    ActionPerformed = $"Doctor Cancelled Paid Appointment #{appt.Id}. Refund initiated for ৳{payment.Amount:N2} ({payment.PaymentMethod}). Reason: {cancellationReason}",
                    Description = $"Patient: {appt.Patient?.User?.Username}, Doctor: {doctor.User?.Username}, TrxID: {payment.TransactionId}",
                    ActionDateTime = DateTime.Now
                });

                TempData["Success"] = $"Appointment #{appt.Id} cancelled. Patient was notified and 100% refund of ৳{payment.Amount:N2} was initiated.";
            }
            else
            {
                try { await _notificationService.SendAppointmentCancelledAsync(appt); } catch { }
                TempData["Success"] = $"Appointment #{appt.Id} cancelled successfully.";
            }

            _context.SaveChanges();
            return RedirectToAction("Dashboard", new { section = "appointments" });
        }

        public IActionResult Complete(int id)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            var appt = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .FirstOrDefault(a => a.Id == id && a.DoctorId == doctor.Id);

            if (appt == null)
            {
                TempData["Error"] = "Appointment not found.";
                return RedirectToAction("Dashboard", new { section = "appointments" });
            }

            if (appt.AppointmentStatus == "Completed")
            {
                TempData["Error"] = "Appointment is already completed.";
                return RedirectToAction("Dashboard", new { section = "appointments" });
            }

            if (appt.AppointmentStatus == "Cancelled")
            {
                TempData["Error"] = "Action Denied: Cannot complete a cancelled appointment.";
                return RedirectToAction("Dashboard", new { section = "appointments" });
            }

            appt.AppointmentStatus = "Completed";

            // Update queue entry
            var queueEntry = _context.QueueEntries.FirstOrDefault(q => q.AppointmentId == appt.Id);
            if (queueEntry != null)
            {
                queueEntry.Status = "Completed";
                queueEntry.CompletionTime = DateTime.Now;
            }

            decimal docFee = doctor.ConsultationFee;
            decimal ticketFee = 50;
            decimal total = docFee + ticketFee;

            var existingInvoice = _context.Invoices.FirstOrDefault(i => i.AppointmentId == appt.Id);
            if (existingInvoice == null)
            {
                var invoice = new Invoice
                {
                    PatientId = appt.PatientId,
                    AppointmentId = appt.Id,
                    TotalAmount = total,
                    IssueDate = DateTime.Now,
                    Status = "Paid",
                    Particulars = $"Doctor Consultation Fee (৳{docFee}) + Hospital Ticket / Booking Fee (৳{ticketFee}) for Dr. {doctor.User?.Username ?? "Doctor"} ({doctor.Specialization})"
                };
                _context.Invoices.Add(invoice);
            }
            else
            {
                existingInvoice.Status = "Paid";
            }

            if (appt.Patient?.UserId != null)
            {
                var notification = new Notification
                {
                    UserId = appt.Patient.UserId,
                    NotificationType = "Invoice",
                    Title = $"Consultation Completed (Appt #{appt.Id})",
                    Message = $"Dr. {doctor.User?.Username ?? "Doctor"} has completed your appointment. Total Bill: ৳{total} (Consultation Fee: ৳{docFee}, Ticket Fee: ৳{ticketFee}). Your invoice has been updated in your dashboard.",
                    SentDateTime = DateTime.Now,
                    NotificationStatus = "Unread"
                };
                _context.Notifications.Add(notification);
            }

            _context.SaveChanges();
            TempData["Success"] = "Appointment marked as Completed. Payment invoice and notification generated for patient.";
            return RedirectToAction("Dashboard", new { section = "appointments" });
        }

        public async Task<IActionResult> Delay(int id)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            var appt = _context.Appointments.FirstOrDefault(a => a.Id == id && a.DoctorId == doctor.Id);
            if (appt != null)
            {
                if (appt.AppointmentStatus == "Completed" || appt.AppointmentStatus == "Cancelled")
                {
                    TempData["Error"] = "Cannot delay a completed or cancelled appointment.";
                    return RedirectToAction("Dashboard", new { section = "appointments" });
                }

                appt.AppointmentStatus = "Delayed";
                _context.SaveChanges();
                try { await _notificationService.SendAppointmentDelayedAsync(appt); } catch { }
                TempData["Success"] = "Appointment marked as Delayed.";
            }
            else
            {
                TempData["Error"] = "Appointment not found.";
            }
            return RedirectToAction("Dashboard", new { section = "appointments" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, string status)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            var appt = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .FirstOrDefault(a => a.Id == id && a.DoctorId == doctor.Id);

            if (appt == null)
            {
                TempData["Error"] = "Appointment not found.";
                return RedirectToAction("Dashboard", new { section = "appointments" });
            }

            // ANTI-FRAUD RULE 1: If already Completed, cannot modify
            if (appt.AppointmentStatus == "Completed")
            {
                TempData["Error"] = "Action Denied: This appointment has already been completed and finalized.";
                return RedirectToAction("Dashboard", new { section = "appointments" });
            }

            // ANTI-FRAUD RULE 2: If already Cancelled, cannot modify
            if (appt.AppointmentStatus == "Cancelled")
            {
                TempData["Error"] = "Action Denied: This appointment has already been cancelled.";
                return RedirectToAction("Dashboard", new { section = "appointments" });
            }

            // ANTI-FRAUD RULE 3: Cannot revert to Pending
            if (status == "Pending")
            {
                TempData["Error"] = "Action Denied: Cannot revert confirmed appointment to Pending.";
                return RedirectToAction("Dashboard", new { section = "appointments" });
            }

            // ANTI-FRAUD RULE 4: If changing to Cancelled, process full refund
            if (status == "Cancelled")
            {
                return await CancelAppointment(id, "Cancelled by Doctor via status control");
            }

            if (status == "Completed")
            {
                return Complete(id);
            }

            appt.AppointmentStatus = status;

            // Update queue entry if applicable
            var queueEntry = _context.QueueEntries.FirstOrDefault(q => q.AppointmentId == appt.Id);
            if (queueEntry != null)
            {
                if (status == "Checked In") queueEntry.Status = "Waiting";
                else if (status == "In Consultation") queueEntry.Status = "InConsultation";
            }

            _context.SaveChanges();
            TempData["Success"] = $"Appointment #{id} status updated to {status}.";
            return RedirectToAction("Dashboard", new { section = "appointments" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RescheduleAppointment(int id, DateTime newDate, string newTime)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            var appt = _context.Appointments.FirstOrDefault(a => a.Id == id && a.DoctorId == doctor.Id);
            if (appt != null)
            {
                appt.AppointmentDate = newDate;
                appt.AppointmentTime = newTime;
                appt.AppointmentStatus = "Confirmed";
                _context.SaveChanges();
                TempData["Success"] = $"Appointment #{id} rescheduled to {newDate:MMM dd, yyyy} at {newTime}.";
            }
            return RedirectToAction("Dashboard", new { section = "appointments" });
        }

        // =========================================================================
        // EMR & MEDICAL RECORDS MANAGEMENT
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveEMR(int patientId, int? appointmentId, string diagnosis, string? symptoms, string? treatmentPlan, string? vitalSigns, string? allergies, string? chronicDiseases, string? followUpNotes, string? notes)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            var record = new MedicalRecord
            {
                PatientId = patientId,
                DoctorId = doctor.Id,
                AppointmentId = appointmentId,
                Diagnosis = diagnosis,
                Symptoms = symptoms,
                TreatmentPlan = treatmentPlan,
                TreatmentDetails = treatmentPlan,
                VitalSigns = vitalSigns,
                Allergies = allergies,
                ChronicDiseases = chronicDiseases,
                FollowUpNotes = followUpNotes,
                Notes = notes,
                RecordDate = DateTime.Now
            };

            _context.MedicalRecords.Add(record);

            if (appointmentId.HasValue)
            {
                var appt = _context.Appointments.FirstOrDefault(a => a.Id == appointmentId.Value);
                if (appt != null)
                {
                    appt.AppointmentStatus = "Completed";
                }
            }

            // Also update patient record allergies/chronic diseases if provided
            var patient = _context.Patients.FirstOrDefault(p => p.Id == patientId);
            if (patient != null)
            {
                if (!string.IsNullOrWhiteSpace(allergies)) patient.Allergies = allergies;
                if (!string.IsNullOrWhiteSpace(chronicDiseases)) patient.ChronicDiseases = chronicDiseases;
            }

            _context.SaveChanges();
            TempData["Success"] = "Electronic Medical Record (EMR) saved successfully!";
            return RedirectToAction("Dashboard", new { section = "patients", patientId = patientId });
        }

        // =========================================================================
        // PRESCRIPTION MANAGEMENT
        // =========================================================================
        public IActionResult AddPrescription(int appointmentId)
        {
            ViewBag.AppointmentId = appointmentId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPrescription(Prescription model)
        {
            var appointment = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .FirstOrDefault(a => a.Id == model.AppointmentId);

            if (appointment != null)
            {
                model.DoctorId = appointment.DoctorId;
                model.PatientId = appointment.PatientId;
                appointment.AppointmentStatus = "Completed";

                // Automatically update queue entry to Completed
                var queueEntry = _context.QueueEntries.FirstOrDefault(q => q.AppointmentId == appointment.Id);
                if (queueEntry != null)
                {
                    queueEntry.Status = "Completed";
                    queueEntry.CompletionTime = DateTime.Now;
                }

                decimal docFee = appointment.Doctor?.ConsultationFee ?? 500;
                decimal ticketFee = 50;
                decimal total = docFee + ticketFee;

                var existingInvoice = _context.Invoices.FirstOrDefault(i => i.AppointmentId == appointment.Id);
                if (existingInvoice == null)
                {
                    var invoice = new Invoice
                    {
                        PatientId = appointment.PatientId,
                        AppointmentId = appointment.Id,
                        TotalAmount = total,
                        IssueDate = DateTime.Now,
                        Status = "Paid",
                        Particulars = $"Doctor Consultation Fee (৳{docFee}) + Hospital Ticket / Booking Fee (৳{ticketFee}) for Dr. {appointment.Doctor?.User?.Username ?? "Doctor"} ({appointment.Doctor?.Specialization ?? ""})"
                    };
                    _context.Invoices.Add(invoice);
                }
                else
                {
                    existingInvoice.Status = "Paid";
                }

                if (appointment.Patient?.UserId != null)
                {
                    var notification = new Notification
                    {
                        UserId = appointment.Patient.UserId,
                        NotificationType = "Prescription",
                        Title = $"Prescription Issued & Completed (Appt #{appointment.Id})",
                        Message = $"Dr. {appointment.Doctor?.User?.Username ?? "Doctor"} has issued your digital prescription and marked appointment #{appointment.Id} as Completed.",
                        SentDateTime = DateTime.Now,
                        NotificationStatus = "Unread"
                    };
                    _context.Notifications.Add(notification);
                }
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

            TempData["Success"] = $"Prescription created successfully! Appointment #{model.AppointmentId} is now marked as Completed.";
            return RedirectToAction("Dashboard", new { section = "prescriptions" });
        }

        [AuthorizeRole("Doctor", "Patient", "Admin")]
        public IActionResult PrintPrescription(int id)
        {
            var prescription = _context.Prescriptions
                .Include(p => p.Doctor).ThenInclude(d => d.User)
                .Include(p => p.Patient).ThenInclude(pt => pt.User)
                .Include(p => p.Appointment)
                .FirstOrDefault(p => p.Id == id);

            if (prescription == null) return NotFound();
            return View(prescription);
        }

        // =========================================================================
        // LABORATORY TESTS MANAGEMENT
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RequestLabTest(int patientId, int labTestId, string? remarks)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            var report = new LabReport
            {
                PatientId = patientId,
                LabTestId = labTestId,
                ReportDate = DateTime.Now,
                Result = "Pending",
                Remarks = remarks ?? "Test requested by Doctor"
            };

            _context.LabReports.Add(report);
            _context.SaveChanges();
            TempData["Success"] = "Laboratory test requested successfully!";
            return RedirectToAction("Dashboard", new { section = "lab" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateLabReportResult(int reportId, string result, string? remarks)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            var report = _context.LabReports.FirstOrDefault(lr => lr.Id == reportId);
            if (report != null)
            {
                report.Result = result;
                if (!string.IsNullOrWhiteSpace(remarks))
                {
                    report.Remarks = remarks;
                }
                _context.SaveChanges();
                TempData["Success"] = "Laboratory test result updated successfully!";
            }
            return RedirectToAction("Dashboard", new { section = "lab" });
        }

        // =========================================================================
        // MEDICAL DOCUMENTS UPLOAD MANAGEMENT
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadMedicalDocument(int patientId, string documentName, string documentType, IFormFile documentFile, string? notes)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            if (documentFile != null && documentFile.Length > 0)
            {
                string docsFolder = Path.Combine(_env.WebRootPath, "uploads", "medical_documents");
                if (!Directory.Exists(docsFolder)) Directory.CreateDirectory(docsFolder);

                string fileName = $"doc_{patientId}_{Guid.NewGuid()}{Path.GetExtension(documentFile.FileName)}";
                string filePath = Path.Combine(docsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await documentFile.CopyToAsync(stream);
                }

                var doc = new MedicalDocument
                {
                    PatientId = patientId,
                    DoctorId = doctor.Id,
                    DocumentName = documentName,
                    DocumentType = documentType,
                    FilePath = $"/uploads/medical_documents/{fileName}",
                    UploadDate = DateTime.Now,
                    Notes = notes
                };

                _context.MedicalDocuments.Add(doc);
                _context.SaveChanges();
                TempData["Success"] = "Medical document uploaded successfully!";
            }
            else
            {
                TempData["Error"] = "Please select a valid document file.";
            }

            return RedirectToAction("Dashboard", new { section = "documents" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteMedicalDocument(int id)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            var doc = _context.MedicalDocuments.FirstOrDefault(d => d.Id == id && d.DoctorId == doctor.Id);
            if (doc != null)
            {
                _context.MedicalDocuments.Remove(doc);
                _context.SaveChanges();
                TempData["Success"] = "Medical document removed successfully.";
            }
            return RedirectToAction("Dashboard", new { section = "documents" });
        }

        // =========================================================================
        // FOLLOW-UP APPOINTMENT MANAGEMENT
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ScheduleFollowUp(int patientId, int? originalAppointmentId, DateTime followUpDate, string? notes)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            var followUp = new FollowUp
            {
                DoctorId = doctor.Id,
                PatientId = patientId,
                OriginalAppointmentId = originalAppointmentId,
                FollowUpDate = followUpDate,
                Notes = notes,
                Status = "Scheduled"
            };

            _context.FollowUps.Add(followUp);

            // Create a follow-up appointment entry as well
            var appt = new Appointment
            {
                DoctorId = doctor.Id,
                PatientId = patientId,
                AppointmentDate = followUpDate,
                AppointmentTime = "10:00 AM",
                ReasonForVisit = $"Follow-up: {notes ?? "Routine checkup"}",
                AppointmentStatus = "Confirmed",
                BookingDateTime = DateTime.Now
            };

            _context.Appointments.Add(appt);
            _context.SaveChanges();

            TempData["Success"] = $"Follow-up appointment scheduled for {followUpDate:MMM dd, yyyy}.";
            return RedirectToAction("Dashboard", new { section = "followup" });
        }

        // =========================================================================
        // CERTIFICATE GENERATOR
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GenerateCertificate(int patientId, string certificateType, DateTime startDate, DateTime endDate, string diagnosis, string? remarks)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            string certNo = $"CERT-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
            var cert = new MedicalCertificate
            {
                DoctorId = doctor.Id,
                PatientId = patientId,
                CertificateNumber = certNo,
                CertificateType = certificateType,
                IssueDate = DateTime.Now,
                StartDate = startDate,
                EndDate = endDate,
                Diagnosis = diagnosis,
                Remarks = remarks
            };

            _context.MedicalCertificates.Add(cert);
            _context.SaveChanges();

            TempData["Success"] = $"Certificate #{certNo} generated successfully!";
            return RedirectToAction("Dashboard", new { section = "certificates" });
        }

        public IActionResult PrintCertificate(int id)
        {
            var cert = _context.MedicalCertificates
                .Include(c => c.Doctor).ThenInclude(d => d.User)
                .Include(c => c.Patient).ThenInclude(p => p.User)
                .FirstOrDefault(c => c.Id == id);

            if (cert == null) return NotFound();
            return View(cert);
        }

        [AuthorizeRole("Doctor", "Patient", "Admin")]
        public IActionResult PrintInvoice(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.Patient).ThenInclude(p => p.User)
                .Include(i => i.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.User)
                .FirstOrDefault(i => i.Id == id || i.AppointmentId == id);

            if (invoice == null) return NotFound();

            var payment = _context.Payments
                .FirstOrDefault(p => p.AppointmentId == invoice.AppointmentId || p.PatientId == invoice.PatientId);
            ViewBag.Payment = payment;

            return View("~/Views/Patient/PrintInvoice.cshtml", invoice);
        }

        // =========================================================================
        // COMMUNICATION & IN-APP CHAT
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendChatMessage(int receiverUserId, string message)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            if (!string.IsNullOrWhiteSpace(message))
            {
                var chat = new ChatMessage
                {
                    SenderUserId = doctor.UserId,
                    ReceiverUserId = receiverUserId,
                    Message = message,
                    SentAt = DateTime.Now,
                    IsRead = false
                };

                _context.ChatMessages.Add(chat);
                _context.SaveChanges();
            }

            return RedirectToAction("Dashboard", new { section = "messages", patientUserId = receiverUserId });
        }

        // =========================================================================
        // SCHEDULE & LEAVE MANAGEMENT
        // =========================================================================
        public IActionResult Schedule(int? month, int? year)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            int m = month ?? DateTime.Today.Month;
            int y = year ?? DateTime.Today.Year;

            var schedules = _context.DoctorSchedules
                .Where(ds => ds.DoctorId == doctor.Id && ds.AvailableDate.Month == m && ds.AvailableDate.Year == y)
                .OrderBy(ds => ds.AvailableDate)
                .ThenBy(ds => ds.StartTime)
                .ToList();

            ViewBag.DoctorId = doctor.Id;
            ViewBag.DoctorName = doctor.User.Username;
            ViewBag.Month = m;
            ViewBag.Year = y;
            ViewBag.MonthName = new DateTime(y, m, 1).ToString("MMMM yyyy");
            ViewBag.DaysInMonth = DateTime.DaysInMonth(y, m);
            ViewBag.FirstDayOfWeek = (int)new DateTime(y, m, 1).DayOfWeek;

            var bookedDates = _context.Appointments
                .Where(a => a.DoctorId == doctor.Id && a.AppointmentDate.Month == m && a.AppointmentDate.Year == y && a.AppointmentStatus != "Cancelled")
                .GroupBy(a => a.AppointmentDate.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.BookedCounts = bookedDates;

            return View(schedules);
        }

        public IActionResult AddSchedule()
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");
            return View(new DoctorScheduleViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                    ModelState.AddModelError("", "A schedule already exists for this date. Edit the existing entry instead.");
                    return View(vm);
                }
            }

            var schedule = new DoctorSchedule
            {
                DoctorId = doctor.Id,
                AvailableDate = vm.AvailableDate,
                StartTime = vm.IsVacation ? "—" : vm.StartTime,
                EndTime = vm.IsVacation ? "—" : vm.EndTime,
                BreakStartTime = vm.BreakStartTime,
                BreakEndTime = vm.BreakEndTime,
                SlotStatus = vm.IsVacation ? "Blocked" : "Available",
                IsVacation = vm.IsVacation,
                Notes = vm.Notes
            };

            _context.DoctorSchedules.Add(schedule);
            _context.SaveChanges();

            TempData["Success"] = vm.IsVacation
                ? $"Vacation day marked for {vm.AvailableDate:MMMM dd, yyyy}."
                : $"Schedule added for {vm.AvailableDate:MMMM dd, yyyy} ({vm.StartTime} – {vm.EndTime}).";

            return RedirectToAction("Dashboard", new { section = "schedule" });
        }

        public IActionResult EditSchedule(int id)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            var entry = _context.DoctorSchedules.FirstOrDefault(ds => ds.Id == id && ds.DoctorId == doctor.Id);
            if (entry == null) return NotFound();

            var vm = new DoctorScheduleViewModel
            {
                Id = entry.Id,
                AvailableDate = entry.AvailableDate,
                StartTime = entry.StartTime,
                EndTime = entry.EndTime,
                BreakStartTime = entry.BreakStartTime,
                BreakEndTime = entry.BreakEndTime,
                IsVacation = entry.IsVacation,
                Notes = entry.Notes
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSchedule(DoctorScheduleViewModel vm)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(vm);

            var entry = _context.DoctorSchedules.FirstOrDefault(ds => ds.Id == vm.Id && ds.DoctorId == doctor.Id);
            if (entry == null) return NotFound();

            entry.AvailableDate = vm.AvailableDate;
            entry.StartTime = vm.IsVacation ? "—" : vm.StartTime;
            entry.EndTime = vm.IsVacation ? "—" : vm.EndTime;
            entry.BreakStartTime = vm.BreakStartTime;
            entry.BreakEndTime = vm.BreakEndTime;
            entry.SlotStatus = vm.IsVacation ? "Blocked" : "Available";
            entry.IsVacation = vm.IsVacation;
            entry.Notes = vm.Notes;

            _context.SaveChanges();

            TempData["Success"] = "Schedule updated successfully.";
            return RedirectToAction("Dashboard", new { section = "schedule" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSchedule(int id)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            var entry = _context.DoctorSchedules.FirstOrDefault(ds => ds.Id == id && ds.DoctorId == doctor.Id);
            if (entry != null)
            {
                _context.DoctorSchedules.Remove(entry);
                _context.SaveChanges();
                TempData["Success"] = "Schedule entry removed.";
            }
            return RedirectToAction("Dashboard", new { section = "schedule" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetVacationRange(DateTime from, DateTime to, string? notes)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            if (from > to)
            {
                TempData["Error"] = "Start date must be before end date.";
                return RedirectToAction("Dashboard", new { section = "schedule" });
            }

            int added = 0;
            for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
            {
                bool exists = _context.DoctorSchedules.Any(ds => ds.DoctorId == doctor.Id && ds.AvailableDate.Date == d);
                if (!exists)
                {
                    _context.DoctorSchedules.Add(new DoctorSchedule
                    {
                        DoctorId = doctor.Id,
                        AvailableDate = d,
                        StartTime = "—",
                        EndTime = "—",
                        SlotStatus = "Blocked",
                        IsVacation = true,
                        Notes = notes ?? "Vacation"
                    });
                    added++;
                }
            }
            _context.SaveChanges();

            TempData["Success"] = $"Vacation set for {added} day(s) from {from:MMM dd} to {to:MMM dd, yyyy}.";
            return RedirectToAction("Dashboard", new { section = "schedule" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RequestLeave(DateTime startDate, DateTime endDate, string reason)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            if (startDate > endDate)
            {
                TempData["Error"] = "Start date must be before end date.";
                return RedirectToAction("Dashboard", new { section = "schedule" });
            }

            var lr = new LeaveRequest
            {
                UserId = doctor.UserId,
                StartDate = startDate,
                EndDate = endDate,
                Reason = reason,
                Status = "Pending"
            };

            _context.LeaveRequests.Add(lr);
            _context.SaveChanges();
            TempData["Success"] = "Leave request submitted to Admin.";
            return RedirectToAction("Dashboard", new { section = "schedule" });
        }

        public JsonResult GetScheduleJson(int month, int year)
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return Json(new List<object>());

            var schedules = _context.DoctorSchedules
                .Where(ds => ds.DoctorId == doctor.Id && ds.AvailableDate.Month == month && ds.AvailableDate.Year == year)
                .Select(ds => new
                {
                    id = ds.Id,
                    date = ds.AvailableDate.ToString("yyyy-MM-dd"),
                    startTime = ds.StartTime,
                    endTime = ds.EndTime,
                    breakStart = ds.BreakStartTime,
                    breakEnd = ds.BreakEndTime,
                    slotStatus = ds.SlotStatus,
                    isVacation = ds.IsVacation,
                    notes = ds.Notes
                })
                .ToList();

            return Json(schedules);
        }

        // =========================================================================
        // NOTIFICATIONS MANAGEMENT
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
        public IActionResult MarkAllNotificationsRead()
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return RedirectToAction("Login", "Account");

            int userId = doctor.UserId;
            var unread = _context.Notifications.Where(n => n.UserId == userId && n.NotificationStatus == "Unread").ToList();
            foreach (var n in unread) n.NotificationStatus = "Read";
            _context.SaveChanges();
            return RedirectToAction("Dashboard", new { section = "notifications" });
        }

        // =========================================================================
        // APEXCHARTS JSON API FOR DASHBOARD & EARNINGS
        // =========================================================================
        public JsonResult GetEarningsChartData()
        {
            var doctor = GetCurrentDoctor();
            if (doctor == null) return Json(new { success = false });

            var today = DateTime.Today;
            var last7Days = Enumerable.Range(0, 7).Select(i => today.AddDays(-6 + i)).ToList();

            var dailyEarnings = last7Days.Select(d => new
            {
                day = d.ToString("ddd (MMM dd)"),
                earnings = _context.Payments
                    .Where(p => p.Appointment.DoctorId == doctor.Id && (p.PaymentStatus == "Paid" || p.PaymentStatus == "Completed") && p.PaymentDateTime.Date == d)
                    .Sum(p => (decimal?)p.Amount) ?? (_context.Appointments.Count(a => a.DoctorId == doctor.Id && a.AppointmentStatus == "Completed" && a.AppointmentDate.Date == d) * doctor.ConsultationFee)
            }).ToList();

            var last6Months = Enumerable.Range(0, 6).Select(i => today.AddMonths(-5 + i)).ToList();
            var monthlyTrends = last6Months.Select(m => new
            {
                month = m.ToString("MMM yyyy"),
                appointments = _context.Appointments.Count(a => a.DoctorId == doctor.Id && a.AppointmentDate.Month == m.Month && a.AppointmentDate.Year == m.Year),
                revenue = _context.Payments
                    .Where(p => p.Appointment.DoctorId == doctor.Id && (p.PaymentStatus == "Paid" || p.PaymentStatus == "Completed") && p.PaymentDateTime.Month == m.Month && p.PaymentDateTime.Year == m.Year)
                    .Sum(p => (decimal?)p.Amount) ?? (_context.Appointments.Count(a => a.DoctorId == doctor.Id && a.AppointmentStatus == "Completed" && a.AppointmentDate.Month == m.Month && a.AppointmentDate.Year == m.Year) * doctor.ConsultationFee)
            }).ToList();

            return Json(new
            {
                success = true,
                dailyEarnings,
                monthlyTrends
            });
        }
    }
}
