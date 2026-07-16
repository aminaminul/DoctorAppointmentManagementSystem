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

        public IActionResult Register()
        {
            ViewBag.Roles = _context.Roles.Where(r => r.Name != "Admin").ToList();
            return View();
        }
        [HttpPost]
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
                if (string.IsNullOrWhiteSpace(model.Specialization))
                {
                    ModelState.AddModelError("Specialization", "Specialization is required for doctors.");
                    return View(model);
                }
            }
            else if (selRoleId == 3)
            {
                if (!model.Age.HasValue && !model.DateOfBirth.HasValue)
                {
                    ModelState.AddModelError("Age", "Please provide Age or Date of Birth for patients.");
                    return View(model);
                }
            }

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

            if (roleId == 3)
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
            else if (roleId == 2)
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
            var role = _context.Roles.Find(roleId);
            var roleName = role?.RoleName ?? role?.Name ?? "Patient";
            HttpContext.Session.SetString("UserRole", roleName);
            HttpContext.Session.SetString("UserName", user.Username ?? user.Email);

            if (roleId == 1)
                return RedirectToAction("Dashboard", "Admin");

            if (roleId == 2)
                return RedirectToAction("Dashboard", "Doctor");

            return RedirectToAction("Dashboard", "Patient");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password!";
                return View(model);
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserRole", user.Role?.RoleName ?? "Patient");
            HttpContext.Session.SetString("UserName", user.Username ?? user.Email);

            if (user.RoleId == 1)
                return RedirectToAction("Dashboard", "Admin");

            if (user.RoleId == 2)
                return RedirectToAction("Dashboard", "Doctor");

            return RedirectToAction("Dashboard", "Patient");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

    }
}