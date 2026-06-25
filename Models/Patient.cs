using System;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppointmentManagementSystem.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        
        [Required]
        public string Gender { get; set; }
        
        [NotMapped]
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Year;
                if (DateOfBirth.Date > today.AddYears(-age)) age--;
                return age;
            }
            set
            {
                DateOfBirth = DateTime.Today.AddYears(-value);
            }
        }
        
        public DateTime DateOfBirth { get; set; }
        
        public string? BloodGroup { get; set; }
        
        public string? Address { get; set; }
        
        public string? EmergencyContact { get; set; }

        public string? MedicalHistory { get; set; }

        public string? Allergies { get; set; }
        
        public bool ActiveStatus { get; set; } = true;

        public User User { get; set; }
    }
}
