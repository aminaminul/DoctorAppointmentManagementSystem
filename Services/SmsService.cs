namespace DoctorAppointmentManagementSystem.Services
{
    /// <summary>
    /// SMS stub that logs messages. Replace the body of SendSmsAsync with
    /// a Twilio API call once you have credentials.
    /// </summary>
    public class SmsService : ISmsService
    {
        private readonly ILogger<SmsService> _logger;

        public SmsService(ILogger<SmsService> logger)
        {
            _logger = logger;
        }

        public Task SendSmsAsync(string? phoneNumber, string message)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                _logger.LogWarning("SMS skipped — recipient has no phone number on record.");
                return Task.CompletedTask;
            }

            // ── Twilio integration point ─────────────────────────────────────────────
            // To enable real SMS with Twilio:
            // 1. Install package: dotnet add package Twilio
            // 2. Add to appsettings.json:
            //    "TwilioSettings": { "AccountSid": "...", "AuthToken": "...", "FromNumber": "+1..." }
            // 3. Replace the log below with:
            //    TwilioClient.Init(accountSid, authToken);
            //    MessageResource.CreateAsync(to: new PhoneNumber(phoneNumber),
            //        from: new PhoneNumber(fromNumber), body: message);
            // ─────────────────────────────────────────────────────────────────────────

            _logger.LogInformation("📱 [SMS-STUB] To: {Phone} | Message: {Msg}", phoneNumber, message);
            return Task.CompletedTask;
        }
    }
}
