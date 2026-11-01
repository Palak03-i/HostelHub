namespace Hostel_hub.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalHostels { get; set; }

        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int FullRooms { get; set; }

        public int TotalComplaints { get; set; }
        public int PendingComplaints { get; set; }
        public int AssignedComplaints { get; set; }
        public int InProgressComplaints { get; set; }
        public int ResolvedComplaints { get; set; }

        public int HighPriorityComplaints { get; set; }
        public int MediumPriorityComplaints { get; set; }
        public int LowPriorityComplaints { get; set; }

        public int TodayBreakfastTaking { get; set; }
        public int TodayLunchTaking { get; set; }
        public int TodayDinnerTaking { get; set; }

        public int ActiveAnnouncementsCount { get; set; }
    }
}