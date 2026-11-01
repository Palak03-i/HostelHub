using System.ComponentModel.DataAnnotations;

namespace Hostel_hub.ViewModels
{
    public class RoomViewModel
    {
        [Required]
        public int HostelId { get; set; }

        [Required]
        [MaxLength(20)]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        [Range(1, 20)]
        public int Capacity { get; set; }
    }
}