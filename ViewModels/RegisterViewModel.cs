using System.ComponentModel.DataAnnotations;
namespace DoctorAppointmentManagementSystem.ViewModels
{ 
public class RegisterViewModel
{
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email format")]

    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$",
        ErrorMessage = "Email must be a Gmail address (@gmail.com)")]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }

    public string Name { get; set; }
    public string Role { get; set; }
        [Required(ErrorMessage = "Please select gender")]
        public string Gender { get; set; }
    } }