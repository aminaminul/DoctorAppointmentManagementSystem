using DoctorAppointmentManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using DoctorAppointmentManagementSystem.Data;

namespace DoctorAppointmentManagementSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DashboardController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var model = new DashboardViewModel
            {
                TotalDoctors = _db.Doctors.Count(),
                TotalPatients = _db.Patients.Count(),
                TotalAppointments = _db.Appointments.Count(),
                TodaysAppointments = _db.Appointments.Count(a => a.AppointmentDate.Date == DateTime.Today),
                PendingRequests = _db.Appointments.Count(a => a.AppointmentStatus == "Pending"),
                CompletedAppointments = _db.Appointments.Count(a => a.AppointmentStatus == "Completed")
            };

            // Build last 7 days series for a small SVG chart
            var labels = new List<string>();
            var values = new List<int>();
            for (int i = 6; i >= 0; i--)
            {
                var day = DateTime.Today.AddDays(-i);
                labels.Add(day.ToString("MMM dd"));
                values.Add(_db.Appointments.Count(a => a.AppointmentDate.Date == day));
            }
            model.LabelsLast7Days = labels.ToArray();
            model.AppointmentsLast7Days = values.ToArray();

            return View(model);
        }
    }
}
