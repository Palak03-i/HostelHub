using System.ComponentModel.DataAnnotations;
using Hostel_hub.Models;

namespace Hostel_hub.ViewModels
{
    public class MaintenanceStaffEditViewModel
    {
        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        [Required]
        public StaffSpecialization Specialization { get; set; }
    }
}