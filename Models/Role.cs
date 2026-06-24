using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentManagementSystem.Models
{
    public class Role
    {
        public int Id { get; set; }
        
        [Required]
        public string RoleName { get; set; }
        
        public string? Description { get; set; }
    }
}
