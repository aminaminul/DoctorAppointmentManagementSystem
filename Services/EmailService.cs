using System.Net;
using System.Net.Mail;

namespace DoctorAppointmentManagementSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            var smtpHost     = _config["EmailSettings:SmtpHost"];
            var smtpPortStr  = _config["EmailSettings:SmtpPort"];
            var senderEmail  = _config["EmailSettings:SenderEmail"];
            var senderPass   = _config["EmailSettings:SenderPassword"];
            var senderName   = _config["EmailSettings:SenderName"] ?? "Doctor Appointment System";
            var enableSslStr = _config["EmailSettings:EnableSsl"];

            if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(senderEmail) ||
                string.IsNullOrWhiteSpace(senderPass))
            {
                _logger.LogWarning(
                    "Email NOT sent to {Email} â€” SMTP credentials not configured in appsettings.json. " +
                    "Fill in EmailSettings section to enable real email delivery.", toEmail);
                return;
            }

            int  port = int.TryParse(smtpPortStr, out var p) ? p : 587;
            bool ssl  = !string.Equals(enableSslStr, "false", StringComparison.OrdinalIgnoreCase);

            try
            {
                using var smtp = new SmtpClient(smtpHost, port)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPass),
                    EnableSsl   = ssl
                };

                var mail = new MailMessage
                {
                    From       = new MailAddress(senderEmail, senderName),
                    Subject    = subject,
                    Body       = htmlBody,
                    IsBodyHtml = true
                };
                mail.To.Add(new MailAddress(toEmail, toName));

                await smtp.SendMailAsync(mail);
                _logger.LogInformation("âœ‰ Email sent to {Email} â€” Subject: {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email} â€” Subject: {Subject}", toEmail, subject);
            }
        }
    }
}
