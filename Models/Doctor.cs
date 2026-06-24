using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppointmentManagementSystem.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        
        [Required]
        public string Specialization { get; set; }
        
        [NotMapped]
        public string Availability
        {
            get => AvailableTime ?? "";
            set => AvailableTime = value;
        }
        
        public string? Qualification { get; set; }
        
        public int Experience { get; set; }
        
        public decimal ConsultationFee { get; set; }
        
        public string? AvailableDays { get; set; }
        
        public string? AvailableTime { get; set; }
        
        public string? ProfileImage { get; set; }
        
        public bool ActiveStatus { get; set; } = true;

        public User User { get; set; }
    }
}
