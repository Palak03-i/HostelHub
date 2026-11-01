using System.ComponentModel.DataAnnotations;
using Hostel_hub.Models;

namespace Hostel_hub.ViewModels
{
    public class CreateHostelWithWardenViewModel
    {
        // Hostel information
        [Required]
        [MaxLength(100)]
        public string HostelName { get; set; } = string.Empty;

        [Required]
        public HostelType HostelType { get; set; }

        // Warden information
        [Required]
        [MaxLength(150)]
        [Display(Name = "Warden Full Name")]
        public string WardenFullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        [Display(Name = "Warden Email")]
        public string WardenEmail { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        [Display(Name = "Temporary Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}