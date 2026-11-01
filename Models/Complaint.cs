using System.ComponentModel.DataAnnotations;

namespace Hostel_hub.Models
{
    public enum ComplaintPriority { Low, Medium, High }
    public enum ComplaintStatus { Pending, Assigned, InProgress, Resolved }
    public enum ComplaintCategory { Electrical, Plumbing, Furniture, WaterLeakage, Internet, Cleaning, Other }

    public class Complaint
    {
        public int ComplaintId { get; set; }

        [Required]
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public ComplaintPriority Priority { get; set; }

        [Required]
        public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending;

        [Required]
        public ComplaintCategory Category { get; set; }

        public DateTime? ResolvedDate { get; set; }

        public int? AssignedStaffId { get; set; }
        public MaintenanceStaff? AssignedStaff { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}