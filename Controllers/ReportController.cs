using DoctorAppointmentManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DoctorAppointmentManagementSystem.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }
// Index Action
        public IActionResult Index()
        {
            return View();
        }
// Revenue Action
        public IActionResult Revenue()
        {
            var payments = _context.Payments.Include(p => p.Appointment).ThenInclude(a => a.Patient).ThenInclude(p => p.User).ToList();
            ViewBag.TotalRevenue = payments.Sum(p => p.Amount);
            return View(payments);
        }
// Appointment Action
        public IActionResult Appointment()
        {
            var appointments = _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .ToList();
            return View(appointments);
        }
// Patient Action
        public IActionResult Patient()
        {
            var patients = _context.Patients.Include(p => p.User).ToList();
            return View(patients);
        }
// Doctor Action
        public IActionResult Doctor()
        {
            var doctors = _context.Doctors.Include(d => d.User).ToList();
            return View(doctors);
        }

        // PrintInvoice Action
        public IActionResult PrintInvoice(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.Patient).ThenInclude(p => p.User)
                .Include(i => i.Appointment).ThenInclude(a => a.Doctor).ThenInclude(d => d.User)
                .FirstOrDefault(i => i.Id == id || i.AppointmentId == id);

            if (invoice == null) return NotFound();

            var payment = _context.Payments
                .FirstOrDefault(p => p.AppointmentId == invoice.AppointmentId || p.PatientId == invoice.PatientId);
            ViewBag.Payment = payment;

            return View("~/Views/Patient/PrintInvoice.cshtml", invoice);
        }
    }
}

