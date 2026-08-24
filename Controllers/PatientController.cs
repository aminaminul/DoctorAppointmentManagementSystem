using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DoctorAppointmentManagementSystem.Data;
using DoctorAppointmentManagementSystem.Models;
using DoctorAppointmentManagementSystem.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointmentManagementSystem.Controllers
{
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IWebHostEnvironment _env;

        public PatientController(ApplicationDbContext context, INotificationService notificationService, IWebHostEnvironment env)
        {
            _context = context;
            _notificationService = notificationService;
            _env = env;
        }

        private Patient? GetCurrentPatient()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return null;

            var patient = _context.Patients
                .Include(p => p.User)
                .FirstOrDefault(p => p.UserId == userId);

            if (patient == null)
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    patient = new Patient
                    {
                        UserId = user.Id,
                        Gender = "Male",
                        DateOfBirth = DateTime.Today.AddYears(-20)
                    };
                    _context.Patients.Add(patient);
                    _context.SaveChanges();
                    patient.User = user;
                }
            }

            return patient;
        }

        // =========================================================================
        // PATIENT DASHBOARD & SECTION SWITCHING
        // =========================================================================
        public IActionResult Dashboard(string? section, string? search, string? specFilter, decimal? maxFee, int? minRating)
        {
            var patient = GetCurrentPatient();
            if (patient == null) return RedirectToAction("Login", "Account");

            string currentSection = (section ?? "overview").ToLowerInvariant();
            ViewBag.Section = currentSection;
            ViewBag.Patient = patient;
            ViewBag.PatientName = patient.User?.Username ?? "Patient";
            ViewBag.Email = patient.User?.Email;
            ViewBag.Age = patient.Age;
            ViewBag.Gender = patient.Gender;
            ViewBag.BloodGroup = patient.BloodGroup;
            ViewBag.Address = patient.Address;
            ViewBag.EmergencyContact = patient.EmergencyContact;

            int userId = patient.UserId;

            // Notifications Count
            ViewBag.UnreadNotificationCount = _context.Notifications
                .Count(n => n.UserId == userId && n.NotificationStatus == "Unread");

            var allAppointments = _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Where(a => a.PatientId == patient.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();

            ViewBag.Appointments = allAppointments;
            ViewBag.TotalAppointments = allAppointments.Count;
            ViewBag.PendingAppointments = allAppointments.Count(a => a.AppointmentStatus == "Pending");
            ViewBag.CompletedAppointments = allAppointments.Count(a => a.AppointmentStatus == "Completed");
            ViewBag.UpcomingAppointmentsList = allAppointments.Where(a => a.AppointmentDate.Date >= DateTime.Today && a.AppointmentStatus != "Cancelled").ToList();

            // Next Upcoming Appointment Card
            var nextAppt = allAppointments
                .Where(a => a.AppointmentDate.Date >= DateTime.Today && a.AppointmentStatus != "Cancelled")
                .OrderBy(a => a.AppointmentDate)
                .FirstOrDefault();
            ViewBag.NextAppointment = nextAppt;

            // Stats
            var payments = _context.Payments.Where(p => p.PatientId == patient.Id).ToList();
            ViewBag.TotalSpent = payments.Sum(p => p.Amount);
            ViewBag.Payments = payments;

            // Prescriptions
            var prescriptions = _context.Prescriptions
                .Include(p => p.Doctor).ThenInclude(d => d.User)
                .Where(p => p.PatientId == patient.Id)
                .OrderByDescending(p => p.PrescriptionDateTime)
                .ToList();
            ViewBag.Prescriptions = prescriptions;

            // Medical Records
            var medicalRecords = _context.MedicalRecords
                .Include(mr => mr.Doctor).ThenInclude(d => d.User)
                .Where(mr => mr.PatientId == patient.Id)
                .OrderByDescending(mr => mr.RecordDate)
                .ToList();
            ViewBag.MedicalRecords = medicalRecords;

            // Lab Reports & Tests
            var labReports = _context.LabReports
                .Include(lr => lr.Patient).ThenInclude(p => p.User)
                .Where(lr => lr.PatientId == patient.Id)
                .OrderByDescending(lr => lr.ReportDate)
                .ToList();
            ViewBag.LabReports = labReports;

            // Medical Documents
            var medicalDocuments = _context.MedicalDocuments
                .Include(md => md.Doctor).ThenInclude(d => d.User)
                .Where(md => md.PatientId == patient.Id)
                .OrderByDescending(md => md.UploadDate)
                .ToList();
            ViewBag.MedicalDocuments = medicalDocuments;

            // Family Members
            var familyMembers = _context.FamilyMembers
                .Where(fm => fm.PatientId == patient.Id)
                .ToList();
            ViewBag.FamilyMembers = familyMembers;

            // Insurance Info
            var insurance = _context.InsuranceInfos
                .FirstOrDefault(i => i.PatientId == patient.Id);
            ViewBag.Insurance = insurance;

            // Notifications List
            ViewBag.Notifications = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.SentDateTime)
                .Take(15)
                .ToList();

            // Doctors List for Search & Booking
            var doctorsQuery = _context.Doctors
                .Include(d => d.User)
                .Where(d => d.ActiveStatus)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                doctorsQuery = doctorsQuery.Where(d => d.User.Username.Contains(search) || d.Specialization.Contains(search));
            }
            if (!string.IsNullOrWhiteSpace(specFilter))
            {
                doctorsQuery = doctorsQuery.Where(d => d.Specialization.Contains(specFilter));
            }
            if (maxFee.HasValue && maxFee > 0)
            {
                doctorsQuery = doctorsQuery.Where(d => d.ConsultationFee <= maxFee.Value);
            }

            var doctors = doctorsQuery.ToList();
            ViewBag.Doctors = doctors;
            ViewBag.Specializations = _context.Doctors.Select(d => d.Specialization).Distinct().ToList();

            // Reviews given by patient
            var patientReviews = _context.Feedbacks
                .Include(f => f.Doctor).ThenInclude(d => d.User)
                .Where(f => f.PatientId == patient.Id)
                .OrderByDescending(f => f.FeedbackDateTime)
                .ToList();
            ViewBag.Reviews = patientReviews;

            // Doctor Ratings Summary
            var allReviews = _context.Feedbacks.ToList();
            ViewBag.AllReviews = allReviews;

            // Chat Messages
            int selectedDoctorUserId = 0;
            string docUserQuery = Request.Query["doctorUserId"].ToString();
            if (int.TryParse(docUserQuery, out int dUserId))
            {
                selectedDoctorUserId = dUserId;
            }
            else if (doctors.Any())
            {
                selectedDoctorUserId = doctors.First().UserId;
            }

            ViewBag.SelectedDoctorUserId = selectedDoctorUserId;
            if (selectedDoctorUserId > 0)
            {
                ViewBag.ChatMessages = _context.ChatMessages
                    .Include(c => c.SenderUser)
                    .Include(c => c.ReceiverUser)
                    .Where(c => (c.SenderUserId == userId && c.ReceiverUserId == selectedDoctorUserId) ||
                                (c.SenderUserId == selectedDoctorUserId && c.ReceiverUserId == userId))
                    .OrderBy(c => c.SentAt)
                    .ToList();
            }

            return View();
        }

        // =========================================================================
        // APPOINTMENT BOOKING & WORKFLOW
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(int doctorId, DateTime appointmentDate, string appointmentTime, string? reasonForVisit, int? familyMemberId, bool isEmergency)
        {
            var patient = GetCurrentPatient();
            if (patient == null) return RedirectToAction("Login", "Account");

            var doctor = _context.Doctors.Include(d => d.User).FirstOrDefault(d => d.Id == doctorId);
            if (doctor == null)
            {
                TempData["Error"] = "Selected doctor not found.";
                return RedirectToAction("Dashboard", new { section = "doctors" });
            }

            var appointment = new Appointment
            {
                PatientId = patient.Id,
                DoctorId = doctorId,
                AppointmentDate = appointmentDate,
                AppointmentTime = appointmentTime,
                ReasonForVisit = string.IsNullOrWhiteSpace(reasonForVisit) ? "Routine Consultation" : reasonForVisit,
                AppointmentStatus = "Pending",
                IsEmergency = isEmergency,
                BookingDateTime = DateTime.Now
            };

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            // Auto-Generate Notification
            try
            {
                var notification = new Notification
                {
                    UserId = patient.UserId,
                    Message = $"Your appointment request with Dr. {doctor.User?.Username} for {appointmentDate.ToString("dd MMM yyyy")} at {appointmentTime} has been submitted.",
                    SentDateTime = DateTime.Now,
                    NotificationStatus = "Unread"
                };
                _context.Notifications.Add(notification);
                _context.SaveChanges();
            }
            catch { }

            TempData["Success"] = "Appointment booked successfully! Status: Pending Approval.";
            return RedirectToAction("Dashboard", new { section = "appointments" });
        }

        // =========================================================================
        // DOCTOR PROFILE VIEW
        // =========================================================================
        public IActionResult DoctorProfile(int id)
        {
            var patient = GetCurrentPatient();
            if (patient == null) return RedirectToAction("Login", "Account");

            var doctor = _context.Doctors
                .Include(d => d.User)
                .FirstOrDefault(d => d.Id == id);

            if (doctor == null) return NotFound();

            // Get doctor's feedback/reviews with patient info
            var reviews = _context.Feedbacks
                .Include(f => f.Patient).ThenInclude(p => p!.User)
                .Where(f => f.DoctorId == id)
                .OrderByDescending(f => f.FeedbackDateTime)
                .ToList();

            // Get total appointment count for this doctor
            var totalPatients = _context.Appointments
                .Where(a => a.DoctorId == id && a.AppointmentStatus == "Completed")
                .Select(a => a.PatientId)
                .Distinct()
                .Count();

            // Get available schedules
            var schedules = _context.DoctorSchedules
                .Where(s => s.DoctorId == id && !s.IsVacation && s.AvailableDate >= DateTime.Today)
                .OrderBy(s => s.AvailableDate)
                .Take(7)
                .ToList();

            ViewBag.Reviews = reviews;
            ViewBag.TotalPatients = totalPatients;
            ViewBag.Schedules = schedules;
            ViewBag.AvgRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
            ViewBag.PatientId = patient.Id;

            return View(doctor);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelAppointment(int id)
        {
            var patient = GetCurrentPatient();
            if (patient == null) return RedirectToAction("Login", "Account");

            var appt = _context.Appointments.FirstOrDefault(a => a.Id == id && a.PatientId == patient.Id);
            if (appt != null)
            {
                appt.AppointmentStatus = "Cancelled";
                _context.SaveChanges();
                TempData["Success"] = "Appointment cancelled successfully.";
            }
            else
            {
                TempData["Error"] = "Appointment not found.";
            }
            return RedirectToAction("Dashboard", new { section = "appointments" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RescheduleAppointment(int id, DateTime newDate, string newTime)
        {
            var patient = GetCurrentPatient();
            if (patient == null) return RedirectToAction("Login", "Account");

            var appt = _context.Appointments.FirstOrDefault(a => a.Id == id && a.PatientId == patient.Id);
            if (appt != null)
            {
                appt.AppointmentDate = newDate;
                appt.AppointmentTime = newTime;
                appt.AppointmentStatus = "Pending";
                _context.SaveChanges();
                TempData["Success"] = "Appointment rescheduled successfully. Status: Pending Approval.";
            }
            else
            {
                TempData["Error"] = "Appointment not found.";
            }
            return RedirectToAction("Dashboard", new { section = "appointments" });
        }

        // =========================================================================
        // FAMILY MEMBERS MANAGEMENT
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddFamilyMember(string name, string relationship, string gender, int age, string? bloodGroup, string? emergencyContact)
        {
            var patient = GetCurrentPatient();
            if (patient == null) return RedirectToAction("Login", "Account");

            var fm = new FamilyMember
            {
                PatientId = patient.Id,
                Name = name,
                Relationship = relationship,
                Gender = gender,
                Age = age,
                BloodGroup = bloodGroup,
                EmergencyContact = emergencyContact
            };

            _context.FamilyMembers.Add(fm);
            _context.SaveChanges();

            TempData["Success"] = "Family member added successfully.";
            return RedirectToAction("Dashboard", new { section = "family" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteFamilyMember(int id)
        {
            var patient = GetCurrentPatient();
            if (patient == null) return RedirectToAction("Login", "Account");

            var fm = _context.FamilyMembers.FirstOrDefault(f => f.Id == id && f.PatientId == patient.Id);
            if (fm != null)
            {
                _context.FamilyMembers.Remove(fm);
                _context.SaveChanges();
                TempData["Success"] = "Family member removed.";
            }
            return RedirectToAction("Dashboard", new { section = "family" });
        }

        // =========================================================================
        // INSURANCE INFO MANAGEMENT
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveInsuranceInfo(string providerName, string policyNumber, string? coverageDetails, DateTime expiryDate)
        {
            var patient = GetCurrentPatient();
            if (patient == null) return RedirectToAction("Login", "Account");

            var existing = _context.InsuranceInfos.FirstOrDefault(i => i.PatientId == patient.Id);
            if (existing == null)
            {
                var ins = new InsuranceInfo
                {
                    PatientId = patient.Id,
                    ProviderName = providerName,
                    PolicyNumber = policyNumber,
                    CoverageDetails = coverageDetails,
                    ExpiryDate = expiryDate,
                    Status = "Active"
                };
                _context.InsuranceInfos.Add(ins);
            }
            else
            {
                existing.ProviderName = providerName;
                existing.PolicyNumber = policyNumber;
                existing.CoverageDetails = coverageDetails;
                existing.ExpiryDate = expiryDate;
                _context.InsuranceInfos.Update(existing);
            }
            _context.SaveChanges();

            TempData["Success"] = "Insurance details updated successfully.";
            return RedirectToAction("Dashboard", new { section = "insurance" });
        }

        // =========================================================================
        // MEDICAL DOCUMENTS UPLOAD
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadMedicalDocument(string documentName, string documentType, IFormFile file, string? notes)
        {
            var patient = GetCurrentPatient();
            if (patient == null) return RedirectToAction("Login", "Account");

            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a valid document file to upload.";
                return RedirectToAction("Dashboard", new { section = "documents" });
            }

            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "medical_documents");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var doc = new MedicalDocument
            {
                PatientId = patient.Id,
                DoctorId = _context.Doctors.Select(d => d.Id).FirstOrDefault(),
                DocumentName = string.IsNullOrWhiteSpace(documentName) ? file.FileName : documentName,
                DocumentType = documentType ?? "Other",
                FilePath = "/uploads/medical_documents/" + uniqueFileName,
                UploadDate = DateTime.Now,
                Notes = notes
            };

            _context.MedicalDocuments.Add(doc);
            _context.SaveChanges();

            TempData["Success"] = "Medical document uploaded successfully.";
            return RedirectToAction("Dashboard", new { section = "documents" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteMedicalDocument(int id)
        {
            var patient = GetCurrentPatient();
            if (patient == null) return RedirectToAction("Login", "Account");

            var doc = _context.MedicalDocuments.FirstOrDefault(d => d.Id == id && d.PatientId == patient.Id);
            if (doc != null)
            {
                _context.MedicalDocuments.Remove(doc);
                _context.SaveChanges();
                TempData["Success"] = "Document deleted successfully.";
            }
            return RedirectToAction("Dashboard", new { section = "documents" });
        }

        // =========================================================================
        // ONLINE PAYMENT
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessOnlinePayment(int appointmentId, decimal amount, string paymentMethod, string transactionId)
        {
            var patient = GetCurrentPatient();
            if (patient == null) return RedirectToAction("Login", "Account");

            var payment = new Payment
            {
                PatientId = patient.Id,
                AppointmentId = appointmentId,
                Amount = amount,
                PaymentMethod = paymentMethod ?? "Online Mobile Banking",
                TransactionId = string.IsNullOrWhiteSpace(transactionId) ? "TXN-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper() : transactionId,
                PaymentStatus = "Completed",
                PaymentDateTime = DateTime.Now
            };

            _context.Payments.Add(payment);
            _context.SaveChanges();

            TempData["Success"] = "Payment completed successfully!";
            return RedirectToAction("Dashboard", new { section = "payments" });
        }

        // =========================================================================
        // DOCTOR REVIEWS & RATINGS
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddDoctorReview(int doctorId, int rating, string? comment)
        {
            var patient = GetCurrentPatient();
            if (patient == null) return RedirectToAction("Login", "Account");

            var review = new Feedback
            {
                PatientId = patient.Id,
                DoctorId = doctorId,
                Rating = rating < 1 ? 5 : (rating > 5 ? 5 : rating),
                Comment = comment,
                Status = "Active",
                FeedbackDateTime = DateTime.Now
            };

            _context.Feedbacks.Add(review);
            _context.SaveChanges();

            TempData["Success"] = "Thank you! Your doctor review has been submitted.";
            return RedirectToAction("Dashboard", new { section = "reviews" });
        }

        // =========================================================================
        // IN-APP MESSAGING
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendChatMessage(int doctorUserId, string message)
        {
            var patient = GetCurrentPatient();
            if (patient == null) return RedirectToAction("Login", "Account");

            if (!string.IsNullOrWhiteSpace(message))
            {
                var chat = new ChatMessage
                {
                    SenderUserId = patient.UserId,
                    ReceiverUserId = doctorUserId,
                    Message = message,
                    SentAt = DateTime.Now,
                    IsRead = false
                };
                _context.ChatMessages.Add(chat);
                _context.SaveChanges();
            }

            return RedirectToAction("Dashboard", new { section = "messages", doctorUserId = doctorUserId });
        }

        // =========================================================================
        // PROFILE UPDATE
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(string username, string phoneNumber, string gender, DateTime? dateOfBirth, string? bloodGroup, string? address, string? emergencyContact, string? medicalHistory, string? allergies, string? chronicDiseases)
        {
            var patient = GetCurrentPatient();
            if (patient == null) return RedirectToAction("Login", "Account");

            if (patient.User != null)
            {
                patient.User.Username = username;
                patient.User.PhoneNumber = phoneNumber;
            }

            patient.Gender = gender ?? patient.Gender;
            if (dateOfBirth.HasValue) patient.DateOfBirth = dateOfBirth.Value;
            patient.BloodGroup = bloodGroup;
            patient.Address = address;
            patient.EmergencyContact = emergencyContact;
            patient.MedicalHistory = medicalHistory;
            patient.Allergies = allergies;
            patient.ChronicDiseases = chronicDiseases;

            _context.SaveChanges();

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Dashboard", new { section = "profile" });
        }
    }
}
