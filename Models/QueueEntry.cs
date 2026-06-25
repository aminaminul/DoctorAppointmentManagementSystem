using System;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentManagementSystem.Models
{
    public class QueueEntry
    {
        public int Id { get; set; }
        
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }

        public int TokenNumber { get; set; }

        // SequenceNumber determines the actual current ordering in the live queue
        public int SequenceNumber { get; set; }

        // Status: Waiting, Calling, InConsultation, Completed, Skipped
        [Required]
        public string Status { get; set; } = "Waiting";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? CallTime { get; set; }
        
        public DateTime? CompletionTime { get; set; }
    }
}
