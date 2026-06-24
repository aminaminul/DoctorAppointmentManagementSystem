namespace DoctorAppointmentManagementSystem.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }

        public User User { get; set; }
    }
}
