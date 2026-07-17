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

        public IActionResult EditPrivacy()
        {
            var policy = _context.PrivacyPolicies.OrderByDescending(p => p.UpdatedAt).FirstOrDefault();
            return View(policy ?? new DoctorAppointmentManagementSystem.Models.PrivacyPolicy());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        public IActionResult Dashboard(string section)
        {
            var roles = _context.Roles.ToList();
            ViewBag.Roles = roles;

            ViewBag.TotalDoctorsCount = _context.Doctors.Count();
            ViewBag.TotalPatientsCount = _context.Patients.Count();
            ViewBag.TotalAppointmentsCount = _context.Appointments.Count();
            ViewBag.TotalSchedulesCount = _context.DoctorSchedules.Count();

            // Load recent 5 appointments for the overview panel
            ViewBag.RecentAppointments = _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .OrderByDescending(a => a.BookingDateTime)
                .Take(5)
                .ToList();

            // Load some active doctor schedules for the bottom list/grid
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

        public IActionResult AddDoctor()
        {
            return View();
        }

        [HttpPost]
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
                Availability = model.Availability
            };

            _context.Doctors.Add(doctor);
            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        public IActionResult EditDoctor(int id)
        {
            var doctor = _context.Doctors.Find(id);
            return View(doctor);
        }

        [HttpPost]
        public IActionResult EditDoctor(Doctor model)
        {
            _context.Doctors.Update(model);
            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        public IActionResult DeleteDoctor(int id)
        {
            var doctor = _context.Doctors.Find(id);

            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
                _context.SaveChanges();
            }

            return RedirectToAction("Dashboard");
        }

        public IActionResult AddPatient()
        {
            return View();
        }

        [HttpPost]
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
                Gender = model.Gender
            };

            _context.Patients.Add(patient);
            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }


        public IActionResult EditPatient(int id)
        {
            var patient = _context.Patients
                .Include(p => p.User)
                .FirstOrDefault(p => p.Id == id);
            return View(patient);
        }

        [HttpPost]
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

                if (patient.User != null && model.User != null)
                {
                    patient.User.Username = model.User.Username;
                    patient.User.Email = model.User.Email;
                    patient.User.PhoneNumber = model.User.PhoneNumber;
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Dashboard", new { section = "patients" });
        }


        public IActionResult DeletePatient(int id)
        {
            var patient = _context.Patients.Find(id);

            if (patient != null)
            {
                _context.Patients.Remove(patient);
                _context.SaveChanges();
            }

            return RedirectToAction("Dashboard");
        }

        public IActionResult PatientDetails(int id)
        {
            var patient = _context.Patients
                .Include(p => p.User)
                .FirstOrDefault(p => p.Id == id);

            return View(patient);
        }
    }
}
