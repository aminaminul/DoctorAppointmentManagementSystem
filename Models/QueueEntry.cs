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

        public int SequenceNumber { get; set; }

        [Required]
        public string Status { get; set; } = "Waiting";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? CallTime { get; set; }
        
        public DateTime? CompletionTime { get; set; }
    }
}
