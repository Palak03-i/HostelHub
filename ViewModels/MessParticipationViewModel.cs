using Hostel_hub.Models;

namespace Hostel_hub.ViewModels
{
    public class MessParticipationViewModel
    {
        public int MenuId { get; set; }
        public int HostelId { get; set; }
        public string HostelName { get; set; } = string.Empty;
        public DateOnly MenuDate { get; set; }
        public int TotalStudents { get; set; }
        public List<MealParticipationRow> Rows { get; set; } = new();
    }

    public class MealParticipationRow
    {
        public MealType MealType { get; set; }
        public bool IsAvailable { get; set; }
        public int Taking { get; set; }
        public int Skipping { get; set; }
        public int NoResponse { get; set; }
    }
}