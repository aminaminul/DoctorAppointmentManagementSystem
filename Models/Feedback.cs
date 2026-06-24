using System;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentManagementSystem.Models
{
    public class Feedback
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        
        public int Rating { get; set; }
        
        public DateTime FeedbackDateTime { get; set; } = DateTime.Now;
        
        [Required]
        public string Status { get; set; } // Active, Blocked, etc.

        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
    }
}
