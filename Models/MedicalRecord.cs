using System;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentManagementSystem.Models
{
    public class MedicalRecord
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        
        [Required]
        public string Diagnosis { get; set; }
        
        public string? TreatmentDetails { get; set; }
        
        public string? Notes { get; set; }
        
        public DateTime RecordDate { get; set; } = DateTime.Now;

        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
    }
}
