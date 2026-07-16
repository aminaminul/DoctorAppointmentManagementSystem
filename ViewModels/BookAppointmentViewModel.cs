using System;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentManagementSystem.ViewModels
{
    public class BookAppointmentViewModel
    {
        public string? Department { get; set; }

        [Required(ErrorMessage = "Please select a doctor.")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Please select a date.")]
        public DateTime Date { get; set; }

        public string? TimeSlot { get; set; }

        public string? ReasonForVisit { get; set; }
        public bool IsEmergency { get; set; }

        public string? DoctorName { get; set; }
        public string? DoctorSpecialization { get; set; }
        public decimal ConsultationFee { get; set; }
        public int Experience { get; set; }
        public string? ProfileImage { get; set; }
    }
}
