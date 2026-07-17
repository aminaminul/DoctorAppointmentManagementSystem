using DoctorAppointmentManagementSystem.Data;
using DoctorAppointmentManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace DoctorAppointmentManagementSystem.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult System()
        {
            var settings = _context.SystemSettings.ToList();
            return View(settings);
        }

        [HttpPost]
        public IActionResult SaveSetting(string key, string value)
        {
            var setting = _context.SystemSettings.FirstOrDefault(s => s.Key == key);
            if (setting == null)
            {
                _context.SystemSettings.Add(new SystemSetting { Key = key, Value = value });
            }
            else
            {
                setting.Value = value;
                _context.SystemSettings.Update(setting);
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(System));
        }

        public IActionResult DeleteSetting(int id)
        {
            var setting = _context.SystemSettings.Find(id);
            if (setting != null)
            {
                _context.SystemSettings.Remove(setting);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(System));
        }

        public IActionResult Holidays()
        {
            return View(_context.Holidays.ToList());
        }

        [HttpPost]
        public IActionResult AddHoliday(Holiday holiday)
        {
            _context.Holidays.Add(holiday);
            _context.SaveChanges();
            return RedirectToAction(nameof(Holidays));
        }

        public IActionResult EditHoliday(int id)
        {
            var holiday = _context.Holidays.Find(id);
            if (holiday == null) return NotFound();
            return View(holiday);
        }

        [HttpPost]
        public IActionResult EditHoliday(Holiday holiday)
        {
            _context.Holidays.Update(holiday);
            _context.SaveChanges();
            return RedirectToAction(nameof(Holidays));
        }

        public IActionResult DeleteHoliday(int id)
        {
            var holiday = _context.Holidays.Find(id);
            if (holiday != null)
            {
                _context.Holidays.Remove(holiday);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Holidays));
        }

        public IActionResult Branches()
        {
            return View(_context.Branches.ToList());
        }

        [HttpPost]
        public IActionResult AddBranch(Branch branch)
        {
            _context.Branches.Add(branch);
            _context.SaveChanges();
            return RedirectToAction(nameof(Branches));
        }

        public IActionResult EditBranch(int id)
        {
            var branch = _context.Branches.Find(id);
            if (branch == null) return NotFound();
            return View(branch);
        }

        [HttpPost]
        public IActionResult EditBranch(Branch branch)
        {
            _context.Branches.Update(branch);
            _context.SaveChanges();
            return RedirectToAction(nameof(Branches));
        }

        public IActionResult DeleteBranch(int id)
        {
            var branch = _context.Branches.Find(id);
            if (branch != null)
            {
                _context.Branches.Remove(branch);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Branches));
        }

        public IActionResult Specializations()
        {
            return View(_context.Specializations.ToList());
        }

        [HttpPost]
        public IActionResult AddSpecialization(Specialization spec)
        {
            _context.Specializations.Add(spec);
            _context.SaveChanges();
            return RedirectToAction(nameof(Specializations));
        }

        public IActionResult EditSpecialization(int id)
        {
            var spec = _context.Specializations.Find(id);
            if (spec == null) return NotFound();
            return View(spec);
        }

        [HttpPost]
        public IActionResult EditSpecialization(Specialization spec)
        {
            _context.Specializations.Update(spec);
            _context.SaveChanges();
            return RedirectToAction(nameof(Specializations));
        }

        public IActionResult DeleteSpecialization(int id)
        {
            var spec = _context.Specializations.Find(id);
            if (spec != null)
            {
                _context.Specializations.Remove(spec);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Specializations));
        }
    }
}
