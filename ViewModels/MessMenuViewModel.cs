using System.ComponentModel.DataAnnotations;

namespace Hostel_hub.ViewModels
{
    public class MessMenuViewModel
    {
        [Required]
        public int HostelId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly MenuDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        public bool IsBreakfastAvailable { get; set; } = true;
        public bool IsLunchAvailable { get; set; } = true;
        public bool IsDinnerAvailable { get; set; } = true;

        [Display(Name = "Breakfast items (one per line)")]
        public string BreakfastItemsText { get; set; } = string.Empty;

        [Display(Name = "Lunch items (one per line)")]
        public string LunchItemsText { get; set; } = string.Empty;

        [Display(Name = "Dinner items (one per line)")]
        public string DinnerItemsText { get; set; } = string.Empty;
    }
}