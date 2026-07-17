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
// System Action
        public IActionResult System()
        {
            var settings = _context.SystemSettings.ToList();
            return View(settings);
        }

        [HttpPost]
// SaveSetting Action
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
// DeleteSetting Action
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
// Holidays Action
        public IActionResult Holidays()
        {
            return View(_context.Holidays.ToList());
        }

        [HttpPost]
// AddHoliday Action
        public IActionResult AddHoliday(Holiday holiday)
        {
            _context.Holidays.Add(holiday);
            _context.SaveChanges();
            return RedirectToAction(nameof(Holidays));
        }
// EditHoliday Action
        public IActionResult EditHoliday(int id)
        {
            var holiday = _context.Holidays.Find(id);
            if (holiday == null) return NotFound();
            return View(holiday);
        }

        [HttpPost]
// EditHoliday Action
        public IActionResult EditHoliday(Holiday holiday)
        {
            _context.Holidays.Update(holiday);
            _context.SaveChanges();
            return RedirectToAction(nameof(Holidays));
        }
// DeleteHoliday Action
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
// Branches Action
        public IActionResult Branches()
        {
            return View(_context.Branches.ToList());
        }

        [HttpPost]
// AddBranch Action
        public IActionResult AddBranch(Branch branch)
        {
            _context.Branches.Add(branch);
            _context.SaveChanges();
            return RedirectToAction(nameof(Branches));
        }
// EditBranch Action
        public IActionResult EditBranch(int id)
        {
            var branch = _context.Branches.Find(id);
            if (branch == null) return NotFound();
            return View(branch);
        }

        [HttpPost]
// EditBranch Action
        public IActionResult EditBranch(Branch branch)
        {
            _context.Branches.Update(branch);
            _context.SaveChanges();
            return RedirectToAction(nameof(Branches));
        }
// DeleteBranch Action
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
// Specializations Action
        public IActionResult Specializations()
        {
            return View(_context.Specializations.ToList());
        }

        [HttpPost]
// AddSpecialization Action
        public IActionResult AddSpecialization(Specialization spec)
        {
            _context.Specializations.Add(spec);
            _context.SaveChanges();
            return RedirectToAction(nameof(Specializations));
        }
// EditSpecialization Action
        public IActionResult EditSpecialization(int id)
        {
            var spec = _context.Specializations.Find(id);
            if (spec == null) return NotFound();
            return View(spec);
        }

        [HttpPost]
// EditSpecialization Action
        public IActionResult EditSpecialization(Specialization spec)
        {
            _context.Specializations.Update(spec);
            _context.SaveChanges();
            return RedirectToAction(nameof(Specializations));
        }
// DeleteSpecialization Action
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

