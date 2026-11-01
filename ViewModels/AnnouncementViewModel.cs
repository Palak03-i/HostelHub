using System.ComponentModel.DataAnnotations;

namespace Hostel_hub.ViewModels
{
    public class AnnouncementViewModel
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateOnly PublishDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required]
        [DataType(DataType.Date)]
        public DateOnly ExpiryDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

        public bool IsImportant { get; set; }

        // Only meaningful for Super Admin (chooses a hostel, or leaves null for Global).
        // A Warden's submitted value here is ignored server-side.
        public int? HostelId { get; set; }
    }
}