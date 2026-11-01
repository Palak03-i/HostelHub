using System.ComponentModel.DataAnnotations;

namespace Hostel_hub.Models
{
    public class Room
    {
        public int RoomId { get; set; }

        [Required]
        [MaxLength(20)]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        public int HostelId { get; set; }
        public Hostel? Hostel { get; set; }

        [Required]
        [Range(1, 20)]
        public int Capacity { get; set; }

        public int CurrentOccupancy { get; set; } = 0;
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        // Navigation: reverse side of Student.RoomId — lets us write
        // room.Students to see who's currently allocated here.
        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}