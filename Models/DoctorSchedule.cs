using System;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentManagementSystem.Models
{
    public class DoctorSchedule
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }

        [Required]
        public DateTime AvailableDate { get; set; }

        [Required]
        public string StartTime { get; set; }

        [Required]
        public string EndTime { get; set; }

        public string? BreakStartTime { get; set; }
        public string? BreakEndTime { get; set; }

        [Required]
        public string SlotStatus { get; set; } = "Available";

        public bool IsVacation { get; set; } = false;

        public string? Notes { get; set; }

        public Doctor Doctor { get; set; }
    }
}
