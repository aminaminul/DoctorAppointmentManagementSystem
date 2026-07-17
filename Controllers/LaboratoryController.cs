using DoctorAppointmentManagementSystem.Data;
using DoctorAppointmentManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointmentManagementSystem.Controllers
{
    public class LaboratoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LaboratoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Tests()
        {
            var tests = _context.LabTests.ToList();
            return View(tests);
        }

        public IActionResult CreateTest()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateTest(LabTest test)
        {
            if (ModelState.IsValid)
            {
                _context.LabTests.Add(test);
                _context.SaveChanges();
                return RedirectToAction(nameof(Tests));
            }
            return View(test);
        }

        public IActionResult Reports()
        {
            var reports = _context.LabReports
                .Include(r => r.Patient).ThenInclude(p => p.User)
                .Include(r => r.LabTest)
                .ToList();
            return View(reports);
        }

        public IActionResult CreateReport()
        {
            ViewBag.Patients = _context.Patients.Include(p => p.User).ToList();
            ViewBag.LabTests = _context.LabTests.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateReport(LabReport report)
        {
            _context.LabReports.Add(report);
            _context.SaveChanges();
            return RedirectToAction(nameof(Reports));
        }

        public IActionResult EditTest(int id)
        {
            var test = _context.LabTests.Find(id);
            if (test == null) return NotFound();
            return View(test);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditTest(LabTest test)
        {
            if (ModelState.IsValid)
            {
                _context.LabTests.Update(test);
                _context.SaveChanges();
                return RedirectToAction(nameof(Tests));
            }
            return View(test);
        }

        public IActionResult DeleteTest(int id)
        {
            var test = _context.LabTests.Find(id);
            if (test != null)
            {
                _context.LabTests.Remove(test);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Tests));
        }

        public IActionResult EditReport(int id)
        {
            var report = _context.LabReports.Find(id);
            if (report == null) return NotFound();
            ViewBag.Patients = _context.Patients.Include(p => p.User).ToList();
            ViewBag.LabTests = _context.LabTests.ToList();
            return View(report);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditReport(LabReport report)
        {
            _context.LabReports.Update(report);
            _context.SaveChanges();
            return RedirectToAction(nameof(Reports));
        }

        public IActionResult DeleteReport(int id)
        {
            var report = _context.LabReports.Find(id);
            if (report != null)
            {
                _context.LabReports.Remove(report);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Reports));
        }

        public IActionResult TestDetails(int id)
        {
            var test = _context.LabTests.Find(id);
            if (test == null) return NotFound();
            return View(test);
        }

        public IActionResult ReportDetails(int id)
        {
            var report = _context.LabReports
                .Include(r => r.Patient).ThenInclude(p => p.User)
                .Include(r => r.LabTest)
                .FirstOrDefault(r => r.Id == id);
            if (report == null) return NotFound();
            return View(report);
        }
    }
}
