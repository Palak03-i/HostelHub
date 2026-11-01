using System.ComponentModel.DataAnnotations;

namespace Hostel_hub.ViewModels
{
    public class StudentEditProfileViewModel
    {
        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
    }
}