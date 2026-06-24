using System;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentManagementSystem.Models
{
    public class DoctorSchedule
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }

        // ── Core schedule ──────────────────────────────────────────────────────
        [Required]
        public DateTime AvailableDate { get; set; }

        [Required]
        public string StartTime { get; set; }      // e.g. "09:00 AM"

        [Required]
        public string EndTime { get; set; }        // e.g. "05:00 PM"

        // ── Break window (optional) ────────────────────────────────────────────
        public string? BreakStartTime { get; set; }    // e.g. "01:00 PM"
        public string? BreakEndTime { get; set; }      // e.g. "02:00 PM"

        // ── Slot status ────────────────────────────────────────────────────────
        /// <summary>Available | Booked | Blocked</summary>
        [Required]
        public string SlotStatus { get; set; } = "Available";

        // ── Vacation / leave ───────────────────────────────────────────────────
        /// <summary>When true the whole day is blocked for vacation/leave.</summary>
        public bool IsVacation { get; set; } = false;

        /// <summary>Optional note: "Public Holiday", "Conference", "Personal Leave" …</summary>
        public string? Notes { get; set; }

        // ── Navigation ─────────────────────────────────────────────────────────
        public Doctor Doctor { get; set; }
    }
}
