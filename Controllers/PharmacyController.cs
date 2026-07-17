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
// Index Action
        public IActionResult Index()
        {
            var medicines = _context.Medicines.ToList();
            return View(medicines);
        }
// Create Action
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
// Create Action
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
// Edit Action
        public IActionResult Edit(int id)
        {
            var medicine = _context.Medicines.Find(id);
            if (medicine == null) return NotFound();
            return View(medicine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
// Edit Action
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
// Details Action
        public IActionResult Details(int id)
        {
            var medicine = _context.Medicines.Find(id);
            if (medicine == null) return NotFound();
            return View(medicine);
        }
// Delete Action
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

