using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppointmentManagementSystem.Models
{
    public class FollowUp
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public Doctor? Doctor { get; set; }

        [Required]
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        public int? OriginalAppointmentId { get; set; }

        [ForeignKey("OriginalAppointmentId")]
        public Appointment? OriginalAppointment { get; set; }

        [Required]
        public DateTime FollowUpDate { get; set; }

        public string? Notes { get; set; }

        [Required]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled
    }
}
