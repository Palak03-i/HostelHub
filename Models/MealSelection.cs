using System.ComponentModel.DataAnnotations;

namespace Hostel_hub.Models
{
    public enum MealSelectionStatus { Take, Skip }

    public class MealSelection
    {
        public int MealSelectionId { get; set; }

        [Required]
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        [Required]
        public int MessMenuId { get; set; }
        public MessMenu? MessMenu { get; set; }

        [Required]
        public MealType MealType { get; set; }

        [Required]
        public MealSelectionStatus Status { get; set; }

        public DateTime SelectedAt { get; set; } = DateTime.UtcNow;
    }
}