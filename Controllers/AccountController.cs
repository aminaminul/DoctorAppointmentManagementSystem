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
// Register Action
        public IActionResult Register()
        {
            ViewBag.Roles = _context.Roles.Where(r => r.Name != "Admin").ToList();
            return View();
        }
        [HttpPost]
// Register Action
        public IActionResult Register(RegisterViewModel model)
        {
            ViewBag.Roles = _context.Roles.Where(r => r.Name != "Admin").ToList();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!model.Email.EndsWith("@gmail.com"))
            {
                ModelState.AddModelError("Email", "Only Gmail allowed (@gmail.com)");
                return View(model);
            }

            var exists = _context.Users.Any(u => u.Email == model.Email);

            if (exists)
            {
                ModelState.AddModelError("Email", "Email already exists!");
                return View(model);
            }

            var selectedRole = _context.Roles.FirstOrDefault(r => r.Id == model.RoleId);
            string selectedRoleName = selectedRole?.Name ?? "Patient";

            int virtualRoleId = selectedRoleName switch
            {
                "Admin" => 1,
                "Doctor" => 2,
                "Patient" => 3,
                _ => 3
            };

            if (virtualRoleId == 2)
            {
                if (string.IsNullOrWhiteSpace(model.Specialization))
                {
                    ModelState.AddModelError("Specialization", "Specialization is required for doctors.");
                    return View(model);
                }
            }
            else if (virtualRoleId == 3)
            {
                if (!model.Age.HasValue && !model.DateOfBirth.HasValue)
                {
                    ModelState.AddModelError("Age", "Please provide Age or Date of Birth for patients.");
                    return View(model);
                }
            }

            User user = new User()
            {
                Username = model.Username,
                Email = model.Email,
                Password = model.Password,
                PhoneNumber = model.PhoneNumber,
                RoleId = model.RoleId > 0 ? model.RoleId : (selectedRole?.Id ?? 3)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            if (virtualRoleId == 3)
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
            else if (virtualRoleId == 2)
            {
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

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetInt32("RoleId", virtualRoleId);
            HttpContext.Session.SetString("UserRole", selectedRoleName);
            HttpContext.Session.SetString("UserName", user.Username ?? user.Email);

            if (virtualRoleId == 1)
                return RedirectToAction("Dashboard", "Admin");

            if (virtualRoleId == 2)
                return RedirectToAction("Dashboard", "Doctor");

            return RedirectToAction("Dashboard", "Patient");
        }
// Login Action
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
// Login Action
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = (model.Email ?? "").Trim().ToLower();
            var password = (model.Password ?? "").Trim();

            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Email.ToLower() == email && u.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password!";
                return View(model);
            }

            var role = user.Role ?? _context.Roles.FirstOrDefault(r => r.Id == user.RoleId);
            string roleName = role?.Name ?? "Patient";
            
            int virtualRoleId = roleName.ToLower() switch
            {
                "admin" => 1,
                "doctor" => 2,
                "patient" => 3,
                _ => 3
            };

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetInt32("RoleId", virtualRoleId);
            HttpContext.Session.SetString("UserRole", roleName);
            HttpContext.Session.SetString("UserName", user.Username ?? user.Email);

            if (model.RememberMe)
            {
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(30),
                    HttpOnly = true,
                    IsEssential = true
                };
                Response.Cookies.Append("dams_remember_user", user.Id.ToString(), cookieOptions);
            }
            else
            {
                Response.Cookies.Delete("dams_remember_user");
            }

            if (virtualRoleId == 1 || roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            if (virtualRoleId == 2 || roleName.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Dashboard", "Doctor");
            }

            return RedirectToAction("Dashboard", "Patient");
        }

        // Logout Action
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("dams_remember_user");
            return RedirectToAction("Login");
        }

    }
}

