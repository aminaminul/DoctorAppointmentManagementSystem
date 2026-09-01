using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppointmentManagementSystem.Models
{
    public class FamilyMember
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Relationship { get; set; } // Spouse, Child, Parent, Sibling, Other

        public string Gender { get; set; } = "Male";

        public int Age { get; set; }

        public string? BloodGroup { get; set; }

        public string? EmergencyContact { get; set; }
    }
}
