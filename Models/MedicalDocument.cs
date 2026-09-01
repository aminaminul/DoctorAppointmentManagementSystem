using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppointmentManagementSystem.Models
{
    public class MedicalDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public Doctor? Doctor { get; set; }

        [Required]
        public string DocumentName { get; set; }

        [Required]
        public string DocumentType { get; set; } // X-Ray, MRI, CT Scan, Blood Report, Other

        [Required]
        public string FilePath { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;

        public string? Notes { get; set; }
    }
}
