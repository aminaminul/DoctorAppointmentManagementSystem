using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppointmentManagementSystem.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Optional longer description for role (some views/controllers reference this)
        public string Description { get; set; }

        [NotMapped]
        public string RoleName
        {
            get => Name;
            set => Name = value;
        }
    }
}
