using System.ComponentModel.DataAnnotations;

namespace Hostel_hub.Models
{
    public enum FeedbackSource { Complaint, Mess, General }

    public class Feedback
    {
        public int FeedbackId { get; set; }

        [Required]
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        [Required]
        public FeedbackSource Source { get; set; }

        // Only set when Source == Complaint; null otherwise.
        public int? ComplaintId { get; set; }
        public Complaint? Complaint { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string? Comments { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}