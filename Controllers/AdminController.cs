using Microsoft.EntityFrameworkCore;
using DoctorAppointmentManagementSystem.Models;
using DoctorAppointmentManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using DoctorAppointmentManagementSystem.Data;
using DoctorAppointmentManagementSystem.Services;
using DoctorAppointmentManagementSystem.Filters;
using System.Linq;

namespace DoctorAppointmentManagementSystem.Controllers
{
    [AuthorizeRole("Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public AdminController(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // Privacy Policy Management
// EditPrivacy Action
        public IActionResult EditPrivacy()
        {
            var policy = _context.PrivacyPolicies.OrderByDescending(p => p.UpdatedAt).FirstOrDefault();
            return View(policy ?? new DoctorAppointmentManagementSystem.Models.PrivacyPolicy());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Privacy Policy Management
// EditPrivacy Action
        public IActionResult EditPrivacy(DoctorAppointmentManagementSystem.Models.PrivacyPolicy model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existing = _context.PrivacyPolicies.OrderByDescending(p => p.UpdatedAt).FirstOrDefault();
            if (existing == null)
            {
                model.UpdatedAt = DateTime.Now;
                _context.PrivacyPolicies.Add(model);
            }
            else
            {
                existing.Content = model.Content;
                existing.UpdatedAt = DateTime.Now;
                _context.PrivacyPolicies.Update(existing);
            }
            _context.SaveChanges();
            TempData["Success"] = "Privacy policy updated.";
            return RedirectToAction("Dashboard");
        }

        // Admin Dashboard Action
        public IActionResult Dashboard(string section)
        {
            var userName = HttpContext.Session.GetString("UserName");
            ViewData["Username"] = !string.IsNullOrEmpty(userName) ? userName : "Admin";

            var roles = _context.Roles.ToList();
            ViewBag.Roles = roles;

            ViewBag.TotalDoctorsCount = _context.Doctors.Count();
            ViewBag.TotalPatientsCount = _context.Patients.Count();
            ViewBag.TotalAppointmentsCount = _context.Appointments.Count();
            ViewBag.TotalSchedulesCount = _context.DoctorSchedules.Count();

            // Total Collection & Revenue from Database
            var allPayments = _context.Payments.Where(p => p.PaymentStatus == "Paid" || p.PaymentStatus == "Completed").ToList();
            ViewBag.TotalCollection = allPayments.Sum(p => p.Amount);
            ViewBag.TodayCollection = allPayments.Where(p => p.PaymentDateTime.Date == DateTime.Today).Sum(p => p.Amount);
            ViewBag.MonthlyCollection = allPayments.Where(p => p.PaymentDateTime.Month == DateTime.Today.Month && p.PaymentDateTime.Year == DateTime.Today.Year).Sum(p => p.Amount);
            ViewBag.TotalInvoicesCount = _context.Invoices.Count();

            // 1. Weekly Appointment Trend (Past 7 Days)
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-6 + i))
                .ToList();
            var weeklyLabels = last7Days.Select(d => d.ToString("ddd (dd MMM)")).ToList();
            var sevenDaysAgo = DateTime.Today.AddDays(-6);
            var recentApptsList = _context.Appointments
                .Where(a => a.AppointmentDate.Date >= sevenDaysAgo && a.AppointmentDate.Date <= DateTime.Today)
                .ToList();
            var weeklyTotalCounts = last7Days
                .Select(d => recentApptsList.Count(a => a.AppointmentDate.Date == d.Date))
                .ToList();
            var weeklyCompletedCounts = last7Days
                .Select(d => recentApptsList.Count(a => a.AppointmentDate.Date == d.Date && a.AppointmentStatus == "Completed"))
                .ToList();

            ViewBag.WeeklyLabels = weeklyLabels;
            ViewBag.WeeklyTotalCounts = weeklyTotalCounts;
            ViewBag.WeeklyCompletedCounts = weeklyCompletedCounts;

            // 2. Department Revenue Breakdown
            var departmentRevenues = _context.Payments
                .Where(p => (p.PaymentStatus == "Paid" || p.PaymentStatus == "Completed") && p.Appointment != null && p.Appointment.Doctor != null)
                .Include(p => p.Appointment).ThenInclude(a => a.Doctor)
                .GroupBy(p => p.Appointment.Doctor.Specialization)
                .Select(g => new { Department = g.Key, Total = g.Sum(p => p.Amount) })
                .OrderByDescending(x => x.Total)
                .Take(6)
                .ToList();

            if (!departmentRevenues.Any())
            {
                departmentRevenues = _context.Doctors
                    .Where(d => d.ActiveStatus)
                    .GroupBy(d => d.Specialization)
                    .Select(g => new { Department = g.Key, Total = (decimal)g.Count() * 500m })
                    .Take(5)
                    .ToList();
            }
            ViewBag.DeptLabels = departmentRevenues.Select(d => string.IsNullOrEmpty(d.Department) ? "General" : d.Department).ToList();
            ViewBag.DeptAmounts = departmentRevenues.Select(d => d.Total).ToList();

            // 3. Appointment Status Distribution
            var statusCounts = _context.Appointments
                .GroupBy(a => a.AppointmentStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();
            ViewBag.StatusLabels = statusCounts.Select(s => string.IsNullOrEmpty(s.Status) ? "Pending" : s.Status).ToList();
            ViewBag.StatusCounts = statusCounts.Select(s => s.Count).ToList();

            ViewBag.RecentAppointments = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .OrderByDescending(a => a.BookingDateTime)
                .Take(5)
                .ToList();

            ViewBag.RecentSchedules = _context.DoctorSchedules
                .Include(ds => ds.Doctor).ThenInclude(d => d.User)
                .OrderByDescending(ds => ds.AvailableDate)
                .Take(6)
                .ToList();

            if (section == "doctors")
                ViewBag.Doctors = _context.Doctors
                     .Include(d => d.User)
                     .ToList();

            if (section == "patients")
                ViewBag.Patients = _context.Patients
                    .Include(p => p.User)
                    .ToList();

            if (section == "appointments")
                ViewBag.Appointments = _context.Appointments
                    .Include(a => a.Patient).ThenInclude(p => p.User)
                    .Include(a => a.Doctor).ThenInclude(d => d.User)
                    .ToList();

            if (section == "queue")
            {
                ViewBag.Doctors = _context.Doctors.Include(d => d.User).ToList();
                
                int selectedDocId = 0;
                string docQuery = Request.Query["doctorId"].ToString();
                if (int.TryParse(docQuery, out int dId))
                {
                    selectedDocId = dId;
                }
                else if (ViewBag.Doctors.Count > 0)
                {
                    selectedDocId = ViewBag.Doctors[0].Id;
                }

                ViewBag.SelectedDoctorId = selectedDocId;

                var today = DateTime.Today;
                if (selectedDocId > 0)
                {
                    QueueManager.EnsureQueueGenerated(_context, selectedDocId, today);
                    ViewBag.QueueEntries = _context.QueueEntries
                        .Include(q => q.Appointment)
                        .ThenInclude(a => a.Patient)
                        .ThenInclude(p => p.User)
                        .Where(q => q.Appointment.DoctorId == selectedDocId 
                                 && q.Appointment.AppointmentDate.Date == today)
                        .OrderBy(q => q.SequenceNumber)
                        .ToList();
                    ViewBag.DoctorStatus = QueueManager.GetDoctorStatus(_context, selectedDocId, today);
                }
            }

            ViewBag.Section = section;

            return View();
        }

        // Doctors List Action
// Doctors Action
        public IActionResult Doctors()
        {
            var doctors = _context.Doctors.Include(d => d.User).ToList();
            return View(doctors);
        }

        // Patients List Action
// Patients Action
        public IActionResult Patients()
        {
            var patients = _context.Patients.Include(p => p.User).ToList();
            return View(patients);
        }
// AddDoctor Action
        public IActionResult AddDoctor()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddDoctor(DoctorCreateViewModel model)
        {
            var exists = _context.Users.Any(u => u.Email == model.Email);

            if (exists)
            {
                ViewBag.Error = "Email already exists!";
                return View(model);
            }
            var doctorRole = _context.Roles.FirstOrDefault(r => r.Name == "Doctor");
            int doctorRoleId = doctorRole?.Id ?? 2;

            User user = new User()
            {
                Username = model.Name,
                Email = model.Email,
                Password = model.Password,
                PhoneNumber = model.PhoneNumber,
                RoleId = doctorRoleId,
                ActiveStatus = true
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            Doctor doctor = new Doctor()
            {
                UserId = user.Id,
                Specialization = model.Specialization,
                Availability = model.Availability,
                Qualification = model.Qualification ?? "",
                Experience = model.Experience,
                ConsultationFee = model.ConsultationFee,
                AvailableDays = model.AvailableDays ?? "Mon-Fri",
                AvailableTime = model.Availability,
                ActiveStatus = true
            };

            _context.Doctors.Add(doctor);

            _context.AdminLogs.Add(new AdminLog
            {
                AdminId = HttpContext.Session.GetInt32("UserId") ?? 1,
                ActionPerformed = "Doctor Created",
                Description = $"New doctor account registered: Dr. {user.Username} ({model.Specialization}), Consultation Fee: ৳{model.ConsultationFee}.",
                ActionDateTime = DateTime.Now
            });

            _context.SaveChanges();
            TempData["Success"] = $"Dr. {user.Username} successfully added.";

            return RedirectToAction("Doctors");
        }

        public IActionResult EditDoctor(int id)
        {
            var doctor = _context.Doctors
                .Include(d => d.User)
                .FirstOrDefault(d => d.Id == id);
            return View(doctor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditDoctor(Doctor model)
        {
            var doctor = _context.Doctors
                .Include(d => d.User)
                .FirstOrDefault(d => d.Id == model.Id);

            if (doctor != null)
            {
                doctor.Specialization = model.Specialization;
                doctor.Availability = model.Availability;
                doctor.AvailableTime = model.Availability;
                doctor.Qualification = model.Qualification;
                doctor.Experience = model.Experience;
                doctor.ConsultationFee = model.ConsultationFee;
                doctor.AvailableDays = model.AvailableDays;
                doctor.ActiveStatus = model.ActiveStatus;

                if (doctor.User != null && model.User != null)
                {
                    doctor.User.Username = model.User.Username;
                    doctor.User.PhoneNumber = model.User.PhoneNumber;
                    doctor.User.ActiveStatus = model.ActiveStatus;
                }

                _context.AdminLogs.Add(new AdminLog
                {
                    AdminId = HttpContext.Session.GetInt32("UserId") ?? 1,
                    ActionPerformed = "Doctor Updated",
                    Description = $"Profile updated for Dr. {doctor.User?.Username} ({doctor.Specialization}).",
                    ActionDateTime = DateTime.Now
                });

                _context.SaveChanges();
                TempData["Success"] = $"Doctor details updated.";
            }

            return RedirectToAction("Doctors");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleDoctorStatus(int id)
        {
            var doctor = _context.Doctors.Include(d => d.User).FirstOrDefault(d => d.Id == id);
            if (doctor != null)
            {
                doctor.ActiveStatus = !doctor.ActiveStatus;
                if (doctor.User != null) doctor.User.ActiveStatus = doctor.ActiveStatus;

                _context.AdminLogs.Add(new AdminLog
                {
                    AdminId = HttpContext.Session.GetInt32("UserId") ?? 1,
                    ActionPerformed = "Doctor Status Toggled",
                    Description = $"Admin changed status of Dr. {doctor.User?.Username} to {(doctor.ActiveStatus ? "Active" : "Inactive")}.",
                    ActionDateTime = DateTime.Now
                });

                _context.SaveChanges();
                TempData["Success"] = $"Dr. {doctor.User?.Username} status changed to {(doctor.ActiveStatus ? "Active" : "Inactive")}.";
            }
            return RedirectToAction("Doctors");
        }

        public IActionResult DeleteDoctor(int id)
        {
            var doctor = _context.Doctors.Include(d => d.User).FirstOrDefault(d => d.Id == id);

            if (doctor != null)
            {
                bool hasAppointments = _context.Appointments.Any(a => a.DoctorId == id);
                bool hasSchedules = _context.DoctorSchedules.Any(s => s.DoctorId == id);

                if (hasAppointments || hasSchedules)
                {
                    doctor.ActiveStatus = false;
                    if (doctor.User != null) doctor.User.ActiveStatus = false;

                    _context.AdminLogs.Add(new AdminLog
                    {
                        AdminId = HttpContext.Session.GetInt32("UserId") ?? 1,
                        ActionPerformed = "Doctor Deactivated",
                        Description = $"Doctor Dr. {doctor.User?.Username} has existing records ({_context.Appointments.Count(a => a.DoctorId == id)} appointments). Safely deactivated instead of deleted.",
                        ActionDateTime = DateTime.Now
                    });

                    _context.SaveChanges();
                    TempData["Success"] = $"Dr. {doctor.User?.Username} has active clinical/appointment history and was safely deactivated to preserve medical data.";
                }
                else
                {
                    var user = doctor.User;
                    _context.Doctors.Remove(doctor);
                    if (user != null) _context.Users.Remove(user);

                    _context.AdminLogs.Add(new AdminLog
                    {
                        AdminId = HttpContext.Session.GetInt32("UserId") ?? 1,
                        ActionPerformed = "Doctor Deleted",
                        Description = $"Doctor record deleted: Dr. {user?.Username}.",
                        ActionDateTime = DateTime.Now
                    });

                    _context.SaveChanges();
                    TempData["Success"] = "Doctor record deleted successfully.";
                }
            }

            return RedirectToAction("Doctors");
        }

        public IActionResult AddPatient()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddPatient(PatientCreateViewModel model)
        {
            var exists = _context.Users.Any(u => u.Email == model.Email);

            if (exists)
            {
                ViewBag.Error = "Email already exists!";
                return View(model);
            }

            var patientRole = _context.Roles.FirstOrDefault(r => r.Name == "Patient");
            int patientRoleId = patientRole?.Id ?? 3;

            User user = new User()
            {
                Username = model.Name,
                Email = model.Email,
                Password = model.Password,
                PhoneNumber = model.PhoneNumber,
                RoleId = patientRoleId,
                ActiveStatus = true
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            Patient patient = new Patient()
            {
                UserId = user.Id,
                Age = model.Age,
                Gender = model.Gender,
                BloodGroup = model.BloodGroup,
                Address = model.Address,
                DateOfBirth = DateTime.Today.AddYears(-model.Age),
                ActiveStatus = true
            };

            _context.Patients.Add(patient);

            _context.AdminLogs.Add(new AdminLog
            {
                AdminId = HttpContext.Session.GetInt32("UserId") ?? 1,
                ActionPerformed = "Patient Created",
                Description = $"New patient registered: {user.Username} ({model.Gender}, {model.Age} yrs).",
                ActionDateTime = DateTime.Now
            });

            _context.SaveChanges();
            TempData["Success"] = $"Patient {user.Username} successfully registered.";

            return RedirectToAction("Patients");
        }

        public IActionResult EditPatient(int id)
        {
            var patient = _context.Patients
                .Include(p => p.User)
                .FirstOrDefault(p => p.Id == id);
            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPatient(Patient model)
        {
            var patient = _context.Patients
                .Include(p => p.User)
                .FirstOrDefault(p => p.Id == model.Id);

            if (patient != null)
            {
                patient.DateOfBirth = model.DateOfBirth;
                patient.Gender = model.Gender;
                patient.BloodGroup = model.BloodGroup;
                patient.Address = model.Address;
                patient.EmergencyContact = model.EmergencyContact;
                patient.MedicalHistory = model.MedicalHistory;
                patient.Allergies = model.Allergies;
                patient.ActiveStatus = model.ActiveStatus;

                if (patient.User != null && model.User != null)
                {
                    patient.User.Username = model.User.Username;
                    patient.User.PhoneNumber = model.User.PhoneNumber;
                    patient.User.ActiveStatus = model.ActiveStatus;
                }

                _context.AdminLogs.Add(new AdminLog
                {
                    AdminId = HttpContext.Session.GetInt32("UserId") ?? 1,
                    ActionPerformed = "Patient Updated",
                    Description = $"Profile updated for patient {patient.User?.Username}.",
                    ActionDateTime = DateTime.Now
                });

                _context.SaveChanges();
                TempData["Success"] = "Patient details updated.";
            }

            return RedirectToAction("Patients");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TogglePatientStatus(int id)
        {
            var patient = _context.Patients.Include(p => p.User).FirstOrDefault(p => p.Id == id);
            if (patient != null)
            {
                patient.ActiveStatus = !patient.ActiveStatus;
                if (patient.User != null) patient.User.ActiveStatus = patient.ActiveStatus;

                _context.AdminLogs.Add(new AdminLog
                {
                    AdminId = HttpContext.Session.GetInt32("UserId") ?? 1,
                    ActionPerformed = "Patient Status Toggled",
                    Description = $"Admin changed status of patient {patient.User?.Username} to {(patient.ActiveStatus ? "Active" : "Inactive")}.",
                    ActionDateTime = DateTime.Now
                });

                _context.SaveChanges();
                TempData["Success"] = $"Patient {patient.User?.Username} status changed to {(patient.ActiveStatus ? "Active" : "Inactive")}.";
            }
            return RedirectToAction("Patients");
        }

        public IActionResult DeletePatient(int id)
        {
            var patient = _context.Patients.Include(p => p.User).FirstOrDefault(p => p.Id == id);

            if (patient != null)
            {
                bool hasAppointments = _context.Appointments.Any(a => a.PatientId == id);
                bool hasInvoices = _context.Invoices.Any(i => i.PatientId == id);

                if (hasAppointments || hasInvoices)
                {
                    patient.ActiveStatus = false;
                    if (patient.User != null) patient.User.ActiveStatus = false;

                    _context.AdminLogs.Add(new AdminLog
                    {
                        AdminId = HttpContext.Session.GetInt32("UserId") ?? 1,
                        ActionPerformed = "Patient Deactivated",
                        Description = $"Patient {patient.User?.Username} has existing records ({_context.Appointments.Count(a => a.PatientId == id)} appointments). Safely deactivated instead of deleted.",
                        ActionDateTime = DateTime.Now
                    });

                    _context.SaveChanges();
                    TempData["Success"] = $"Patient {patient.User?.Username} has active appointment/billing history and was safely deactivated to preserve records.";
                }
                else
                {
                    var user = patient.User;
                    _context.Patients.Remove(patient);
                    if (user != null) _context.Users.Remove(user);

                    _context.AdminLogs.Add(new AdminLog
                    {
                        AdminId = HttpContext.Session.GetInt32("UserId") ?? 1,
                        ActionPerformed = "Patient Deleted",
                        Description = $"Patient record deleted: {user?.Username}.",
                        ActionDateTime = DateTime.Now
                    });

                    _context.SaveChanges();
                    TempData["Success"] = "Patient record deleted successfully.";
                }
            }

            return RedirectToAction("Patients");
        }

        public IActionResult PatientDetails(int id)
        {
            var patient = _context.Patients
                .Include(p => p.User)
                .FirstOrDefault(p => p.Id == id);

            if (patient == null) return NotFound();

            ViewBag.Appointments = _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Where(a => a.PatientId == id)
                .OrderByDescending(a => a.AppointmentDate)
                .Take(10)
                .ToList();

            ViewBag.Invoices = _context.Invoices
                .Where(i => i.PatientId == id)
                .OrderByDescending(i => i.IssueDate)
                .Take(10)
                .ToList();

            return View(patient);
        }

        public IActionResult DoctorDetails(int id)
        {
            var doctor = _context.Doctors
                .Include(d => d.User)
                .FirstOrDefault(d => d.Id == id);

            if (doctor == null)
            {
                return NotFound();
            }

            ViewBag.Schedules = _context.DoctorSchedules
                .Where(s => s.DoctorId == id)
                .OrderByDescending(s => s.AvailableDate)
                .Take(10)
                .ToList();

            ViewBag.Appointments = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Where(a => a.DoctorId == id)
                .OrderByDescending(a => a.AppointmentDate)
                .Take(10)
                .ToList();

            return View(doctor);
        }

        // Leave Requests List Action
// LeaveRequests Action
        public IActionResult LeaveRequests()
        {
            var requests = _context.LeaveRequests.Include(lr => lr.User).ToList();
            return View(requests);
        }

        [HttpPost]
// UpdateLeaveStatus Action
        public IActionResult UpdateLeaveStatus(int id, string status)
        {
            var request = _context.LeaveRequests.Find(id);
            if (request != null)
            {
                request.Status = status;
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(LeaveRequests));
        }
// DeleteLeaveRequest Action
        public IActionResult DeleteLeaveRequest(int id)
        {
            var request = _context.LeaveRequests.Find(id);
            if (request != null)
            {
                _context.LeaveRequests.Remove(request);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(LeaveRequests));
        }

        // Complaints List Action
// Complaints Action
        public IActionResult Complaints()
        {
            var complaints = _context.Complaints.Include(c => c.User).ToList();
            return View(complaints);
        }

        [HttpPost]
// UpdateComplaintStatus Action
        public IActionResult UpdateComplaintStatus(int id, string status)
        {
            var complaint = _context.Complaints.Find(id);
            if (complaint != null)
            {
                complaint.Status = status;
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Complaints));
        }
// DeleteComplaint Action
        public IActionResult DeleteComplaint(int id)
        {
            var complaint = _context.Complaints.Find(id);
            if (complaint != null)
            {
                _context.Complaints.Remove(complaint);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Complaints));
        }

        // Invoices List Action
// Invoices Action
        public IActionResult Invoices()
        {
            var invoices = _context.Invoices.Include(i => i.Patient).ThenInclude(p => p.User).ToList();
            return View(invoices);
        }
// CreateInvoice Action
        public IActionResult CreateInvoice()
        {
            ViewBag.Patients = _context.Patients.Include(p => p.User).ToList();
            return View();
        }

        [HttpPost]
// CreateInvoice Action
        public IActionResult CreateInvoice(DoctorAppointmentManagementSystem.Models.Invoice invoice)
        {
            invoice.IssueDate = DateTime.Now;
            _context.Invoices.Add(invoice);
            _context.SaveChanges();
            return RedirectToAction(nameof(Invoices));
        }
// EditInvoice Action
        public IActionResult EditInvoice(int id)
        {
            var invoice = _context.Invoices.Find(id);
            if (invoice == null) return NotFound();
            ViewBag.Patients = _context.Patients.Include(p => p.User).ToList();
            return View(invoice);
        }

        [HttpPost]
// EditInvoice Action
        public IActionResult EditInvoice(DoctorAppointmentManagementSystem.Models.Invoice invoice)
        {
            var existing = _context.Invoices.Find(invoice.Id);
            if (existing != null)
            {
                existing.PatientId = invoice.PatientId;
                existing.TotalAmount = invoice.TotalAmount;
                existing.Particulars = invoice.Particulars;
                existing.Status = invoice.Status;
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Invoices));
        }
// DeleteInvoice Action
        public IActionResult DeleteInvoice(int id)
        {
            var invoice = _context.Invoices.Find(id);
            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Invoices));
        }

        // Appointments Management
        public IActionResult Appointments(string? status, int? doctorId, DateTime? date, string? search)
        {
            var query = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.AppointmentStatus == status);
            }
            if (doctorId.HasValue && doctorId.Value > 0)
            {
                query = query.Where(a => a.DoctorId == doctorId.Value);
            }
            if (date.HasValue)
            {
                query = query.Where(a => a.AppointmentDate.Date == date.Value.Date);
            }
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(a =>
                    (a.Patient != null && a.Patient.User != null && a.Patient.User.Username.ToLower().Contains(search)) ||
                    (a.Doctor != null && a.Doctor.User != null && a.Doctor.User.Username.ToLower().Contains(search)) ||
                    a.Id.ToString() == search);
            }

            var appts = query.OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.BookingDateTime).ToList();
            var apptIds = appts.Select(a => a.Id).ToList();

            ViewBag.PaymentsMap = _context.Payments
                .Where(p => apptIds.Contains(p.AppointmentId))
                .ToDictionary(p => p.AppointmentId, p => p);

            ViewBag.PrescriptionsMap = _context.Prescriptions
                .Where(pr => apptIds.Contains(pr.AppointmentId))
                .ToDictionary(pr => pr.AppointmentId, pr => pr);

            ViewBag.Doctors = _context.Doctors.Include(d => d.User).Where(d => d.ActiveStatus).ToList();
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedDoctorId = doctorId;
            ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd");
            ViewBag.SearchTerm = search;

            return View(appts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateAppointmentStatus(int id, string status)
        {
            var appt = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .FirstOrDefault(a => a.Id == id);

            if (appt != null)
            {
                var oldStatus = appt.AppointmentStatus;
                appt.AppointmentStatus = status;

                if (status == "Completed" && appt.Doctor != null && appt.Patient != null)
                {
                    var existingInvoice = _context.Invoices.FirstOrDefault(i => i.AppointmentId == appt.Id);
                    if (existingInvoice == null)
                    {
                        var invoice = new Invoice
                        {
                            AppointmentId = appt.Id,
                            PatientId = appt.PatientId,
                            TotalAmount = appt.Doctor.ConsultationFee,
                            IssueDate = DateTime.Now,
                            Status = "Paid",
                            Particulars = $"Consultation with Dr. {appt.Doctor.User?.Username} ({appt.Doctor.Specialization}) on {appt.AppointmentDate:dd MMM yyyy}"
                        };
                        _context.Invoices.Add(invoice);
                    }
                }

                int adminId = HttpContext.Session.GetInt32("UserId") ?? 1;
                _context.AdminLogs.Add(new AdminLog
                {
                    AdminId = adminId,
                    ActionPerformed = "Appointment Status Updated",
                    Description = $"Admin updated Appointment #{appt.Id} status from '{oldStatus}' to '{status}' (Patient: {appt.Patient?.User?.Username}, Doctor: {appt.Doctor?.User?.Username}).",
                    ActionDateTime = DateTime.Now
                });

                _context.SaveChanges();
                TempData["Success"] = $"Appointment #{appt.Id} status updated to '{status}'.";
            }
            return RedirectToAction(nameof(Appointments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RescheduleAppointment(int id, DateTime newDate, string newTime)
        {
            var appt = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .FirstOrDefault(a => a.Id == id);

            if (appt != null)
            {
                appt.AppointmentDate = newDate;
                appt.AppointmentTime = newTime;
                appt.AppointmentStatus = "Confirmed";

                int adminId = HttpContext.Session.GetInt32("UserId") ?? 1;
                _context.AdminLogs.Add(new AdminLog
                {
                    AdminId = adminId,
                    ActionPerformed = "Appointment Rescheduled",
                    Description = $"Admin rescheduled Appointment #{appt.Id} for {appt.Patient?.User?.Username} with Dr. {appt.Doctor?.User?.Username} to {newDate:dd MMM yyyy} at {newTime}.",
                    ActionDateTime = DateTime.Now
                });

                if (appt.Patient?.UserId != null)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = appt.Patient.UserId,
                        NotificationType = "Appointment",
                        Title = "Appointment Rescheduled",
                        Message = $"Your appointment with Dr. {appt.Doctor?.User?.Username} has been rescheduled by Clinic Admin to {newDate:dd MMM yyyy} at {newTime}.",
                        SentDateTime = DateTime.Now,
                        NotificationStatus = "Unread"
                    });
                }

                _context.SaveChanges();
                TempData["Success"] = $"Appointment #{appt.Id} successfully rescheduled to {newDate:dd MMM yyyy} at {newTime}.";
            }
            return RedirectToAction(nameof(Appointments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id, string reason)
        {
            var appt = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .FirstOrDefault(a => a.Id == id);

            if (appt == null)
            {
                TempData["Error"] = "Appointment not found.";
                return RedirectToAction(nameof(Appointments));
            }

            if (appt.AppointmentStatus == "Cancelled")
            {
                TempData["Error"] = "Appointment is already cancelled.";
                return RedirectToAction(nameof(Appointments));
            }

            string cancellationReason = !string.IsNullOrWhiteSpace(reason) ? reason : "Administrative cancellation";
            appt.AppointmentStatus = "Cancelled";

            var queueEntry = _context.QueueEntries.FirstOrDefault(q => q.AppointmentId == appt.Id);
            if (queueEntry != null)
            {
                queueEntry.Status = "Cancelled";
            }

            int adminId = HttpContext.Session.GetInt32("UserId") ?? 1;
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

                _context.AdminLogs.Add(new AdminLog
                {
                    AdminId = adminId,
                    ActionPerformed = "Appointment Cancelled & 100% Refunded",
                    Description = $"Admin cancelled paid Appointment #{appt.Id}. 100% refund of ৳{payment.Amount:N2} ({payment.PaymentMethod}) issued to patient {appt.Patient?.User?.Username}. Reason: {cancellationReason}",
                    ActionDateTime = DateTime.Now
                });

                TempData["Success"] = $"Appointment #{appt.Id} cancelled. Patient was notified and 100% refund of ৳{payment.Amount:N2} was initiated.";
            }
            else
            {
                _context.AdminLogs.Add(new AdminLog
                {
                    AdminId = adminId,
                    ActionPerformed = "Appointment Cancelled",
                    Description = $"Admin cancelled Appointment #{appt.Id} (Patient: {appt.Patient?.User?.Username}, Doctor: {appt.Doctor?.User?.Username}). Reason: {cancellationReason}",
                    ActionDateTime = DateTime.Now
                });

                if (appt.Patient?.UserId != null)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = appt.Patient.UserId,
                        NotificationType = "Appointment",
                        Title = "Appointment Cancelled",
                        Message = $"Your appointment #{appt.Id} with Dr. {appt.Doctor?.User?.Username} was cancelled by Admin. Reason: {cancellationReason}",
                        SentDateTime = DateTime.Now,
                        NotificationStatus = "Unread"
                    });
                }

                TempData["Success"] = $"Appointment #{appt.Id} cancelled successfully.";
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Appointments));
        }

        // Financial Ledger & Payments Management
        public IActionResult Payments(string? status, string? method, string? search)
        {
            var query = _context.Payments
                .Include(p => p.Appointment).ThenInclude(a => a.Patient).ThenInclude(pt => pt.User)
                .Include(p => p.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.PaymentStatus == status);
            }
            if (!string.IsNullOrEmpty(method))
            {
                query = query.Where(p => p.PaymentMethod == method);
            }
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(p =>
                    (p.TransactionId != null && p.TransactionId.ToLower().Contains(search)) ||
                    (p.Appointment != null && p.Appointment.Patient != null && p.Appointment.Patient.User != null && p.Appointment.Patient.User.Username.ToLower().Contains(search)) ||
                    (p.Appointment != null && p.Appointment.Doctor != null && p.Appointment.Doctor.User != null && p.Appointment.Doctor.User.Username.ToLower().Contains(search)) ||
                    p.AppointmentId.ToString() == search);
            }

            var payments = query.OrderByDescending(p => p.PaymentDateTime).ToList();

            var allPaid = _context.Payments.Where(p => p.PaymentStatus == "Paid" || p.PaymentStatus == "Completed").ToList();
            var allRefunded = _context.Payments.Where(p => p.PaymentStatus == "Refunded").ToList();

            ViewBag.TotalPaidAmount = allPaid.Sum(p => p.Amount);
            ViewBag.TotalRefundedAmount = allRefunded.Sum(p => p.Amount);
            ViewBag.TodayPaidAmount = allPaid.Where(p => p.PaymentDateTime.Date == DateTime.Today).Sum(p => p.Amount);
            ViewBag.TotalTransactionsCount = _context.Payments.Count();

            ViewBag.SelectedStatus = status;
            ViewBag.SelectedMethod = method;
            ViewBag.SearchTerm = search;

            return View(payments);
        }

        // Audit & Security Logs
        public IActionResult Logs(string? search, string? actionType)
        {
            var query = _context.AdminLogs
                .Include(l => l.Admin)
                .AsQueryable();

            if (!string.IsNullOrEmpty(actionType))
            {
                query = query.Where(l => l.ActionPerformed == actionType);
            }
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(l =>
                    (l.Description != null && l.Description.ToLower().Contains(search)) ||
                    l.ActionPerformed.ToLower().Contains(search) ||
                    (l.Admin != null && l.Admin.Username.ToLower().Contains(search)));
            }

            var logs = query.OrderByDescending(l => l.ActionDateTime).Take(200).ToList();

            ViewBag.ActionTypes = _context.AdminLogs.Select(l => l.ActionPerformed).Distinct().ToList();
            ViewBag.SelectedActionType = actionType;
            ViewBag.SearchTerm = search;

            return View(logs);
        }

        // Live Chamber Queue Monitoring
        public IActionResult Queue(int? doctorId)
        {
            var doctors = _context.Doctors.Include(d => d.User).Where(d => d.ActiveStatus).ToList();
            ViewBag.Doctors = doctors;

            int selectedDocId = doctorId ?? (doctors.Any() ? doctors[0].Id : 0);
            ViewBag.SelectedDoctorId = selectedDocId;

            var today = DateTime.Today;
            if (selectedDocId > 0)
            {
                QueueManager.EnsureQueueGenerated(_context, selectedDocId, today);
                ViewBag.QueueEntries = _context.QueueEntries
                    .Include(q => q.Appointment)
                    .ThenInclude(a => a.Patient)
                    .ThenInclude(p => p.User)
                    .Where(q => q.Appointment.DoctorId == selectedDocId
                             && q.Appointment.AppointmentDate.Date == today)
                    .OrderBy(q => q.SequenceNumber)
                    .ToList();

                ViewBag.DoctorStatus = QueueManager.GetDoctorStatus(_context, selectedDocId, today);
                ViewBag.SelectedDoctor = doctors.FirstOrDefault(d => d.Id == selectedDocId);
            }

            return View();
        }
    }
}


