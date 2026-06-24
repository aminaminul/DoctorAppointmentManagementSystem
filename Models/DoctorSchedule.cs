using System;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentManagementSystem.Models
{
    public class DoctorSchedule
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        
        public DateTime AvailableDate { get; set; }
        
        [Required]
        public string StartTime { get; set; }
        
        [Required]
        public string EndTime { get; set; }
        
        [Required]
        public string SlotStatus { get; set; } // Available/Booked

        public Doctor Doctor { get; set; }
    }
}
