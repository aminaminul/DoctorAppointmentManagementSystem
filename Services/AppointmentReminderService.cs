using DoctorAppointmentManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointmentManagementSystem.Services
{
    /// <summary>
    /// Background service that runs every hour and sends appointment reminders
    /// for appointments scheduled for tomorrow.
    /// </summary>
    public class AppointmentReminderService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppointmentReminderService> _logger;
        private Timer? _timer;

        public AppointmentReminderService(
            IServiceProvider serviceProvider,
            ILogger<AppointmentReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger          = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🔔 AppointmentReminderService started — checking every hour for tomorrow's appointments.");

            // Run every 60 minutes, first run after 1 minute
            _timer = new Timer(DoWork, null, TimeSpan.FromMinutes(1), TimeSpan.FromHours(1));
            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            _logger.LogInformation("🔔 Reminder check triggered at {Time}", DateTime.Now);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                var tomorrow = DateTime.Today.AddDays(1);

                // Find confirmed appointments for tomorrow that haven't been reminded
                // We check if a "Reminder" notification already exists for the patient/appointment
                var appointmentsTomorrow = await context.Appointments
                    .Include(a => a.Patient).ThenInclude(p => p.User)
                    .Include(a => a.Doctor).ThenInclude(d => d.User)
                    .Where(a => a.AppointmentDate.Date == tomorrow.Date
                             && (a.AppointmentStatus == "Confirmed" || a.AppointmentStatus == "Approved"))
                    .ToListAsync();

                if (!appointmentsTomorrow.Any())
                {
                    _logger.LogInformation("🔔 No appointments scheduled for tomorrow ({Date}).", tomorrow.ToString("dd MMM yyyy"));
                    return;
                }

                int sent = 0;
                foreach (var appt in appointmentsTomorrow)
                {
                    // Check if we already sent a reminder for this appointment
                    bool alreadyReminded = await context.Notifications
                        .AnyAsync(n => n.UserId == appt.Patient.UserId
                                    && n.NotificationType == "Reminder"
                                    && n.Title.Contains("Reminder")
                                    && n.SentDateTime.Date == DateTime.Today);

                    if (!alreadyReminded)
                    {
                        await notificationService.SendAppointmentReminderAsync(appt);
                        sent++;
                    }
                }

                _logger.LogInformation("🔔 Sent {Count} reminder(s) for {Date}.", sent, tomorrow.ToString("dd MMM yyyy"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔔 Error in AppointmentReminderService.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🔔 AppointmentReminderService stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
