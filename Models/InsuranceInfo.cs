using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppointmentManagementSystem.Models
{
    public class InsuranceInfo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [Required]
        public string ProviderName { get; set; }

        [Required]
        public string PolicyNumber { get; set; }

        public string? CoverageDetails { get; set; }

        public DateTime ExpiryDate { get; set; } = DateTime.Today.AddYears(1);

        public string Status { get; set; } = "Active"; // Active, Expired, Pending
    }
}
