using System.ComponentModel.DataAnnotations;

namespace Hostel_hub.Models
{
    public class Student
    {
        public int StudentId { get; set; }

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string RollNumber { get; set; } = string.Empty;

        [Phone]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public int? HostelId { get; set; }
        public Hostel? Hostel { get; set; }

        public int? RoomId { get; set; }
        public Room? Room { get; set; }
    }
}