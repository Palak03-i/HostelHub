using System.ComponentModel.DataAnnotations;

namespace Hostel_hub.Models
{
    public enum StaffSpecialization
    {
        Plumbing,
        Electrical,
        Carpentry,
        Cleaning,
        General
    }

    public class MaintenanceStaff
    {
        public int MaintenanceStaffId { get; set; }

        [Required]
        public int UserId { get; set; }
        public int? HostelId { get; set; }
        public Hostel? Hostel { get; set; }
        public User? User { get; set; }

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        public StaffSpecialization Specialization { get; set; }

        public bool IsActive { get; set; } = true;
    }
}