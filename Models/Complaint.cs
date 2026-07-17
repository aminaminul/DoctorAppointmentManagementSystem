using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppointmentManagementSystem.Models
{
    public class Complaint
    {
        // Primary Key
        [Key]
        public int Id { get; set; }

        [Required]
        // User Association
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        // Complaint Details
        public string Subject { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime DateSubmitted { get; set; }

        [Required]
        public string Status { get; set; } = "Open"; // Open, In Progress, Resolved
    }
}

