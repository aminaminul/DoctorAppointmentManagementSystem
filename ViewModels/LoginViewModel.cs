using System.ComponentModel.DataAnnotations;
namespace DoctorAppointmentManagementSystem.ViewModels


{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [RegularExpression(@"^[^@\s]+@gmail\.com$", ErrorMessage = "Email must be a Gmail address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}