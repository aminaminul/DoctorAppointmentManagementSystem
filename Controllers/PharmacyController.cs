using DoctorAppointmentManagementSystem.Data;
using DoctorAppointmentManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointmentManagementSystem.Controllers
{
    public class PharmacyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PharmacyController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var medicines = _context.Medicines.ToList();
            return View(medicines);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Medicine medicine)
        {
            if (ModelState.IsValid)
            {
                _context.Medicines.Add(medicine);
                _context.SaveChanges();
                TempData["Success"] = "Medicine added successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(medicine);
        }

        public IActionResult Edit(int id)
        {
            var medicine = _context.Medicines.Find(id);
            if (medicine == null) return NotFound();
            return View(medicine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Medicine medicine)
        {
            if (ModelState.IsValid)
            {
                _context.Medicines.Update(medicine);
                _context.SaveChanges();
                TempData["Success"] = "Medicine updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(medicine);
        }

        public IActionResult Details(int id)
        {
            var medicine = _context.Medicines.Find(id);
            if (medicine == null) return NotFound();
            return View(medicine);
        }

        public IActionResult Delete(int id)
        {
            var medicine = _context.Medicines.Find(id);
            if (medicine != null)
            {
                _context.Medicines.Remove(medicine);
                _context.SaveChanges();
                TempData["Success"] = "Medicine deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
