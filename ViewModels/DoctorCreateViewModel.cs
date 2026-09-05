namespace DoctorAppointmentManagementSystem.ViewModels
{
    public class DoctorCreateViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public string Specialization { get; set; }
        public string Availability { get; set; }
        public string? Qualification { get; set; }
        public int Experience { get; set; } = 0;
        public decimal ConsultationFee { get; set; } = 0;
        public string? PhoneNumber { get; set; }
        public string? AvailableDays { get; set; } = "Mon-Fri";
    }
}

