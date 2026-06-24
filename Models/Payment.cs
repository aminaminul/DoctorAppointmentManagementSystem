using System;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentManagementSystem.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        
        public decimal Amount { get; set; }
        
        [Required]
        public string PaymentMethod { get; set; }
        
        public string? TransactionId { get; set; }
        
        public DateTime PaymentDateTime { get; set; } = DateTime.Now;
        
        [Required]
        public string PaymentStatus { get; set; } // Paid, Unpaid, Pending, etc.

        public Appointment Appointment { get; set; }
        public Patient Patient { get; set; }
    }
}
