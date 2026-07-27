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

        public string? Symptoms { get; set; }

        public string? TreatmentPlan { get; set; }
        
        public string? TreatmentDetails { get; set; }
        
        public string? TestReports { get; set; }

        public string? VitalSigns { get; set; }

        public string? FollowUpNotes { get; set; }

        public string? Allergies { get; set; }

        public string? ChronicDiseases { get; set; }
        
        public string? Notes { get; set; }
        
        public DateTime RecordDate { get; set; } = DateTime.Now;

        public int? AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
    }
}
