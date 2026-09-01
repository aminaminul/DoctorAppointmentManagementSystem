using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppointmentManagementSystem.Models
{
    public class MedicalCertificate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CertificateNumber { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public Doctor? Doctor { get; set; }

        [Required]
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [Required]
        public string CertificateType { get; set; } // Medical Certificate, Fitness Certificate, Sick Leave Certificate

        public DateTime IssueDate { get; set; } = DateTime.Now;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Required]
        public string Diagnosis { get; set; }

        public string? Remarks { get; set; }
    }
}
