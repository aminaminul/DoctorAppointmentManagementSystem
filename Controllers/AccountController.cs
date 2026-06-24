using DoctorAppointmentManagementSystem.Models;
using DoctorAppointmentManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

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
            User user = new User()
            {
                Name = model.Name,
                Email = model.Email,
                Password = model.Password,
                Role = model.Role
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // 🔥 STEP 5: Role wise create
            if (user.Role == "Patient")
            {
                Patient patient = new Patient()
                {
                    UserId = user.Id,
                    Age = 20,
                    Gender = model.Gender
                };

                _context.Patients.Add(patient);
            }
            else if (user.Role == "Doctor")
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
                .FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password!";
                return View(model);
            }

            // 🔥 STEP 3: Session set
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserRole", user.Role);

            // 🔥 STEP 4: Role-based redirect (IMPORTANT)
            if (user.Role == "Patient")
                return RedirectToAction("Dashboard", "Patient");

            if (user.Role == "Doctor")
                return RedirectToAction("Dashboard", "Doctor");

            if (user.Role == "Admin")
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