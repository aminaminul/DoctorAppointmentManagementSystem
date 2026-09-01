using Microsoft.EntityFrameworkCore;
using DoctorAppointmentManagementSystem.Models;
using DoctorAppointmentManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using DoctorAppointmentManagementSystem.Data;
using System.Linq;

namespace DoctorAppointmentManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
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
// AddDoctor Action
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
                RoleId = doctorRoleId
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            Doctor doctor = new Doctor()
            {
                UserId = user.Id,
                Specialization = model.Specialization,
                Availability = model.Availability,
                Qualification = "",
                Experience = 0,
                ConsultationFee = 0,
                AvailableDays = "Mon-Fri",
                AvailableTime = model.Availability
            };

            _context.Doctors.Add(doctor);
            _context.SaveChanges();

            return RedirectToAction("Doctors");
        }
// EditDoctor Action
        public IActionResult EditDoctor(int id)
        {
            var doctor = _context.Doctors
                .Include(d => d.User)
                .FirstOrDefault(d => d.Id == id);
            return View(doctor);
        }

        [HttpPost]
// EditDoctor Action
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
                    doctor.User.Email = model.User.Email;
                    doctor.User.PhoneNumber = model.User.PhoneNumber;
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Doctors");
        }
// DeleteDoctor Action
        public IActionResult DeleteDoctor(int id)
        {
            var doctor = _context.Doctors.Find(id);

            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
                _context.SaveChanges();
            }

            return RedirectToAction("Doctors");
        }
// AddPatient Action
        public IActionResult AddPatient()
        {
            return View();
        }

        [HttpPost]
// AddPatient Action
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
                RoleId = patientRoleId
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            Patient patient = new Patient()
            {
                UserId = user.Id,
                Age = model.Age,
                Gender = model.Gender,
                DateOfBirth = DateTime.Today.AddYears(-model.Age)
            };

            _context.Patients.Add(patient);
            _context.SaveChanges();

            return RedirectToAction("Patients");
        }
// EditPatient Action
        public IActionResult EditPatient(int id)
        {
            var patient = _context.Patients
                .Include(p => p.User)
                .FirstOrDefault(p => p.Id == id);
            return View(patient);
        }

        [HttpPost]
// EditPatient Action
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
                    patient.User.Email = model.User.Email;
                    patient.User.PhoneNumber = model.User.PhoneNumber;
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Patients");
        }
// DeletePatient Action
        public IActionResult DeletePatient(int id)
        {
            var patient = _context.Patients.Find(id);

            if (patient != null)
            {
                _context.Patients.Remove(patient);
                _context.SaveChanges();
            }

            return RedirectToAction("Patients");
        }
// PatientDetails Action
        public IActionResult PatientDetails(int id)
        {
            var patient = _context.Patients
                .Include(p => p.User)
                .FirstOrDefault(p => p.Id == id);

            return View(patient);
        }
// DoctorDetails Action
        public IActionResult DoctorDetails(int id)
        {
            var doctor = _context.Doctors
                .Include(d => d.User)
                .FirstOrDefault(d => d.Id == id);

            if (doctor == null)
            {
                return NotFound();
            }

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
    }
}


