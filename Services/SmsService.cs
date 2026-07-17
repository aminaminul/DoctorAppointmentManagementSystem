namespace DoctorAppointmentManagementSystem.Services
{
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
                _logger.LogWarning("SMS skipped â€” recipient has no phone number on record.");
                return Task.CompletedTask;
            }

            _logger.LogInformation("ðŸ“± [SMS-STUB] To: {Phone} | Message: {Msg}", phoneNumber, message);
            return Task.CompletedTask;
        }
    }
}
