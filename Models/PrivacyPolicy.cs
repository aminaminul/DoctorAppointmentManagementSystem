using System;

namespace DoctorAppointmentManagementSystem.Models
{
    public class PrivacyPolicy
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
