using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppointmentManagementSystem.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string Password { get; set; }
        
        public string? PhoneNumber { get; set; }
        
        public DateTime AccountCreationDateTime { get; set; } = DateTime.Now;
        
        public bool ActiveStatus { get; set; } = true;

        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}