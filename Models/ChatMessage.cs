using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppointmentManagementSystem.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SenderUserId { get; set; }

        [ForeignKey("SenderUserId")]
        public User? SenderUser { get; set; }

        [Required]
        public int ReceiverUserId { get; set; }

        [ForeignKey("ReceiverUserId")]
        public User? ReceiverUser { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;
    }
}
