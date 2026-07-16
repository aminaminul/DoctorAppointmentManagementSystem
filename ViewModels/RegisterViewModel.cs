using System;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentManagementSystem.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Email must be a Gmail address (@gmail.com)")]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public int RoleId { get; set; }

        public string? RoleName { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public int? Age { get; set; }
        public DateTime? DateOfBirth { get; set; }
        [Required(ErrorMessage = "Please select gender")]
        public string Gender { get; set; }

        public string? Specialization { get; set; }
        public string? Availability { get; set; }
        public DoctorAppointmentManagementSystem.Models.Department? Department { get; set; }
    }
}