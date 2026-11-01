using Hostel_hub.Models;

namespace Hostel_hub.ViewModels
{
    public class StaffManagementViewModel
    {
        public bool IsSuperAdmin { get; set; }

        public string? StatusFilter { get; set; }

        public List<Warden> Wardens { get; set; }
            = new List<Warden>();

        public List<MaintenanceStaff> MaintenanceStaff { get; set; }
            = new List<MaintenanceStaff>();
    }
}