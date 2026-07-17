using System;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentManagementSystem.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        
        [Required]
        public string NotificationType { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        [Required]
        public string Message { get; set; }
        
        public DateTime SentDateTime { get; set; } = DateTime.Now;
        
        [Required]
        public string NotificationStatus { get; set; } // Read/Unread

        public User User { get; set; }
    }
}
