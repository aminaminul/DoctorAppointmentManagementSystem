using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppointmentManagementSystem.Models
{
    public class LabReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient Patient { get; set; }

        [Required]
        public int LabTestId { get; set; }

        [ForeignKey("LabTestId")]
        public LabTest LabTest { get; set; }

        [Required]
        public DateTime ReportDate { get; set; }

        public string Result { get; set; }

        public string Remarks { get; set; }
    }
}
