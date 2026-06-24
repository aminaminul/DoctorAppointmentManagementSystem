using System;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentManagementSystem.Models
{
    public class AdminLog
    {
        public int Id { get; set; }
        public int AdminId { get; set; } // Points to User.Id (who has Admin role)
        
        [Required]
        public string ActionPerformed { get; set; }
        
        public string? Description { get; set; }
        
        public DateTime ActionDateTime { get; set; } = DateTime.Now;

        public User Admin { get; set; }
    }
}
