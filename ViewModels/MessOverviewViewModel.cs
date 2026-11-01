using Hostel_hub.Models;

namespace Hostel_hub.ViewModels
{
    public class MessOverviewViewModel
    {
        public MessMenu? TodayMenu { get; set; }
        public MessMenu? TomorrowMenu { get; set; }
        public Dictionary<MealType, MealSelectionStatus> TodaySelections { get; set; } = new();
        public Dictionary<MealType, MealSelectionStatus> TomorrowSelections { get; set; } = new();
        public string? NoticeMessage { get; set; }
        public Dictionary<MealType, bool> TodayMealOpen { get; set; }
    = new();

        public Dictionary<MealType, bool> TomorrowMealOpen { get; set; }
            = new();
    }
}