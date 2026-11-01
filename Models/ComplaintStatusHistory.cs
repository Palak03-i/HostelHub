using System.ComponentModel.DataAnnotations;

namespace Hostel_hub.Models
{
    public class ComplaintStatusHistory
    {
        public int ComplaintStatusHistoryId { get; set; }

        [Required]
        public int ComplaintId { get; set; }
        public Complaint? Complaint { get; set; }

        [Required]
        public ComplaintStatus OldStatus { get; set; }

        [Required]
        public ComplaintStatus NewStatus { get; set; }

        [Required]
        [MaxLength(150)]
        public string ChangedBy { get; set; } = string.Empty;

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Remarks { get; set; }
    }
}