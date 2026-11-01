using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hostel_hub.Models
{
    public enum RoomChangeStatus { Pending, Approved, Rejected }

    public class RoomChangeRequest
    {
        public int RoomChangeRequestId { get; set; }

        [Required]
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        [Required]
        public int CurrentRoomId { get; set; }
        [ForeignKey(nameof(CurrentRoomId))]
        public Room? CurrentRoom { get; set; }

        [Required]
        public int RequestedRoomId { get; set; }
        [ForeignKey(nameof(RequestedRoomId))]
        public Room? RequestedRoom { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public RoomChangeStatus Status { get; set; } = RoomChangeStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    }
}