using DoctorAppointmentManagementSystem.Models;
using DoctorAppointmentManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            return View();
        }
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
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

            // 🔥 STEP 4: Save User
            int roleId = model.Role switch
            {
                "Admin" => 1,
                "Doctor" => 2,
                "Patient" => 3,
                _ => 3
            };

            User user = new User()
            {
                Name = model.Name,
                Email = model.Email,
                Password = model.Password,
                RoleId = roleId
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // 🔥 STEP 5: Role wise create
            if (roleId == 3) // Patient
            {
                Patient patient = new Patient()
                {
                    UserId = user.Id,
                    Age = 20,
                    Gender = model.Gender
                };

                _context.Patients.Add(patient);
            }
            else if (roleId == 2) // Doctor
            {
                Doctor doctor = new Doctor()
                {
                    UserId = user.Id,
                    Specialization = "General",
                    Availability = "10AM-5PM"
                };

                _context.Doctors.Add(doctor);
            }

            _context.SaveChanges();

            // 🔥 STEP 6: Redirect
            return RedirectToAction("Login");
        }

        // ================= LOGIN =================

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            // 🔥 STEP 1: Model validation (Email + Gmail check)
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 🔥 STEP 2: Check user from DB
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

            // 🔥 STEP 4: Role-based redirect (IMPORTANT)
            if (user.RoleId == 3) // Patient
                return RedirectToAction("Dashboard", "Patient");

            if (user.RoleId == 2) // Doctor
                return RedirectToAction("Dashboard", "Doctor");

            if (user.RoleId == 1) // Admin
                return RedirectToAction("Dashboard", "Admin");

            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

    }
}