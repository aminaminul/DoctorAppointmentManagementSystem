using System;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppointmentManagementSystem.Models
{
    public class Prescription
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        
        [NotMapped]
        public string? Advice
        {
            get => Instructions;
            set => Instructions = value;
        }

        [NotMapped]
        public DateTime CreatedDate
        {
            get => PrescriptionDateTime;
            set => PrescriptionDateTime = value;
        }
        
        [Required]
        public string Diagnosis { get; set; }
        
        [Required]
        public string Medicines { get; set; }
        
        public string? Instructions { get; set; }
        
        public DateTime PrescriptionDateTime { get; set; } = DateTime.Now;
        
        [Required]
        public string Status { get; set; } // Active, Cancelled, etc.

        public Appointment Appointment { get; set; }
        public Doctor Doctor { get; set; }
        public Patient Patient { get; set; }
    }
}
