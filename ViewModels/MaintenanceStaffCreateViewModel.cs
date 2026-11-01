using System.ComponentModel.DataAnnotations;
using Hostel_hub.Models;

namespace Hostel_hub.ViewModels
{
    public class MaintenanceStaffCreateViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        [Required]
        public StaffSpecialization Specialization { get; set; }
    }
}