using Hostel_hub.Models;

namespace Hostel_hub.ViewModels
{
    public class StaffDashboardViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public StaffSpecialization Specialization { get; set; }

        public int TotalAssigned { get; set; }
        public int PendingCount { get; set; }
        public int InProgressCount { get; set; }
        public List<Complaint> RecentlyResolved { get; set; } = new();
    }
}