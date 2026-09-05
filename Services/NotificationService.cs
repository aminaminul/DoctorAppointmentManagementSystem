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

        public async Task SendAppointmentConfirmationAsync(Appointment appointment)
        {
            var appt = await LoadAppointmentAsync(appointment.Id);
            if (appt == null) return;

            const string title = "Appointment Booked Successfully";
            var msg = $"Your appointment with Dr. {appt.Doctor.User.Username} " +
                      $"({appt.Doctor.Specialization}) on {appt.AppointmentDate:dd MMM yyyy} at " +
                      $"{appt.AppointmentTime} has been booked. Status: Pending â€” awaiting doctor confirmation.";

            await SaveInAppAsync(appt.Patient.UserId, "Booking", title, msg);

            await _emailService.SendEmailAsync(
                appt.Patient.User.Email, appt.Patient.User.Username, title,
                BuildEmailHtml(title, appt.Patient.User.Username, msg, "ðŸ“…", "#1e3c72",
                    "Your appointment request has been received. You will be notified once the doctor confirms it."));
        }

        public async Task SendAppointmentApprovedAsync(Appointment appointment)
        {
            var appt = await LoadAppointmentAsync(appointment.Id);
            if (appt == null) return;

            const string title = "Appointment Confirmed âœ…";
            var msg = $"Your appointment with Dr. {appt.Doctor.User.Username} has been CONFIRMED for " +
                      $"{appt.AppointmentDate:dd MMM yyyy} at {appt.AppointmentTime}. " +
                      $"Please arrive 10 minutes early.";

            await SaveInAppAsync(appt.Patient.UserId, "Confirmation", title, msg);

            await _emailService.SendEmailAsync(
                appt.Patient.User.Email, appt.Patient.User.Username, title,
                BuildEmailHtml(title, appt.Patient.User.Username, msg, "âœ…", "#065f46",
                    "Please make sure to arrive on time. Bring any previous medical records if relevant."));

            await _smsService.SendSmsAsync(appt.Patient.User.PhoneNumber,
                $"CONFIRMED: Appt with Dr. {appt.Doctor.User.Username} on " +
                $"{appt.AppointmentDate:dd MMM} at {appt.AppointmentTime}. Arrive 10 mins early.");
        }

        public async Task SendAppointmentCancelledAsync(Appointment appointment)
        {
            var appt = await LoadAppointmentAsync(appointment.Id);
            if (appt == null) return;

            const string title = "Appointment Cancelled âŒ";
            var msg = $"Your appointment with Dr. {appt.Doctor.User.Username} scheduled for " +
                      $"{appt.AppointmentDate:dd MMM yyyy} at {appt.AppointmentTime} has been CANCELLED. " +
                      $"Please contact the clinic or book a new appointment.";

            await SaveInAppAsync(appt.Patient.UserId, "Cancellation", title, msg);

            await _emailService.SendEmailAsync(
                appt.Patient.User.Email, appt.Patient.User.Username, title,
                BuildEmailHtml(title, appt.Patient.User.Username, msg, "âŒ", "#991b1b",
                    "We apologize for any inconvenience. You can book a new appointment at any time."));

            await _smsService.SendSmsAsync(appt.Patient.User.PhoneNumber,
                $"CANCELLED: Your appt with Dr. {appt.Doctor.User.Username} on " +
                $"{appt.AppointmentDate:dd MMM} has been cancelled.");
        }

        public async Task SendAppointmentDelayedAsync(Appointment appointment)
        {
            var appt = await LoadAppointmentAsync(appointment.Id);
            if (appt == null) return;

            const string title = "Appointment Delayed â±";
            var msg = $"Your appointment with Dr. {appt.Doctor.User.Username} on " +
                      $"{appt.AppointmentDate:dd MMM yyyy} at {appt.AppointmentTime} has been marked as DELAYED. " +
                      $"Please wait â€” the clinic team will update you with a new time shortly.";

            await SaveInAppAsync(appt.Patient.UserId, "Delay", title, msg);

            await _emailService.SendEmailAsync(
                appt.Patient.User.Email, appt.Patient.User.Username, title,
                BuildEmailHtml(title, appt.Patient.User.Username, msg, "â±", "#713f12",
                    "Please hold â€” the clinic will contact you with updated timing information."));

            await _smsService.SendSmsAsync(appt.Patient.User.PhoneNumber,
                $"DELAYED: Your appt with Dr. {appt.Doctor.User.Username} on " +
                $"{appt.AppointmentDate:dd MMM} is delayed. The clinic will update you shortly.");
        }

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

            const string title = "Prescription Available ðŸ’Š";
            var msg = $"Dr. {doctor.User.Username} has written a prescription for you. " +
                      $"Diagnosis: {presc.Diagnosis}. Log in to view your full prescription and medicines.";

            await SaveInAppAsync(patient.UserId, "Prescription", title, msg);

            await _emailService.SendEmailAsync(
                patient.User.Email, patient.User.Username, title,
                BuildEmailHtml(title, patient.User.Username, msg, "ðŸ’Š", "#1e40af",
                    $"<strong>Diagnosis:</strong> {presc.Diagnosis}<br>" +
                    $"<strong>Medicines:</strong> {presc.Medicines}<br>" +
                    $"<strong>Instructions:</strong> {presc.Instructions ?? "Follow doctor's advice"}"));
        }

        public async Task SendAppointmentReminderAsync(Appointment appointment)
        {
            var appt = await LoadAppointmentAsync(appointment.Id);
            if (appt == null) return;

            const string title = "ðŸ”” Appointment Reminder â€” Tomorrow";
            var msg = $"Reminder: You have an appointment with Dr. {appt.Doctor.User.Username} " +
                      $"({appt.Doctor.Specialization}) TOMORROW â€” {appt.AppointmentDate:dd MMM yyyy} " +
                      $"at {appt.AppointmentTime}. Please arrive 10â€“15 minutes early.";

            await SaveInAppAsync(appt.Patient.UserId, "Reminder", title, msg);

            await _emailService.SendEmailAsync(
                appt.Patient.User.Email, appt.Patient.User.Username, title,
                BuildEmailHtml(title, appt.Patient.User.Username, msg, "ðŸ””", "#1e3c72",
                    "Remember to bring your insurance card, ID, and any previous medical records."));

            await _smsService.SendSmsAsync(appt.Patient.User.PhoneNumber,
                $"REMINDER: Tomorrow's appt with Dr. {appt.Doctor.User.Username} at " +
                $"{appt.AppointmentTime}. Arrive 10 mins early.");
        }

        public async Task SendPaymentAndBookingNotificationAsync(Appointment appointment, Payment payment)
        {
            var appt = await LoadAppointmentAsync(appointment.Id);
            if (appt == null) return;

            decimal ticketFee = 50.00m;
            decimal docFee = payment.Amount > ticketFee ? payment.Amount - ticketFee : payment.Amount;

            // 1. Notification to PATIENT
            const string patientTitle = "Payment Successful & Appointment Confirmed ✅";
            string patientMsg = $"Your online payment of ৳{payment.Amount:N2} via {payment.PaymentMethod} (TrxID: {payment.TransactionId ?? "N/A"}) has been received. " +
                                $"Your appointment with Dr. {appt.Doctor.User.Username} ({appt.Doctor.Specialization}) is confirmed for " +
                                $"{appt.AppointmentDate:dd MMM yyyy} at {appt.AppointmentTime}. " +
                                $"(Doctor Consultation Fee: ৳{docFee:N2}, Hospital Ticket Fee: ৳{ticketFee:N2}).";

            await SaveInAppAsync(appt.Patient.UserId, "Payment", patientTitle, patientMsg);

            await _emailService.SendEmailAsync(
                appt.Patient.User.Email, appt.Patient.User.Username, patientTitle,
                BuildEmailHtml(patientTitle, appt.Patient.User.Username, patientMsg, "💳", "#047857",
                    $"<strong>Total Paid:</strong> ৳{payment.Amount:N2}<br>" +
                    $"<strong>Payment Method:</strong> {payment.PaymentMethod}<br>" +
                    $"<strong>Transaction ID:</strong> {payment.TransactionId ?? "N/A"}<br>" +
                    $"<strong>Status:</strong> Confirmed & Paid"));

            await _smsService.SendSmsAsync(appt.Patient.User.PhoneNumber,
                $"PAID: ৳{payment.Amount:N2} via {payment.PaymentMethod}. Appt #{appt.Id} with Dr. {appt.Doctor.User.Username} confirmed for {appt.AppointmentDate:dd MMM} at {appt.AppointmentTime}.");

            // 2. Notification to DOCTOR
            string doctorTitle = $"New Paid Appointment Confirmed (Appt #{appt.Id}) 🔔";
            string doctorMsg = $"Patient {appt.Patient.User.Username} has booked an appointment and paid ৳{payment.Amount:N2} via {payment.PaymentMethod} (TrxID: {payment.TransactionId ?? "N/A"}). " +
                               $"Scheduled for {appt.AppointmentDate:dd MMM yyyy} at {appt.AppointmentTime}. Status: Confirmed.";

            await SaveInAppAsync(appt.Doctor.UserId, "Booking", doctorTitle, doctorMsg);

            await _emailService.SendEmailAsync(
                appt.Doctor.User.Email, appt.Doctor.User.Username, doctorTitle,
                BuildEmailHtml(doctorTitle, appt.Doctor.User.Username, doctorMsg, "🩺", "#1e40af",
                    $"<strong>Patient:</strong> {appt.Patient.User.Username}<br>" +
                    $"<strong>Date & Time:</strong> {appt.AppointmentDate:dd MMM yyyy} at {appt.AppointmentTime}<br>" +
                    $"<strong>Payment Status:</strong> Paid (৳{payment.Amount:N2})"));
        }

        public async Task SendAppointmentRefundNotificationAsync(Appointment appointment, Payment payment, string reason)
        {
            var appt = await LoadAppointmentAsync(appointment.Id);
            if (appt == null) return;

            // 1. Notification to PATIENT
            const string patientTitle = "Appointment Cancelled & Full Refund Initiated ⚠️";
            string patientMsg = $"Your paid appointment #{appt.Id} with Dr. {appt.Doctor.User.Username} scheduled for " +
                                $"{appt.AppointmentDate:dd MMM yyyy} at {appt.AppointmentTime} has been cancelled. " +
                                $"Reason: {reason}. A full refund of ৳{payment.Amount:N2} has been initiated to your {payment.PaymentMethod} account (TrxID: {payment.TransactionId ?? "N/A"}).";

            await SaveInAppAsync(appt.Patient.UserId, "Refund", patientTitle, patientMsg);

            await _emailService.SendEmailAsync(
                appt.Patient.User.Email, appt.Patient.User.Username, patientTitle,
                BuildEmailHtml(patientTitle, appt.Patient.User.Username, patientMsg, "↩️", "#b91c1c",
                    $"<strong>Refund Amount:</strong> ৳{payment.Amount:N2}<br>" +
                    $"<strong>Refund Destination:</strong> {payment.PaymentMethod}<br>" +
                    $"<strong>Original TrxID:</strong> {payment.TransactionId ?? "N/A"}<br>" +
                    $"<strong>Cancellation Reason:</strong> {reason}"));

            await _smsService.SendSmsAsync(appt.Patient.User.PhoneNumber,
                $"REFUND INITIATED: ৳{payment.Amount:N2} for cancelled Appt #{appt.Id} with Dr. {appt.Doctor.User.Username}. Transferred to your {payment.PaymentMethod}.");

            // 2. Notification to DOCTOR
            string doctorTitle = $"Appointment #{appt.Id} Cancelled & Refunded";
            string doctorMsg = $"Appointment #{appt.Id} with patient {appt.Patient.User.Username} on {appt.AppointmentDate:dd MMM yyyy} at {appt.AppointmentTime} was cancelled (Reason: {reason}). " +
                               $"The patient's payment of ৳{payment.Amount:N2} has been marked for refund.";

            await SaveInAppAsync(appt.Doctor.UserId, "Cancellation", doctorTitle, doctorMsg);
        }

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
