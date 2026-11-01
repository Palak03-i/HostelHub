using System.ComponentModel.DataAnnotations;

namespace Hostel_hub.Models
{
    public enum MealType { Breakfast, Lunch, Dinner }

    public class MessMenu
    {
        public int MessMenuId { get; set; }

        [Required]
        public int HostelId { get; set; }
        public Hostel? Hostel { get; set; }

        [Required]
        public DateOnly MenuDate { get; set; }

        public bool IsBreakfastAvailable { get; set; } = true;
        public bool IsLunchAvailable { get; set; } = true;
        public bool IsDinnerAvailable { get; set; } = true;

        [Required]
        public int CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<MessMenuItem> Items { get; set; } = new List<MessMenuItem>();
    }
}