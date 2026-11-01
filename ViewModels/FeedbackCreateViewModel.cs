using System.ComponentModel.DataAnnotations;
using Hostel_hub.Models;

namespace Hostel_hub.ViewModels
{
    public class FeedbackCreateViewModel
    {
        [Required]
        public FeedbackSource Source { get; set; }

        // Only required/used when Source == Complaint.
        public int? ComplaintId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string? Comments { get; set; }
    }
}