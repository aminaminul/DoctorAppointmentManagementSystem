namespace DoctorAppointmentManagementSystem.Models
{
    public class Prescription
    {
        public int Id { get; set; }

        public int AppointmentId { get; set; }

        public string Medicines { get; set; }

        public string Advice { get; set; }

        public DateTime CreatedDate { get; set; }

        // Navigation
        public Appointment Appointment { get; set; }
    }
}
