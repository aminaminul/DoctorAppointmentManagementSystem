using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentManagementSystem.Models
{
    public class SystemSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Key { get; set; } // e.g., "HospitalName", "SMSEnabled", "EmailSmtpHost"

        [Required]
        public string Value { get; set; }

        public string Description { get; set; }
    }
}
