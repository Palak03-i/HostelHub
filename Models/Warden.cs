using System.ComponentModel.DataAnnotations;

namespace Hostel_hub.Models
{
    public class Warden
    {
        public int WardenId { get; set; }

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        public int? HostelId { get; set; }
        public Hostel? Hostel { get; set; }
    }
}