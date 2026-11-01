using System.ComponentModel.DataAnnotations;

namespace Hostel_hub.Models
{
    public enum HostelType
    {
        Boys,
        Girls,
        Mixed
    }

    public class Hostel
    {
        public int HostelId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public HostelType Type { get; set; }

        // Navigation property: the "many" side of a one-to-many relationship.
        // A Hostel has many Rooms. This ICollection is what lets us write
        // hostel.Rooms.Count() instead of storing a separate counter.
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}