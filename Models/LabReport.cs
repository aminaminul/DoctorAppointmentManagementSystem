using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppointmentManagementSystem.Models
{
    public class LabReport
    {
        // Primary Key
        [Key]
        public int Id { get; set; }

        [Required]
        // Patient Association
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient Patient { get; set; }

        [Required]
        // Lab Test Association
        public int LabTestId { get; set; }

        [ForeignKey("LabTestId")]
        public LabTest LabTest { get; set; }

        [Required]
        // Report Details
        public DateTime ReportDate { get; set; }

        public string Result { get; set; }

        public string Remarks { get; set; }
    }
}

