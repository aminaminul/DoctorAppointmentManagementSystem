using System;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppointmentManagementSystem.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        
        [NotMapped]
        public DateTime Date
        {
            get => AppointmentDate;
            set => AppointmentDate = value;
        }

        [NotMapped]
        public string TimeSlot
        {
            get => AppointmentTime;
            set => AppointmentTime = value;
        }

        [NotMapped]
        public string Status
        {
            get => AppointmentStatus;
            set => AppointmentStatus = value;
        }
        
        public DateTime AppointmentDate { get; set; }
        
        [Required]
        public string AppointmentTime { get; set; }
        
        public string? ReasonForVisit { get; set; }
        
        [Required]
        public string AppointmentStatus { get; set; } // Pending, Confirmed, Completed, Cancelled
        
        public bool IsEmergency { get; set; } = false;

        public DateTime BookingDateTime { get; set; } = DateTime.Now;

        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
    }
}
