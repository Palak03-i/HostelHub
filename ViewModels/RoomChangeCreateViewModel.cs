using System.ComponentModel.DataAnnotations;

namespace Hostel_hub.ViewModels
{
    public class RoomChangeCreateViewModel
    {
        [Required]
        public int RequestedRoomId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}