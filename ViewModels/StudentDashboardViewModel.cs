using Hostel_hub.Models;

namespace Hostel_hub.ViewModels
{
    public class StudentDashboardViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string? HostelName { get; set; }
        public string? RoomNumber { get; set; }
        public int? RoomCapacity { get; set; }
        public int? RoomCurrentOccupancy { get; set; }

        public MessMenu? TodayMenu { get; set; }
        public Dictionary<MealType, MealSelectionStatus> TodaySelections { get; set; } = new();

        public List<Complaint> ActiveComplaints { get; set; } = new();
        public Complaint? LatestComplaint { get; set; }

        public List<Announcement> RecentAnnouncements { get; set; } = new();

        public RoomChangeRequest? PendingRoomChangeRequest { get; set; }
    }
}