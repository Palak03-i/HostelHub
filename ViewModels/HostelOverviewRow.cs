namespace Hostel_hub.ViewModels
{
    public class HostelOverviewRow
    {
        public int HostelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int FullRooms { get; set; }
        public int OpenComplaints { get; set; }
        public int ResolvedComplaints { get; set; }
        public int TodayMealsTaking { get; set; }
    }
}