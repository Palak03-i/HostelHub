using System.ComponentModel.DataAnnotations;

namespace Hostel_hub.Models
{
    public enum UserRole
    {
        Admin,
        Student,
        MaintenanceStaff
    }

    public class User
    {
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}