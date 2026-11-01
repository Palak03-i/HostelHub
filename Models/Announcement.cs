using System.ComponentModel.DataAnnotations;

namespace Hostel_hub.Models
{
    public class Announcement
    {
        public int AnnouncementId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int PostedByUserId { get; set; }
        public User? PostedByUser { get; set; }
        public int? HostelId { get; set; }
        public Hostel? Hostel { get; set; }

        public DateTime PostedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateOnly PublishDate { get; set; }

        [Required]
        public DateOnly ExpiryDate { get; set; }

        public bool IsImportant { get; set; } = false;

        // Soft delete: Admin "removing" an announcement sets this false
        // rather than deleting the row, preserving the audit trail.
        public bool IsActive { get; set; } = true;
    }
}