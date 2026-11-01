using System.ComponentModel.DataAnnotations;

namespace Hostel_hub.Models
{
    public class MessMenuItem
    {
        public int MessMenuItemId { get; set; }

        [Required]
        public int MessMenuId { get; set; }
        public MessMenu? MessMenu { get; set; }

        [Required]
        public MealType MealType { get; set; }

        [Required]
        [MaxLength(100)]
        public string ItemName { get; set; } = string.Empty;
    }
}