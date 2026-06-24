using System;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentManagementSystem.ViewModels
{
    public class DoctorScheduleViewModel
    {
        public int Id { get; set; }   // 0 = new, >0 = edit

        // ── Core ────────────────────────────────────────────────────────────────
        [Required(ErrorMessage = "Please select a date.")]
        [DataType(DataType.Date)]
        public DateTime AvailableDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Start time is required.")]
        public string StartTime { get; set; } = "09:00 AM";

        [Required(ErrorMessage = "End time is required.")]
        public string EndTime { get; set; } = "05:00 PM";

        // ── Break ───────────────────────────────────────────────────────────────
        public string? BreakStartTime { get; set; }
        public string? BreakEndTime { get; set; }

        // ── Vacation ────────────────────────────────────────────────────────────
        public bool IsVacation { get; set; } = false;

        [MaxLength(300)]
        public string? Notes { get; set; }

        // ── Bulk vacation range ─────────────────────────────────────────────────
        [DataType(DataType.Date)]
        public DateTime? VacationFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? VacationTo { get; set; }
    }
}
