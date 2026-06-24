namespace DoctorAppointmentManagementSystem.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Specialization { get; set; }
        public string Availability { get; set; }

        public User User { get; set; }
    }
}
