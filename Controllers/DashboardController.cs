using DoctorAppointmentManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using DoctorAppointmentManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointmentManagementSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DashboardController(ApplicationDbContext db)
        {
            _db = db;
        }
// Index Action
        public IActionResult Index()
        {
            var today = DateTime.Today;
            var thisMonthStart = new DateTime(today.Year, today.Month, 1);

            var appointments = _db.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .ToList();

            var doctors = _db.Doctors.Include(d => d.User).ToList();
            var patients = _db.Patients.Include(p => p.User).ToList();
            var payments = _db.Payments.ToList();

            var recentAppointments = appointments
                .OrderByDescending(a => a.AppointmentDate)
                .Take(8)
                .Select(a => new RecentAppointmentItem
                {
                    Id = a.Id,
                    PatientName = a.Patient?.User?.Username ?? "Unknown",
                    DoctorName = a.Doctor?.User?.Username ?? "Unknown",
                    Specialization = a.Doctor?.Specialization ?? "",
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    Status = a.AppointmentStatus,
                    IsEmergency = a.IsEmergency
                }).ToList();

            var labels = new List<string>();
            var values = new List<int>();
            for (int i = 6; i >= 0; i--)
            {
                var day = today.AddDays(-i);
                labels.Add(day.ToString("MMM dd"));
                values.Add(appointments.Count(a => a.AppointmentDate.Date == day));
            }

            var monthlyLabels = new string[6];
            var monthlyAppts = new int[6];
            var monthlyRevData = new decimal[6];
            for (int i = 5; i >= 0; i--)
            {
                var month = today.AddMonths(-i);
                var idx = 5 - i;
                monthlyLabels[idx] = month.ToString("MMM yyyy");
                monthlyAppts[idx] = appointments.Count(a => a.AppointmentDate.Year == month.Year && a.AppointmentDate.Month == month.Month);
                monthlyRevData[idx] = payments
                    .Where(p => p.PaymentDateTime.Year == month.Year && p.PaymentDateTime.Month == month.Month)
                    .Sum(p => p.Amount);
            }

            var topSpecs = appointments
                .Where(a => a.Doctor?.Specialization != null)
                .GroupBy(a => a.Doctor!.Specialization)
                .Select(g => (Specialization: g.Key, Count: g.Count()))
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            var model = new DashboardViewModel
            {
                TotalDoctors = doctors.Count,
                TotalPatients = patients.Count,
                TotalAppointments = appointments.Count,
                TodaysAppointments = appointments.Count(a => a.AppointmentDate.Date == today),
                PendingRequests = appointments.Count(a => a.AppointmentStatus == "Pending"),
                CompletedAppointments = appointments.Count(a => a.AppointmentStatus == "Completed"),
                CancelledAppointments = appointments.Count(a => a.AppointmentStatus == "Cancelled"),
                ConfirmedAppointments = appointments.Count(a => a.AppointmentStatus == "Confirmed"),
                TotalRevenue = payments.Sum(p => p.Amount),
                TodaysRevenue = payments.Where(p => p.PaymentDateTime.Date == today).Sum(p => p.Amount),
                MonthlyRevenue = payments.Where(p => p.PaymentDateTime >= thisMonthStart).Sum(p => p.Amount),
                ActiveDoctors = doctors.Count(d => d.ActiveStatus),
                ActivePatients = patients.Count(p => p.ActiveStatus),
                EmergencyCases = appointments.Count(a => a.IsEmergency),
                NewPatientsThisMonth = patients.Count(p => p.User != null),
                PendingLeaveRequests = _db.LeaveRequests.Count(l => l.Status == "Pending"),
                OpenComplaints = _db.Complaints.Count(c => c.Status == "Open"),
                LabelsLast7Days = labels.ToArray(),
                AppointmentsLast7Days = values.ToArray(),
                MonthlyLabels = monthlyLabels,
                MonthlyAppointments = monthlyAppts,
                MonthlyRevenueData = monthlyRevData,
                StatusCounts = new int[]
                {
                    appointments.Count(a => a.AppointmentStatus == "Pending"),
                    appointments.Count(a => a.AppointmentStatus == "Confirmed"),
                    appointments.Count(a => a.AppointmentStatus == "Completed"),
                    appointments.Count(a => a.AppointmentStatus == "Cancelled")
                },
                RecentAppointments = recentAppointments,
                TopSpecializations = topSpecs
            };

            return View(model);
        }
    }
}

