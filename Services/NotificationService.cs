using DoctorAppointmentManagementSystem.Data;
using DoctorAppointmentManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointmentManagementSystem.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService        _emailService;
        private readonly ISmsService          _smsService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            ApplicationDbContext context,
            IEmailService emailService,
            ISmsService smsService,
            ILogger<NotificationService> logger)
        {
            _context      = context;
            _emailService = emailService;
            _smsService   = smsService;
            _logger       = logger;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  BOOKING CONFIRMATION  (patient books → status "Pending")
        // ─────────────────────────────────────────────────────────────────────────
        public async Task SendAppointmentConfirmationAsync(Appointment appointment)
        {
            var appt = await LoadAppointmentAsync(appointment.Id);
            if (appt == null) return;

            const string title = "Appointment Booked Successfully";
            var msg = $"Your appointment with Dr. {appt.Doctor.User.FullName} " +
                      $"({appt.Doctor.Specialization}) on {appt.AppointmentDate:dd MMM yyyy} at " +
                      $"{appt.AppointmentTime} has been booked. Status: Pending — awaiting doctor confirmation.";

            await SaveInAppAsync(appt.Patient.UserId, "Booking", title, msg);

            await _emailService.SendEmailAsync(
                appt.Patient.User.Email, appt.Patient.User.FullName, title,
                BuildEmailHtml(title, appt.Patient.User.FullName, msg, "📅", "#1e3c72",
                    "Your appointment request has been received. You will be notified once the doctor confirms it."));
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  APPOINTMENT CONFIRMED  (doctor approves)
        // ─────────────────────────────────────────────────────────────────────────
        public async Task SendAppointmentApprovedAsync(Appointment appointment)
        {
            var appt = await LoadAppointmentAsync(appointment.Id);
            if (appt == null) return;

            const string title = "Appointment Confirmed ✅";
            var msg = $"Your appointment with Dr. {appt.Doctor.User.FullName} has been CONFIRMED for " +
                      $"{appt.AppointmentDate:dd MMM yyyy} at {appt.AppointmentTime}. " +
                      $"Please arrive 10 minutes early.";

            await SaveInAppAsync(appt.Patient.UserId, "Confirmation", title, msg);

            await _emailService.SendEmailAsync(
                appt.Patient.User.Email, appt.Patient.User.FullName, title,
                BuildEmailHtml(title, appt.Patient.User.FullName, msg, "✅", "#065f46",
                    "Please make sure to arrive on time. Bring any previous medical records if relevant."));

            await _smsService.SendSmsAsync(appt.Patient.User.PhoneNumber,
                $"CONFIRMED: Appt with Dr. {appt.Doctor.User.FullName} on " +
                $"{appt.AppointmentDate:dd MMM} at {appt.AppointmentTime}. Arrive 10 mins early.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  APPOINTMENT CANCELLED  (doctor rejects)
        // ─────────────────────────────────────────────────────────────────────────
        public async Task SendAppointmentCancelledAsync(Appointment appointment)
        {
            var appt = await LoadAppointmentAsync(appointment.Id);
            if (appt == null) return;

            const string title = "Appointment Cancelled ❌";
            var msg = $"Your appointment with Dr. {appt.Doctor.User.FullName} scheduled for " +
                      $"{appt.AppointmentDate:dd MMM yyyy} at {appt.AppointmentTime} has been CANCELLED. " +
                      $"Please contact the clinic or book a new appointment.";

            await SaveInAppAsync(appt.Patient.UserId, "Cancellation", title, msg);

            await _emailService.SendEmailAsync(
                appt.Patient.User.Email, appt.Patient.User.FullName, title,
                BuildEmailHtml(title, appt.Patient.User.FullName, msg, "❌", "#991b1b",
                    "We apologize for any inconvenience. You can book a new appointment at any time."));

            await _smsService.SendSmsAsync(appt.Patient.User.PhoneNumber,
                $"CANCELLED: Your appt with Dr. {appt.Doctor.User.FullName} on " +
                $"{appt.AppointmentDate:dd MMM} has been cancelled.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  APPOINTMENT DELAYED
        // ─────────────────────────────────────────────────────────────────────────
        public async Task SendAppointmentDelayedAsync(Appointment appointment)
        {
            var appt = await LoadAppointmentAsync(appointment.Id);
            if (appt == null) return;

            const string title = "Appointment Delayed ⏱";
            var msg = $"Your appointment with Dr. {appt.Doctor.User.FullName} on " +
                      $"{appt.AppointmentDate:dd MMM yyyy} at {appt.AppointmentTime} has been marked as DELAYED. " +
                      $"Please wait — the clinic team will update you with a new time shortly.";

            await SaveInAppAsync(appt.Patient.UserId, "Delay", title, msg);

            await _emailService.SendEmailAsync(
                appt.Patient.User.Email, appt.Patient.User.FullName, title,
                BuildEmailHtml(title, appt.Patient.User.FullName, msg, "⏱", "#713f12",
                    "Please hold — the clinic will contact you with updated timing information."));

            await _smsService.SendSmsAsync(appt.Patient.User.PhoneNumber,
                $"DELAYED: Your appt with Dr. {appt.Doctor.User.FullName} on " +
                $"{appt.AppointmentDate:dd MMM} is delayed. The clinic will update you shortly.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  PRESCRIPTION READY
        // ─────────────────────────────────────────────────────────────────────────
        public async Task SendPrescriptionReadyAsync(Prescription prescription)
        {
            var presc = await _context.Prescriptions
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Patient).ThenInclude(pat => pat.User)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Doctor).ThenInclude(d => d.User)
                .FirstOrDefaultAsync(p => p.Id == prescription.Id);

            if (presc == null) return;

            var patient = presc.Appointment.Patient;
            var doctor  = presc.Appointment.Doctor;

            const string title = "Prescription Available 💊";
            var msg = $"Dr. {doctor.User.FullName} has written a prescription for you. " +
                      $"Diagnosis: {presc.Diagnosis}. Log in to view your full prescription and medicines.";

            await SaveInAppAsync(patient.UserId, "Prescription", title, msg);

            await _emailService.SendEmailAsync(
                patient.User.Email, patient.User.FullName, title,
                BuildEmailHtml(title, patient.User.FullName, msg, "💊", "#1e40af",
                    $"<strong>Diagnosis:</strong> {presc.Diagnosis}<br>" +
                    $"<strong>Medicines:</strong> {presc.Medicines}<br>" +
                    $"<strong>Instructions:</strong> {presc.Instructions ?? "Follow doctor's advice"}"));
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  APPOINTMENT REMINDER  (24h before)
        // ─────────────────────────────────────────────────────────────────────────
        public async Task SendAppointmentReminderAsync(Appointment appointment)
        {
            var appt = await LoadAppointmentAsync(appointment.Id);
            if (appt == null) return;

            const string title = "🔔 Appointment Reminder — Tomorrow";
            var msg = $"Reminder: You have an appointment with Dr. {appt.Doctor.User.FullName} " +
                      $"({appt.Doctor.Specialization}) TOMORROW — {appt.AppointmentDate:dd MMM yyyy} " +
                      $"at {appt.AppointmentTime}. Please arrive 10–15 minutes early.";

            await SaveInAppAsync(appt.Patient.UserId, "Reminder", title, msg);

            await _emailService.SendEmailAsync(
                appt.Patient.User.Email, appt.Patient.User.FullName, title,
                BuildEmailHtml(title, appt.Patient.User.FullName, msg, "🔔", "#1e3c72",
                    "Remember to bring your insurance card, ID, and any previous medical records."));

            await _smsService.SendSmsAsync(appt.Patient.User.PhoneNumber,
                $"REMINDER: Tomorrow's appt with Dr. {appt.Doctor.User.FullName} at " +
                $"{appt.AppointmentTime}. Arrive 10 mins early.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  HELPERS
        // ─────────────────────────────────────────────────────────────────────────
        private async Task<Appointment?> LoadAppointmentAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        private async Task SaveInAppAsync(int userId, string type, string title, string message)
        {
            try
            {
                _context.Notifications.Add(new Notification
                {
                    UserId             = userId,
                    NotificationType   = type,
                    Title              = title,
                    Message            = message,
                    SentDateTime       = DateTime.Now,
                    NotificationStatus = "Unread"
                });
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save in-app notification for UserId={UserId}", userId);
            }
        }

        private static string BuildEmailHtml(string title, string recipientName,
            string mainMessage, string icon, string accentColor, string extraInfo)
        {
            return $@"<!DOCTYPE html>
<html>
<head>
  <meta charset=""utf-8""/>
  <meta name=""viewport"" content=""width=device-width, initial-scale=1""/>
  <style>
    body {{ font-family:'Segoe UI',Arial,sans-serif; background:#f4f7fb; margin:0; padding:20px; }}
    .wrapper {{ max-width:600px; margin:0 auto; background:white; border-radius:16px; overflow:hidden; box-shadow:0 4px 24px rgba(0,0,0,.08); }}
    .header {{ background:linear-gradient(135deg,{accentColor} 0%,#2a5298 100%); padding:32px 40px; text-align:center; }}
    .header .icon {{ font-size:52px; margin-bottom:10px; }}
    .header h1 {{ color:white; margin:0; font-size:22px; font-weight:700; letter-spacing:-.3px; }}
    .body {{ padding:32px 40px; }}
    .greeting {{ font-size:16px; color:#374151; font-weight:600; margin-bottom:16px; }}
    .main-msg {{ font-size:15px; color:#4b5563; line-height:1.75; background:#f8fafc; border-left:4px solid {accentColor}; padding:16px 20px; border-radius:0 8px 8px 0; margin-bottom:20px; }}
    .extra {{ font-size:14px; color:#6b7280; line-height:1.6; padding:12px 16px; background:#fffbeb; border-radius:8px; }}
    .footer {{ background:#f8fafc; padding:20px 40px; text-align:center; font-size:12px; color:#9ca3af; border-top:1px solid #f1f5f9; }}
    .btn {{ display:inline-block; margin-top:20px; padding:12px 28px; background:{accentColor}; color:white; border-radius:8px; text-decoration:none; font-weight:600; font-size:14px; }}
  </style>
</head>
<body>
  <div class=""wrapper"">
    <div class=""header"">
      <div class=""icon"">{icon}</div>
      <h1>{title}</h1>
    </div>
    <div class=""body"">
      <div class=""greeting"">Dear {recipientName},</div>
      <div class=""main-msg"">{mainMessage}</div>
      <div class=""extra"">{extraInfo}</div>
    </div>
    <div class=""footer"">
      <p>This is an automated message from <strong>Doctor Appointment Management System</strong>.</p>
      <p>Please do not reply to this email. For support, contact the clinic directly.</p>
    </div>
  </div>
</body>
</html>";
        }
    }
}
