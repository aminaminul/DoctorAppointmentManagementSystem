using DoctorAppointmentManagementSystem.Models;
using DoctorAppointmentManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoctorAppointmentManagementSystem.Data;

namespace DoctorAppointmentManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= REGISTER =================
        public IActionResult Register()
        {
            // Provide roles excluding Admin for registration dropdown
            // Use the mapped property 'Name' (RoleName is [NotMapped] and cannot be translated to SQL)
            ViewBag.Roles = _context.Roles.Where(r => r.Name != "Admin").ToList();
            return View();
        }
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            // Ensure roles are available when re-displaying the view after validation errors
            // Use the mapped property 'Name' for server-side filtering
            ViewBag.Roles = _context.Roles.Where(r => r.Name != "Admin").ToList();

            // 🔥 STEP 1: Model validation
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 🔥 STEP 2: Gmail validation
            if (!model.Email.EndsWith("@gmail.com"))
            {
                ModelState.AddModelError("Email", "Only Gmail allowed (@gmail.com)");
                return View(model);
            }

            // 🔥 STEP 3: Email already exists check
            var exists = _context.Users.Any(u => u.Email == model.Email);

            if (exists)
            {
                ModelState.AddModelError("Email", "Email already exists!");
                return View(model);
            }

            // 🔥 STEP 3.5: Determine intended role and perform role-specific validation
            // Determine intended role id (use numeric RoleId if provided, otherwise infer from RoleName, default to Patient = 3)
            int selRoleId = model.RoleId > 0 ? model.RoleId : 3;
            if (!string.IsNullOrEmpty(model.RoleName))
            {
                selRoleId = model.RoleName switch
                {
                    "Admin" => 1,
                    "Doctor" => 2,
                    "Patient" => 3,
                    _ => selRoleId
                };
            }

            if (selRoleId == 2)
            {
                // Doctor must provide specialization
                if (string.IsNullOrWhiteSpace(model.Specialization))
                {
                    ModelState.AddModelError("Specialization", "Specialization is required for doctors.");
                    return View(model);
                }
            }
            else if (selRoleId == 3)
            {
                // Patient must provide age or date of birth
                if (!model.Age.HasValue && !model.DateOfBirth.HasValue)
                {
                    ModelState.AddModelError("Age", "Please provide Age or Date of Birth for patients.");
                    return View(model);
                }
            }

            // 🔥 STEP 4: Save User
        // Determine final role id (validated above in selRoleId)
        int roleId = selRoleId;

            User user = new User()
            {
                Username = model.Username,
                Email = model.Email,
                Password = model.Password,
                PhoneNumber = model.PhoneNumber,
                RoleId = roleId
            };

            _context.Users.Add(user);
            _context.SaveChanges();

        // 🔥 STEP 5: Role-wise create using provided fields
        if (roleId == 3) // Patient
        {
            Patient patient = new Patient()
            {
                UserId = user.Id,
                Gender = model.Gender
            };
            if (model.DateOfBirth.HasValue)
            {
                patient.DateOfBirth = model.DateOfBirth.Value;
            }
            else if (model.Age.HasValue)
            {
                patient.Age = model.Age.Value;
            }
            else
            {
                patient.DateOfBirth = DateTime.Today;
            }

            _context.Patients.Add(patient);
        }
            else if (roleId == 2) // Doctor
            {
                // Combine Department enum with specialization for storage, keep backward compatible string column
                var deptPart = model.Department.HasValue ? model.Department.Value.ToString() + " - " : "";
                Doctor doctor = new Doctor()
                {
                    UserId = user.Id,
                    Specialization = deptPart + (model.Specialization ?? ""),
                    Availability = model.Availability ?? ""
                };

                _context.Doctors.Add(doctor);
            }

            _context.SaveChanges();

            // Set session and redirect based on role
            HttpContext.Session.SetInt32("UserId", user.Id);
            var role = _context.Roles.Find(roleId);
            var roleName = role?.RoleName ?? role?.Name ?? "Patient";
            HttpContext.Session.SetString("UserRole", roleName);
            HttpContext.Session.SetString("UserName", user.Username ?? user.Email);

            if (roleId == 1) // Admin
                return RedirectToAction("Dashboard", "Admin");

            if (roleId == 2) // Doctor
                return RedirectToAction("Dashboard", "Doctor");

            // Default (Patient) -> Patient Dashboard
            return RedirectToAction("Dashboard", "Patient");
        }

        // ================= LOGIN =================

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            // 🔥 STEP 1: Model validation
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 🔥 STEP 2: Check user from DB using Email
            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password!";
                return View(model);
            }

            // 🔥 STEP 3: Session set
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserRole", user.Role?.RoleName ?? "Patient");
            // Store display name for navbar
            HttpContext.Session.SetString("UserName", user.Username ?? user.Email);

            // Role-based redirect after login
            if (user.RoleId == 1) // Admin
                return RedirectToAction("Dashboard", "Admin");

            if (user.RoleId == 2) // Doctor
                return RedirectToAction("Dashboard", "Doctor");

            // Patient -> Patient Dashboard
            return RedirectToAction("Dashboard", "Patient");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

    }
}